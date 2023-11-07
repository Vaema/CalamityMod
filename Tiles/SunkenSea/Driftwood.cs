using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Tiles.SunkenSea
{
    public class Driftwood : ModTile
    {
        public byte[,] tileAdjacency;
        public byte[,] secondTileAdjacency;
        public byte[,] thirdTileAdjacency;
        public byte[,] fourthTileAdjacency;
        public byte[,] fifthTileAdjacency;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Wood"]);

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeDecorativeTiles(Type);
            CalamityUtils.MergeWithDesert(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.HasSlopeFrames[Type] = true;
            HitSound = SoundID.Dig;
            DustType = 121;
            AddMapEntry(new Color(136, 129, 154));

            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<RuneSand>(), out tileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<Shellstone>(), out secondTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Sandstone, out thirdTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Sand, out fourthTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.HardenedSand, out fifthTileAdjacency);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            //TileFraming.DrawUniversalMergeFrames(i, j, tileAdjacency, "CalamityMod/Tiles/Merges/TimelessSandMerge");
            //TileFraming.DrawUniversalMergeFrames(i, j, secondTileAdjacency, "CalamityMod/Tiles/Merges/ShellstoneMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, thirdTileAdjacency, "CalamityMod/Tiles/Merges/SandstoneMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, fourthTileAdjacency, "CalamityMod/Tiles/Merges/SandMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, fifthTileAdjacency, "CalamityMod/Tiles/Merges/HardenedSandMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<RuneSand>(), out tileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<Shellstone>(), out secondTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.Sandstone, out thirdTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.Sand, out fourthTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.HardenedSand, out fifthTileAdjacency[i, j]);
            return TileFraming.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
