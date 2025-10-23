using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        // Blending Refs for every tiles
        private static readonly Dictionary<int, TileBlendingRef[]> _TileBlendingRefs = [];

        public override void ClearWorld()
        {
            _TileBlendingRefs.Clear();
        }

        public static void RemoveBlendingRefData(int tileX, int tileY)
        {
            if (!WorldGen.InWorld(tileX, tileY))
                return;

            var tileIdx = tileX + (Main.tile.Width * tileY);
            _TileBlendingRefs.Remove(tileIdx);
        }

        public static void SetBlendingRefData(int tileX, int tileY, TileBlendingRef[] blendingRef)
        {
            if (!WorldGen.InWorld(tileX, tileY))
                return;

            var tileIdx = tileX + (Main.tile.Width * tileY);
            _TileBlendingRefs[tileIdx] = blendingRef;
        }

        public static bool TryGetBlendingRefData(int tileX, int tileY, out TileBlendingRef[] blendingRefs)
        {
            if (!WorldGen.InWorld(tileX, tileY))
            {
                blendingRefs = null;
                return false;
            }

            var tileIdx = tileX + (Main.tile.Width * tileY);
            return _TileBlendingRefs.TryGetValue(tileIdx, out blendingRefs);
        }
    }
}
