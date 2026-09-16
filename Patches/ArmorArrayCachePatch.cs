using System;
using System.Reflection;
using System.Threading;
using CalamityMod.Items.VanillaArmorChanges;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class ArmorArrayCachePatch
{
    private static readonly int[] GoldAlternativeHeads = { 955 };
    private static ILHook? hook;
    private static volatile bool enabled;
    private static long hits;

    public static bool Enabled => enabled;
    public static long Hits => Interlocked.Read(ref hits);

    public static void Load(Mod owner)
    {
        MethodInfo getter = typeof(GoldArmorSetChange).GetProperty(nameof(GoldArmorSetChange.AlternativeHeadPieceIDs), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)?.GetMethod
            ?? throw new MissingMethodException(typeof(GoldArmorSetChange).FullName, "get_AlternativeHeadPieceIDs");
        hook = new ILHook(getter, InjectCachedReturn);
        enabled = false;
        ResetCounters();
        owner.Logger.Info("Gold armor alternative-head array cache hook installed; state is OFF.");
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

    public static void ResetCounters() => Interlocked.Exchange(ref hits, 0);

    private static void InjectCachedReturn(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel continueOriginal = cursor.DefineLabel();
        cursor.EmitDelegate<Func<int[]?>>(GetCachedArray);
        cursor.Emit(OpCodes.Dup);
        cursor.Emit(OpCodes.Brfalse, continueOriginal);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(continueOriginal);
        cursor.Emit(OpCodes.Pop);
    }

    private static int[]? GetCachedArray()
    {
        if (!enabled)
            return null;
        CalamityPatchDiagnostics.Increment(ref hits);
        return GoldAlternativeHeads;
    }
}
