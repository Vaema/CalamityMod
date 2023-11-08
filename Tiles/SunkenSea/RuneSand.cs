using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Tiles.SunkenSea
{
    public class RuneSand : ModTile
    {
        public byte[,] tileAdjacency;
        public byte[,] secondTileAdjacency;
        public byte[,] thirdTileAdjacency;

        public override void SetStaticDefaults()
        {
            TileID.Sets.GeneralPlacementTiles[Type] = false;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;
            Main.tileShine2[Type] = true;

            DustType = 147;
            AddMapEntry(new Color(210, 124, 45));

            TileFraming.SetUpUniversalMerge(Type, TileID.Sandstone, out tileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Sand, out secondTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.HardenedSand, out thirdTileAdjacency);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            TileFraming.DrawUniversalMergeFrames(i, j, tileAdjacency, "CalamityMod/Tiles/Merges/SandstoneMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, secondTileAdjacency, "CalamityMod/Tiles/Merges/SandMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, thirdTileAdjacency, "CalamityMod/Tiles/Merges/HardenedSandMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFraming.GetAdjacencyData(i, j, TileID.Sandstone, out tileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.Sand, out secondTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.HardenedSand, out thirdTileAdjacency[i, j]);
            return TileFraming.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
