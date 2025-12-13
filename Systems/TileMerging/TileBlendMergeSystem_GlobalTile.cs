using System.Diagnostics.CodeAnalysis;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        #region Don't mind this, This is a small GlobalTile for hooks
        [Autoload(Side = ModSide.Client)]
        private class FancyTileMergeGlobalTile : GlobalTile
        {
            [SuppressMessage("Simplify Method Call", "IDE0002", Justification = "Leave this alone for Consistency")]
            public override void PostSetupTileMerge()
            {
                TileBlendMergeSystem.SetupMergeData();
            }

            public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
            {
                if (CalamityClientConfig.Instance.TileTextureBlendingQuality == TileBlendingQuality.Disable)
                    return;

                if (CalamityTileSets.DrawBlendMergeAfterSolidTile[type])
                    return;

                if (!WorldGen.InWorld(i, j))
                    return;

                var tile = Main.tile[i, j];
                if (!tile.Get<TileSpecialDrawData>().HasBlendMergeData)
                    return;

                if (!TryGetBlendingRefData(i, j, out var blendRefs))
                    return;

                DrawOnTile(tile, i, j, in blendRefs);
            }

            public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
            {
                // Tomat: Naive way to address tile frame logic running on
                // multiple threads resulting in cryptic errors due to
                // SetBlendingRefData/_TileBlendingRefs not being threadsafe.
                // A more correct fix would be to lock or use a thread-safe
                // hashmap, but we don't actually create any scenarios aside
                // from worldgen where this would be ran on multiple threads.
                if (!WorldGen.generatingWorld)
                    TileBlendMergeSystem.TileFrame(i, j, type);

                return base.TileFrame(i, j, type, ref resetFrame, ref noBreak);
            }
        }
        #endregion
    }
}
