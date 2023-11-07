using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Tiles.SunkenSea
{
    public class Shellstone : ModTile
    {
        public byte[,] tileAdjacency;
        public byte[,] secondTileAdjacency;
        public byte[,] thirdTileAdjacency;
        public byte[,] fourthTileAdjacency;

        public override void SetStaticDefaults()
        {
            TileID.Sets.GeneralPlacementTiles[Type] = false;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.HasSlopeFrames[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = SoundID.Tink;
            DustType = 17;
            AddMapEntry(new Color(136, 129, 154));

            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<RuneSand>(), out tileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Sandstone, out secondTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Sand, out thirdTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.HardenedSand, out fourthTileAdjacency);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            //TileFraming.DrawUniversalMergeFrames(i, j, tileAdjacency, "CalamityMod/Tiles/Merges/TimelessSandMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, secondTileAdjacency, "CalamityMod/Tiles/Merges/SandstoneMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, thirdTileAdjacency, "CalamityMod/Tiles/Merges/SandMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, fourthTileAdjacency, "CalamityMod/Tiles/Merges/HardenedSandMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<RuneSand>(), out tileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.Sandstone, out secondTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.Sand, out thirdTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.HardenedSand, out fourthTileAdjacency[i, j]);
            return TileFraming.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
