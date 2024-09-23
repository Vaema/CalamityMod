using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static void TileFrame(int i, int j, int type)
        {
            if (!InValidRange(i, j))
                return;

            var slot = i + (_MaxTilesX * j);

            var tile =      Main.tile[i, j];
            var left =      CalamityUtils.ParanoidTileRetrieval(i - 1, j);
            var right =     CalamityUtils.ParanoidTileRetrieval(i + 1, j);

            var up =        CalamityUtils.ParanoidTileRetrieval(i, j - 1);
            var upLeft =    CalamityUtils.ParanoidTileRetrieval(i - 1, j - 1);
            var upRight =   CalamityUtils.ParanoidTileRetrieval(i + 1, j - 1);

            var down =      CalamityUtils.ParanoidTileRetrieval(i, j + 1);
            var downLeft =  CalamityUtils.ParanoidTileRetrieval(i - 1, j + 1);
            var downRight = CalamityUtils.ParanoidTileRetrieval(i + 1, j + 1);

            tile.Get<TileBlendingData>().Clear();
        }
    }
}
