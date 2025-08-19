using System.Collections.Generic;
using CalamityMod.Items.Placeables;
using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.ModBrowser;

namespace CalamityMod.Tiles.SunkenSea
{
    public class EutrophicSand : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.GeneralPlacementTiles[Type] = false;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.HasSlopeFrames[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Sand"]);

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type); // Tile blends with sandstone, which it is set to merge with here

            Main.tileShine[Type] = 2500;

            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = DustID.Titanium;
            AddMapEntry(new Color(163, 194, 212));

            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Shellstone>(), "CalamityMod/Tiles/Merges/ShellstoneMerge");
            this.RegisterUniversalMerge(TileID.Sandstone, "CalamityMod/Tiles/Merges/SandstoneMerge");
            this.RegisterUniversalMerge(TileID.Sand, "CalamityMod/Tiles/Merges/SandMerge");
            this.RegisterUniversalMerge(TileID.HardenedSand, "CalamityMod/Tiles/Merges/HardenedSandMerge");
        }

        public override void RandomUpdate(int i, int j)
        {
            if (!WorldGen.genRand.NextBool(8))
                return;

            Tile tile = Main.tile[i, j];
            if (tile.HasTile || tile.LiquidAmount <= 0)
                return;

            if (tile.LeftSlope || tile.RightSlope || tile.IsHalfBlock)
                return;
            
            Tile up = Main.tile[i, j - 1];
            if (up.HasTile || up.LiquidAmount <= 0)
                return;

            Tile up2 = Main.tile[i, j - 2];
            if (up2.HasTile || up2.LiquidAmount <= 0)
                return;

            // Place corals
            if (WorldGen.genRand.NextBool(3))
            {
                up.TileType = (ushort)ModContent.TileType<SmallCorals>();
                up.HasTile = true;
                up.TileFrameY = 0;

                // 15 different frames, choose a random one
                up.TileFrameX = (short)(WorldGen.genRand.Next(15) * 18);
                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.dedServ)
                {
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
                }
            }

            //multi-colored corals
            if (WorldGen.genRand.NextBool(3))
            {
                ushort[] ColoredCorals = new ushort[] { (ushort)ModContent.TileType<CoralPileGiant>(),
                    (ushort)ModContent.TileType<CoralPileLarge>(), (ushort)ModContent.TileType<MediumCoral2>() };

                ushort newObject = Main.rand.Next(ColoredCorals);

                WorldGen.PlaceObject(i, j - 1, newObject, true);
                NetMessage.SendObjectPlacement(-1, i, j - 1, newObject, 0, 0, -1, -1);
            }

            //blue coral trees
            if (WorldGen.genRand.NextBool(6))
            {
                ushort[] BlueCorals = new ushort[] { (ushort)ModContent.TileType<MediumCoral3>(), (ushort)ModContent.TileType<BlueCoralTree>() };

                ushort newObject = Main.rand.Next(BlueCorals);

                WorldGen.PlaceObject(i, j - 1, newObject, true);
                NetMessage.SendObjectPlacement(-1, i, j - 1, newObject, 0, 0, -1, -1);
            }

            //brown coral trees
            if (WorldGen.genRand.NextBool(6))
            {
                ushort[] BrownCorals = new ushort[] { (ushort)ModContent.TileType<BrownCoral1>(), (ushort)ModContent.TileType<BrownCoral2>() };

                ushort newObject = Main.rand.Next(BrownCorals);

                WorldGen.PlaceObject(i, j - 1, newObject, true);
                NetMessage.SendObjectPlacement(-1, i, j - 1, newObject, 0, 0, -1, -1);
            }

            //fan coral
            if (WorldGen.genRand.NextBool(10))
            {
                WorldGen.PlaceObject(i, j - 1, (ushort)ModContent.TileType<FanCoral>());
            }

            //misc corals
            if (WorldGen.genRand.NextBool())
            {
                ushort[] MiscCorals = new ushort[] { (ushort)ModContent.TileType<MediumCoral>(),
                    (ushort)ModContent.TileType<SmallWideCoral>(), (ushort)ModContent.TileType<SmallWideCoral2>() };

                ushort newObject = Main.rand.Next(MiscCorals);

                WorldGen.PlaceObject(i, j - 1, newObject, true);
                NetMessage.SendObjectPlacement(-1, i, j - 1, newObject, 0, 0, -1, -1);
            }
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
