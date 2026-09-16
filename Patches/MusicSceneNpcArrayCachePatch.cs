using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using CalamityMod.Systems;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Calamity's music scene scan reads AdditionalNPCs twice per active NPC and
/// several overrides build a new array on every getter call. All definitions in
/// the pinned Calamity source contain immutable post-setup NPC type IDs, and the
/// sole consumer only reads Length and elements. Cache the exact returned array
/// once per scene-effect instance without changing scene selection logic.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class MusicSceneNpcArrayCachePatch
{
    private sealed class ArrayBox(int[] value)
    {
        public readonly int[] Value = value;
    }

    private static ConditionalWeakTable<BaseMusicSceneEffect, ArrayBox> cache = new();
    private static MethodInfo? setSceneEffectMethod;
    private static MethodInfo? getterMethod;
    private static ILHook? hook;
    private static Mod? owner;
    private static bool enabled;
    private static long hits;
    private static long misses;

    public static bool Enabled => enabled;
    public static long Hits => Interlocked.Read(ref hits);
    public static long Misses => Interlocked.Read(ref misses);
    public static string Mode => enabled ? "per-scene-exact-array" : "calamity-per-access";

    public static void Load(Mod mod)
    {
        owner = mod;
        setSceneEffectMethod = typeof(BaseMusicSceneEffect).GetMethod(
            nameof(BaseMusicSceneEffect.SetSceneEffect),
            BindingFlags.Instance | BindingFlags.Public)
            ?? throw new MissingMethodException(typeof(BaseMusicSceneEffect).FullName, nameof(BaseMusicSceneEffect.SetSceneEffect));
        getterMethod = typeof(BaseMusicSceneEffect).GetProperty(
            nameof(BaseMusicSceneEffect.AdditionalNPCs),
            BindingFlags.Instance | BindingFlags.Public)?.GetMethod
            ?? throw new MissingMethodException(typeof(BaseMusicSceneEffect).FullName, "get_AdditionalNPCs");
        mod.Logger.Info("Music scene AdditionalNPCs cache resolved; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        cache = new();
        setSceneEffectMethod = null;
        getterMethod = null;
        owner = null;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (value == enabled)
            return;

        if (value)
        {
            hook = new ILHook(setSceneEffectMethod!, ReplaceAdditionalNpcReads);
            enabled = true;
            owner?.Logger.Info("Music scene AdditionalNPCs arrays are now cached once per scene instance.");
        }
        else
        {
            hook?.Dispose();
            hook = null;
            enabled = false;
            cache = new();
            owner?.Logger.Info("Music scene AdditionalNPCs restored to Calamity per-access evaluation.");
        }

        ResetCounters();
    }

    public static void ResetCounters()
    {
        Interlocked.Exchange(ref hits, 0);
        Interlocked.Exchange(ref misses, 0);
    }

    private static void ReplaceAdditionalNpcReads(ILContext il)
    {
        ILCursor cursor = new(il);
        int replacements = 0;
        while (cursor.TryGotoNext(MoveType.Before,
                   instruction => instruction.MatchCallOrCallvirt(getterMethod!)))
        {
            cursor.Remove();
            cursor.EmitDelegate(GetCachedAdditionalNpcs);
            replacements++;
        }

        if (replacements != 2)
            throw new InvalidOperationException($"Expected two AdditionalNPCs reads in BaseMusicSceneEffect.SetSceneEffect, found {replacements}.");
    }

    private static int[] GetCachedAdditionalNpcs(BaseMusicSceneEffect scene)
    {
        if (cache.TryGetValue(scene, out ArrayBox? existing))
        {
            CalamityPatchDiagnostics.Increment(ref hits);
            return existing.Value;
        }

        ArrayBox created = cache.GetValue(scene, static key => new ArrayBox(key.AdditionalNPCs));
        CalamityPatchDiagnostics.Increment(ref misses);
        return created.Value;
    }
}
