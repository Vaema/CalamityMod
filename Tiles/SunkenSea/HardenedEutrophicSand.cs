using System.Collections.Generic;
using CalamityMod.Items.Placeables;
using CalamityMod.Systems;
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
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            Main.tileShine[Type] = 2500;
            Main.tileShine2[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = DustID.Titanium;
            AddMapEntry(new Color(61, 151, 194));

            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
            this.RegisterUniversalMerge(TileID.Sandstone, "CalamityMod/Tiles/Merges/SandstoneMerge");
            this.RegisterUniversalMerge(TileID.HardenedSand, "CalamityMod/Tiles/Merges/HardenedSandMerge");
            this.RegisterUniversalMerge(TileID.Sand, "CalamityMod/Tiles/Merges/SandMerge");
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BrimstoneFraming(i, j, resetFrame);
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
