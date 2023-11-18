using CalamityMod.Items.Placeables;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class HardenedEutrophicSand : ModTile
    {
        public byte[,] tileAdjacency;
        public byte[,] secondTileAdjacency;
        public byte[,] thirdTileAdjacency;
        public byte[,] fourthTileAdjacency;
        public byte[,] fifthTileAdjacency;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            Main.tileShine[Type] = 1800;
            Main.tileShine2[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = 108;
            AddMapEntry(new Color(61, 151, 194));

            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<EutrophicSand>(), out tileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<Navystone>(), out secondTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Sandstone, out thirdTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.HardenedSand, out fourthTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Sand, out fifthTileAdjacency);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            TileFraming.DrawUniversalMergeFrames(i, j, secondTileAdjacency, "CalamityMod/Tiles/Merges/NavystoneMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, tileAdjacency, "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, thirdTileAdjacency, "CalamityMod/Tiles/Merges/SandstoneMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, fourthTileAdjacency, "CalamityMod/Tiles/Merges/HardenedSandMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, fifthTileAdjacency, "CalamityMod/Tiles/Merges/SandMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<EutrophicSand>(), out tileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<Navystone>(), out secondTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.Sandstone, out thirdTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.HardenedSand, out fourthTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, TileID.Sand, out fifthTileAdjacency[i, j]);
            return TileFraming.BrimstoneFraming(i, j, resetFrame);
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            Tile up = Main.tile[i, j - 1];
            Tile up2 = Main.tile[i, j - 2];

            if (!up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                //brain corals
                if (WorldGen.genRand.NextBool(18))
                {
                    ushort[] BrainCorals = new ushort[] { (ushort)ModContent.TileType<BrainCoral>(), (ushort)ModContent.TileType<SmallBrainCoral>() };

                    ushort newObject = Main.rand.Next(BrainCorals);

                    WorldGen.PlaceObject(i, j - 1, newObject, true);
                    NetMessage.SendObjectPlacement(-1, i, j - 1, newObject, 0, 0, -1, -1);
                }

                //tube corals
                if (WorldGen.genRand.NextBool(8))
                {
                    ushort[] TubeCorals = new ushort[] { (ushort)ModContent.TileType<TubeCoral>(), (ushort)ModContent.TileType<SmallTubeCoral>() };

                    ushort newObject = Main.rand.Next(TubeCorals);

                    WorldGen.PlaceObject(i, j - 1, newObject, true);
                    NetMessage.SendObjectPlacement(-1, i, j - 1, newObject, 0, 0, -1, -1);
                }

                //anemonie
                if (WorldGen.genRand.NextBool(10))
                {
                    WorldGen.PlaceObject(i, j - 1, (ushort)ModContent.TileType<SeaAnemone>(), true);
                    NetMessage.SendObjectPlacement(-1, i, j - 1, (ushort)ModContent.TileType<SeaAnemone>(), 0, 0, -1, -1);
                }
            }
        }
    }
}
