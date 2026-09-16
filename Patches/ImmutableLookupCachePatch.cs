using Terraria.ModLoader;

namespace CalamityMod.Patches;

internal static class ImmutableLookupCachePatch
{
    private static bool enabled;

    public static bool Enabled => enabled;
    public static string Compatibility => CalamityImmutableLookupCacheComponent.Compatibility;
    public static long MiracleHits => CalamityImmutableLookupCacheComponent.MiracleHits;
    public static long MusicHits => CalamityImmutableLookupCacheComponent.MusicHits;
    public static long MusicMisses => CalamityImmutableLookupCacheComponent.MusicMisses;
    public static string Mode => enabled ? "internal-callsite-immutable-snapshots" : "original-allocation";

    public static void Load(Mod mod)
    {
        CalamityImmutableLookupCacheComponent.Load(mod);
        enabled = false;
        ResetCounters();
    }

    public static void PostSetupContent() => CalamityImmutableLookupCacheComponent.PostSetupContent();

    public static void Unload()
    {
        SetEnabled(false);
        CalamityImmutableLookupCacheComponent.Unload();
    }

    public static void SetEnabled(bool value)
    {
        enabled = value && CalamityImmutableLookupCacheComponent.SetEnabled(true);
        if (!value)
            CalamityImmutableLookupCacheComponent.SetEnabled(false);
        ResetCounters();
    }

    public static void ResetCounters() => CalamityImmutableLookupCacheComponent.ResetCounters();
}
