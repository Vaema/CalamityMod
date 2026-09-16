using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.SupremeCalamitas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Calamity's BrimstoneHeart.PreDraw creates one List, its backing array, a
/// second array in PrimitiveRenderer.RenderTrail(List), and two bound delegates
/// for every chain on every frame. Replace only that audited method body with
/// the source-equivalent array overload and one cache per heart instance.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class BrimstoneHeartDrawCachePatch
{
    private sealed class DrawState
    {
        public readonly Vector2[] Points = new Vector2[5];
        public readonly PrimitiveSettings Settings;

        public DrawState(BrimstoneHeart heart)
        {
            Settings = new PrimitiveSettings(
                heart.PrimitiveWidthFunction,
                heart.PrimitiveColorFunction);
        }
    }

    private static ConditionalWeakTable<BrimstoneHeart, DrawState> states = new();
    private static MethodInfo? preDraw;
    private static ILHook? hook;
    private static Mod? owner;
    private static bool enabled;
    private static bool available = true;
    private static string compatibility = "resolved-not-installed";
    private static long drawCalls;
    private static long chainsRendered;

    public static bool Enabled => enabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static long DrawCalls => drawCalls;
    public static long ChainsRendered => chainsRendered;
    public static string Mode => enabled ? "fixed-five-point-buffer-plus-cached-settings" : "calamity-list-toarray-delegates";

    public static void Load(Mod mod)
    {
        owner = mod;
        preDraw = typeof(BrimstoneHeart).GetMethod(
            nameof(BrimstoneHeart.PreDraw),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(SpriteBatch), typeof(Vector2), typeof(Color)],
            modifiers: null)
            ?? throw new MissingMethodException(typeof(BrimstoneHeart).FullName, nameof(BrimstoneHeart.PreDraw));
        enabled = false;
        available = true;
        compatibility = "resolved-not-installed";
        ResetCounters();
        mod.Logger.Info("BrimstoneHeart draw allocation target resolved; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        preDraw = null;
        owner = null;
        states = new();
        available = true;
        compatibility = "unloaded";
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (!value)
        {
            enabled = false;
            hook?.Dispose();
            hook = null;
            states = new();
            compatibility = available ? "resolved-not-installed" : compatibility;
            ResetCounters();
            return;
        }

        if (enabled || !available)
            return;

        try
        {
            hook = new ILHook(preDraw!, InjectExactReplacement);
            enabled = true;
            compatibility = "audited-calamity-2.2.2";
            ResetCounters();
        }
        catch (Exception exception)
        {
            hook?.Dispose();
            hook = null;
            enabled = false;
            available = false;
            compatibility = $"il-anchor-failed:{exception.Message}";
            owner?.Logger.Error("BrimstoneHeart draw allocation patch was disabled.", exception);
        }
    }

    public static void ResetCounters()
    {
        drawCalls = 0;
        chainsRendered = 0;
    }

    private static void InjectExactReplacement(ILContext il)
    {
        int listConstructions = il.Body.Instructions.Count(IsVector2ListConstructor);
        int listRenderCalls = il.Body.Instructions.Count(IsVector2ListRenderTrail);
        int widthDelegates = il.Body.Instructions.Count(instruction => IsDelegateConstructor(instruction, "VertexWidthFunction"));
        int colorDelegates = il.Body.Instructions.Count(instruction => IsDelegateConstructor(instruction, "VertexColorFunction"));

        if (listConstructions != 1 || listRenderCalls != 1 || widthDelegates != 1 || colorDelegates != 1)
        {
            throw new InvalidOperationException(
                $"BrimstoneHeart.PreDraw no longer matches the audited allocation sequence: list={listConstructions}, renderList={listRenderCalls}, width={widthDelegates}, color={colorDelegates}.");
        }

        ILCursor cursor = new(il);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(DrawWithoutTemporaryCollections);
        cursor.Emit(OpCodes.Ret);
    }

    private static bool IsVector2ListConstructor(Instruction instruction) =>
        instruction.OpCode == OpCodes.Newobj &&
        instruction.Operand is MethodReference method &&
        method.Name == ".ctor" &&
        method.DeclaringType.FullName == "System.Collections.Generic.List`1<Microsoft.Xna.Framework.Vector2>";

    private static bool IsVector2ListRenderTrail(Instruction instruction) =>
        instruction.OpCode == OpCodes.Call &&
        instruction.Operand is MethodReference method &&
        method.Name == nameof(PrimitiveRenderer.RenderTrail) &&
        method.Parameters.Count >= 1 &&
        method.Parameters[0].ParameterType.FullName == "System.Collections.Generic.List`1<Microsoft.Xna.Framework.Vector2>";

    private static bool IsDelegateConstructor(Instruction instruction, string delegateName) =>
        instruction.OpCode == OpCodes.Newobj &&
        instruction.Operand is MethodReference method &&
        method.Name == ".ctor" &&
        method.DeclaringType.Name == delegateName;

    private static bool DrawWithoutTemporaryCollections(BrimstoneHeart heart, SpriteBatch spriteBatch)
    {
        CalamityPatchDiagnostics.Increment(ref drawCalls);
        NPC npc = heart.NPC;
        if (npc.IsABestiaryIconDummy)
            return true;

        CalamityUtils.ExitShaderRegion(spriteBatch);
        DrawState state = states.GetValue(heart, static instance => new DrawState(instance));
        Vector2[] points = state.Points;

        for (int i = 0; i < heart.ChainEndpoints.Count; i++)
        {
            Vector2 endpoint = heart.ChainEndpoints[i];
            float distance = npc.Distance(endpoint);
            Vector2 direction = npc.DirectionTo(endpoint);
            for (int j = 0; j < 4; j++)
                points[j] = npc.Center + direction * distance * 0.25f * j;
            points[4] = endpoint + direction * 18f;

            PrimitiveRenderer.RenderTrail(points, state.Settings, 40);
            CalamityPatchDiagnostics.Increment(ref chainsRendered);
        }

        return true;
    }
}
