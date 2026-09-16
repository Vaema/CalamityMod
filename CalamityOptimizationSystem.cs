using System;
using CalamityMod.Patches;
using Terraria.ModLoader;

namespace CalamityMod;

internal sealed class CalamityOptimizationSystem : ModSystem
{
    private static readonly Action<Mod>[] LoadPatches =
    [
        ArmorArrayCachePatch.Load,
        PerTileFastPathPatch.Load,
        GlowMaskHookRelocationPatch.Load,
        ExoticRainbowTargetPoolPatch.Load,
        MetaballPreparePatch.Load,
        ProjectileBroadphaseShadowPatch.Load,
        BrimstoneBarrageCollisionFastPatch.Load,
        ExoPulseLaserCollisionFastPatch.Load,
        WormHeadAnyNpcCachePatch.Load,
        DashImmunitySingleplayerFastPatch.Load,
        DebuffDisplayListReusePatch.Load,
        UpdateLifeRegenFastPatch.Load,
        MusicSceneNpcArrayCachePatch.Load,
        ParticleTextureRequestCachePatch.Load,
        AuricEdgeTileListReusePatch.Load,
        PlayerMovementNpcClosureReusePatch.Load,
        ExoVortexDrawCachePatch.Load,
        PolarityEmptyPostDrawPatch.Load,
        BuffStationTileFastRejectPatch.Load,
        ImmutableLookupCachePatch.Load,
        BrimstoneHeartDrawCachePatch.Load,
        NebulaStarDrawCachePatch.Load
    ];

    private static readonly Action[] UnloadPatches =
    [
        NebulaStarDrawCachePatch.Unload,
        BrimstoneHeartDrawCachePatch.Unload,
        ImmutableLookupCachePatch.Unload,
        BuffStationTileFastRejectPatch.Unload,
        PolarityEmptyPostDrawPatch.Unload,
        ExoVortexDrawCachePatch.Unload,
        PlayerMovementNpcClosureReusePatch.Unload,
        AuricEdgeTileListReusePatch.Unload,
        ParticleTextureRequestCachePatch.Unload,
        MusicSceneNpcArrayCachePatch.Unload,
        UpdateLifeRegenFastPatch.Unload,
        DebuffDisplayListReusePatch.Unload,
        DashImmunitySingleplayerFastPatch.Unload,
        WormHeadAnyNpcCachePatch.Unload,
        ExoPulseLaserCollisionFastPatch.Unload,
        BrimstoneBarrageCollisionFastPatch.Unload,
        ProjectileBroadphaseShadowPatch.Unload,
        MetaballPreparePatch.Unload,
        ExoticRainbowTargetPoolPatch.Unload,
        GlowMaskHookRelocationPatch.Unload,
        PerTileFastPathPatch.Unload,
        ArmorArrayCachePatch.Unload
    ];

    public override void Load()
    {
        foreach (Action<Mod> load in LoadPatches)
            load(Mod);
    }

    public override void PostSetupContent()
    {
        GlowMaskHookRelocationPatch.PostSetupContent(Mod);
        ProjectileBroadphaseShadowPatch.PostSetupContent();
        BrimstoneBarrageCollisionFastPatch.PostSetupContent();
        ExoPulseLaserCollisionFastPatch.PostSetupContent();
        WormHeadAnyNpcCachePatch.PostSetupContent();
        DashImmunitySingleplayerFastPatch.PostSetupContent();
        DebuffDisplayListReusePatch.PostSetupContent();
        UpdateLifeRegenFastPatch.PostSetupContent();
        BuffStationTileFastRejectPatch.PostSetupContent();
        ImmutableLookupCachePatch.PostSetupContent();

        ArmorArrayCachePatch.SetEnabled(true);
        PerTileFastPathPatch.SetEnabled(true);
        GlowMaskHookRelocationPatch.SetEnabled(true);
        ExoticRainbowTargetPoolPatch.SetEnabled(true);
        MetaballPreparePatch.SetEnabled(true);
        ProjectileBroadphaseShadowPatch.SetEnabled(true);
        BrimstoneBarrageCollisionFastPatch.SetEnabled(true);
        ExoPulseLaserCollisionFastPatch.SetEnabled(true);
        WormHeadAnyNpcCachePatch.SetEnabled(true);
        DashImmunitySingleplayerFastPatch.SetEnabled(true);
        DebuffDisplayListReusePatch.SetEnabled(true);
        UpdateLifeRegenFastPatch.SetEnabled(true);
        MusicSceneNpcArrayCachePatch.SetEnabled(true);
        ParticleTextureRequestCachePatch.SetEnabled(true);
        AuricEdgeTileListReusePatch.SetEnabled(true);
        PlayerMovementNpcClosureReusePatch.SetEnabled(true);
        ExoVortexDrawCachePatch.SetEnabled(true);
        PolarityEmptyPostDrawPatch.SetEnabled(true);
        BuffStationTileFastRejectPatch.SetEnabled(true);
        ImmutableLookupCachePatch.SetEnabled(true);
        BrimstoneHeartDrawCachePatch.SetEnabled(true);
        NebulaStarDrawCachePatch.SetEnabled(true);
    }

    public override void Unload()
    {
        foreach (Action unload in UnloadPatches)
            unload();
    }
}
