using System;
using System.Reflection;
using CalamityMod.NPCs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// CalamityGlobalNPC.PreAI decrements all 256 per-player dash immunity slots
/// for every NPC. In strict Singleplayer only Main.myPlayer can ever be
/// written, so the original loop can stop immediately after that slot.
/// Multiplayer keeps Calamity's exact 256-slot bound.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class DashImmunitySingleplayerFastPatch
{
    private static readonly FieldInfo MainNetModeField = typeof(Main).GetField(
        nameof(Main.netMode), BindingFlags.Public | BindingFlags.Static)
        ?? throw new MissingFieldException(typeof(Main).FullName, nameof(Main.netMode));
    private static readonly FieldInfo MainMyPlayerField = typeof(Main).GetField(
        nameof(Main.myPlayer), BindingFlags.Public | BindingFlags.Static)
        ?? throw new MissingFieldException(typeof(Main).FullName, nameof(Main.myPlayer));

    private static Mod? owner;
    private static MethodInfo? preAiMethod;
    private static FieldInfo? dashField;
    private static ILHook? hook;
    private static bool enabled;
    private static bool available;
    private static string compatibility = "content-not-resolved";
    private static long calls;
    private static long visitedSlots;
    private static long skippedSlots;

    public static bool Enabled => enabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static string Mode => enabled ? "direct-il-singleplayer-local-slot;multiplayer-original-256" : "calamity-original-256";
    public static long Calls => calls;
    public static long VisitedSlots => visitedSlots;
    public static long SkippedSlots => skippedSlots;

    public static void Load(Mod mod)
    {
        owner = mod;
        preAiMethod = null;
        dashField = null;
        hook = null;
        enabled = false;
        available = false;
        compatibility = "content-not-resolved";
        ResetCounters();
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
            Type type = typeof(CalamityGlobalNPC);
            preAiMethod = type.GetMethod(nameof(GlobalNPC.PreAI), BindingFlags.Instance | BindingFlags.Public,
                binder: null, types: [typeof(NPC)], modifiers: null)
                ?? throw new MissingMethodException(type.FullName, nameof(GlobalNPC.PreAI));
            dashField = type.GetField("dashImmunityTime", BindingFlags.Instance | BindingFlags.Public)
                ?? throw new MissingFieldException(type.FullName, "dashImmunityTime");

            using ILHook validation = new(preAiMethod, ReplaceAuditedLoopBound);
            available = true;
            compatibility = "audited-calamity-2.2.2-globalnpc-preai-dash-loop";
            owner?.Logger.Info("Dash immunity Singleplayer loop bound validated; state is OFF.");
        }
        catch (Exception exception)
        {
            available = false;
            compatibility = $"anchor-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Warn($"Dash immunity Singleplayer fast path unavailable: {exception}");
        }
    }

    public static void Unload()
    {
        SetEnabled(false);
        preAiMethod = null;
        dashField = null;
        available = false;
        compatibility = "unloaded";
        owner = null;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        bool requested = value && available;
        if (requested == enabled)
        {
            ResetCounters();
            return;
        }

        hook?.Dispose();
        hook = null;
        enabled = false;
        if (requested)
        {
            try
            {
                hook = new ILHook(preAiMethod!, ReplaceAuditedLoopBound);
                enabled = true;
            }
            catch (Exception exception)
            {
                hook?.Dispose();
                hook = null;
                available = false;
                compatibility = $"enable-failed:{exception.GetType().Name}:{exception.Message}";
                owner?.Logger.Warn($"Dash immunity Singleplayer fast path failed to enable: {exception}");
            }
        }
        ResetCounters();
    }

    public static void ResetCounters()
    {
        calls = 0;
        visitedSlots = 0;
        skippedSlots = 0;
    }

    private static void ReplaceAuditedLoopBound(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdfld(dashField!)))
            throw new InvalidOperationException("dashImmunityTime field load was not found.");
        if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchLdcI4(256)))
            throw new InvalidOperationException("dashImmunityTime loop bound 256 was not found after the field load.");
        cursor.Remove();

        // Build the bound directly in IL. The first R7 prototype called a
        // delegate and updated three 64-bit counters for every NPC, which was
        // measurable overhead in the same hot path being optimized.
        ILLabel multiplayer = cursor.DefineLabel();
        ILLabel done = cursor.DefineLabel();
        cursor.Emit(OpCodes.Ldsfld, MainNetModeField);
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Bne_Un, multiplayer);
        cursor.Emit(OpCodes.Ldsfld, MainMyPlayerField);
        cursor.Emit(OpCodes.Ldc_I4_1);
        cursor.Emit(OpCodes.Add);
        cursor.Emit(OpCodes.Br, done);
        cursor.MarkLabel(multiplayer);
        cursor.Emit(OpCodes.Ldc_I4, 256);
        cursor.MarkLabel(done);

        if (cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdfld(dashField!)))
            throw new InvalidOperationException("Unexpected second dashImmunityTime field load in PreAI.");
    }
}
