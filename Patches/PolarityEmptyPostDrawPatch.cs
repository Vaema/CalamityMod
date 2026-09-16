using System;
using System.Reflection;
using System.Threading;
using CalamityMod.NPCs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// CalamityPolarityNPC.PostDraw changes SpriteBatch state twice even when the
/// NPC has no polarity pulses to draw.  The original method is left completely
/// intact for every non-empty pulse list.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class PolarityEmptyPostDrawPatch
{
    private static ILHook? hook;
    private static volatile bool enabled;
    private static long checks;
    private static long skipped;

    public static bool Enabled => enabled;
    public static long Checks => Interlocked.Read(ref checks);
    public static long Skipped => Interlocked.Read(ref skipped);
    public static string Mode => enabled ? "empty-list-exact-return" : "calamity-original";

    public static void Load(Mod owner)
    {
        MethodInfo postDraw = typeof(CalamityPolarityNPC).GetMethod(
            nameof(CalamityPolarityNPC.PostDraw),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            ?? throw new MissingMethodException(typeof(CalamityPolarityNPC).FullName, nameof(CalamityPolarityNPC.PostDraw));

        hook = new ILHook(postDraw, InjectEmptyListReturn);
        enabled = false;
        ResetCounters();
        owner.Logger.Info("Polarity empty-PostDraw fast path installed; state is OFF.");
    }

    public static void Unload()
    {
        enabled = false;
        hook?.Dispose();
        hook = null;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        enabled = value;
        ResetCounters();
    }

    public static void ResetCounters()
    {
        Interlocked.Exchange(ref checks, 0);
        Interlocked.Exchange(ref skipped, 0);
    }

    private static void InjectEmptyListReturn(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel continueOriginal = cursor.DefineLabel();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(ShouldSkip);
        cursor.Emit(OpCodes.Brfalse, continueOriginal);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(continueOriginal);
    }

    private static bool ShouldSkip(CalamityPolarityNPC polarity)
    {
        if (!enabled)
            return false;

        CalamityPatchDiagnostics.Increment(ref checks);
        if (polarity.pulses is not { Count: 0 })
            return false;

        CalamityPatchDiagnostics.Increment(ref skipped);
        return true;
    }
}
