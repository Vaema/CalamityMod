using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// CalamityGlobalNPC.PreDraw creates a temporary List&lt;Texture2D&gt; for every
/// boss/miniboss draw, including every visible worm segment. The list is fully
/// consumed before PreDraw returns and never escapes. Reuse one list per render
/// thread without changing any predicate, texture, icon order, position, or
/// summon-tag scan.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class DebuffDisplayListReusePatch
{
    [ThreadStatic]
    private static List<Texture2D>? threadTextures;

    private static Mod? owner;
    private static MethodInfo? preDraw;
    private static ILHook? hook;
    private static bool enabled;
    private static bool available;
    private static string compatibility = "content-not-resolved";

    public static bool Enabled => enabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static string Mode => enabled
        ? "thread-local-list-reuse-plus-direct-count"
        : "calamity-new-list-per-draw";

    public static void Load(Mod mod)
    {
        owner = mod;
        preDraw = null;
        hook = null;
        enabled = false;
        available = false;
        compatibility = "content-not-resolved";
        threadTextures = null;
    }

    public static void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("CalamityMod", out Mod? calamity) || calamity.Version.ToString() != "2.2.2")
        {
            compatibility = "requires-calamity-2.2.2";
            return;
        }

        try
        {
            preDraw = typeof(CalamityGlobalNPC).GetMethod(
                nameof(GlobalNPC.PreDraw), BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: [typeof(NPC), typeof(SpriteBatch), typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Color)],
                modifiers: null)
                ?? throw new MissingMethodException(typeof(CalamityGlobalNPC).FullName, nameof(GlobalNPC.PreDraw));

            using ILHook validation = new(preDraw, ReplaceAuditedAllocations);
            available = true;
            compatibility = "audited-calamity-2.2.2-globalnpc-predraw-debuffdisplay-list-and-count";
            owner?.Logger.Info("DebuffDisplay list reuse and direct Count anchors validated; state is OFF.");
        }
        catch (Exception exception)
        {
            available = false;
            compatibility = $"anchor-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Warn($"DebuffDisplay list reuse unavailable: {exception}");
        }
    }

    public static void Unload()
    {
        SetEnabled(false);
        preDraw = null;
        available = false;
        compatibility = "unloaded";
        threadTextures = null;
        owner = null;
    }

    public static void SetEnabled(bool value)
    {
        bool requested = value && available;
        if (requested == enabled)
            return;

        hook?.Dispose();
        hook = null;
        enabled = false;
        if (!requested)
            return;

        try
        {
            hook = new ILHook(preDraw!, ReplaceAuditedAllocations);
            enabled = true;
        }
        catch (Exception exception)
        {
            hook?.Dispose();
            hook = null;
            available = false;
            compatibility = $"enable-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Warn($"DebuffDisplay list reuse failed to enable: {exception}");
        }
    }

    private static void ReplaceAuditedAllocations(ILContext il)
    {
        MethodInfo rent = typeof(DebuffDisplayListReusePatch).GetMethod(
            nameof(GetTextureBuffer), BindingFlags.Static | BindingFlags.Public)
            ?? throw new MissingMethodException(typeof(DebuffDisplayListReusePatch).FullName, nameof(GetTextureBuffer));
        FieldInfo debuffListField = typeof(CalamityGlobalNPC).GetField(
            "moddedDebuffTextureList", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(CalamityGlobalNPC).FullName, "moddedDebuffTextureList");
        MethodInfo directCount = debuffListField.FieldType.GetProperty(nameof(List<int>.Count))?.GetMethod
            ?? throw new MissingMethodException(debuffListField.FieldType.FullName, "get_Count");
        MethodReference importedRent = il.Method.Module.ImportReference(rent);
        MethodReference importedCount = il.Method.Module.ImportReference(directCount);
        ILCursor cursor = new(il);
        int listReplacements = 0;
        while (cursor.TryGotoNext(MoveType.Before, IsTextureListConstructor))
        {
            // Three conditional branches target this exact instruction. Mutate
            // it in place so their targets remain valid; removing it leaves the
            // branch operands pointing at a detached instruction and the CLR
            // rejects the method as InvalidProgram.
            Instruction allocation = cursor.Next!;
            allocation.OpCode = OpCodes.Call;
            allocation.Operand = importedRent;
            cursor.Index++;
            listReplacements++;
        }

        if (listReplacements != 1)
            throw new InvalidOperationException($"Expected one texture-list constructor in CalamityGlobalNPC.PreDraw; found {listReplacements}.");

        cursor.Index = 0;
        int countReplacements = 0;
        while (cursor.TryGotoNext(MoveType.Before, IsModdedDebuffEnumerableCount))
        {
            // The source collection is statically List<T>. Enumerable.Count<T>
            // performs interface/type dispatch on every loop condition. Calling
            // List<T>.Count directly returns the same integer without that work.
            Instruction countCall = cursor.Next!;
            countCall.OpCode = OpCodes.Callvirt;
            countCall.Operand = importedCount;
            cursor.Index++;
            countReplacements++;
        }

        if (countReplacements != 1)
            throw new InvalidOperationException($"Expected one modded-debuff Enumerable.Count call in CalamityGlobalNPC.PreDraw; found {countReplacements}.");
    }

    private static bool IsTextureListConstructor(Instruction instruction) =>
        instruction.OpCode == OpCodes.Newobj &&
        instruction.Operand is MethodReference method &&
        method.Name == ".ctor" &&
        method.Parameters.Count == 0 &&
        method.DeclaringType.FullName == "System.Collections.Generic.List`1<Microsoft.Xna.Framework.Graphics.Texture2D>";

    private static bool IsModdedDebuffEnumerableCount(Instruction instruction) =>
        instruction.OpCode == OpCodes.Call &&
        instruction.Operand is GenericInstanceMethod method &&
        method.Name == "Count" &&
        method.DeclaringType.FullName == "System.Linq.Enumerable" &&
        method.GenericArguments.Count == 1 &&
        method.GenericArguments[0].FullName ==
            "System.ValueTuple`2<System.String,System.Predicate`1<Terraria.NPC>>";

    // Called directly from the patched Calamity method, so this must remain
    // public for cross-assembly accessibility.
    public static List<Texture2D> GetTextureBuffer()
    {
        List<Texture2D> textures = threadTextures ??= new List<Texture2D>(16);
        textures.Clear();
        return textures;
    }
}
