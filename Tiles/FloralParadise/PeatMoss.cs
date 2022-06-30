using CalamityMod.Items.Placeables;
using CalamityMod.Projectiles.Environment;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FloralParadise
{
    public class PeatMoss : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;

            TileID.Sets.Grass[Type] = true;
            TileID.Sets.NeedsGrassFraming[Type] = true;
            TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<Peat>();
            TileID.Sets.Conversion.Grass[Type] = true;

            CalamityUtils.SetMerge(Type, TileID.Dirt);
            CalamityUtils.SetMerge(Type, TileID.Grass);
            CalamityUtils.SetMerge(Type, TileID.CorruptGrass);
            CalamityUtils.SetMerge(Type, TileID.HallowedGrass);
            CalamityUtils.SetMerge(Type, TileID.CrimsonGrass);
            CalamityUtils.MergeWithFloralParadise(Type);

            DustType = 39;
            ItemDrop = ModContent.ItemType<PeatItem>();

            AddMapEntry(new Color(65, 142, 101));
        }

        public override void NumDust(int i, int j, bool fail, ref int Type)
        {
            Type = fail ? 1 : 3;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail && !effectOnly)
                Main.tile[i, j].TileType = (ushort)ModContent.TileType<Peat>();
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (WorldGen.genRand.NextBool(60) && !Main.gamePaused && CalamityUtils.ParanoidTileRetrieval(i, j - 1).LiquidAmount >= 200)
            {
                Vector2 algaeVelocity = -Vector2.UnitY.RotatedByRandom(0.91f) * Main.rand.NextFloat(0.85f, 1.6f);
                Projectile.NewProjectile(new EntitySource_WorldEvent(), new Vector2(i + 0.5f, j - 0.5f) * 16f, algaeVelocity, ModContent.ProjectileType<WaterAlgae>(), 0, 0f);
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            int num8 = WorldGen.genRand.Next((int)Main.rockLayer, (int)(Main.rockLayer + (double)Main.maxTilesY * 0.143));
            int nearbyVineCount = 0;
            for (int x = i - 15; x <= i + 15; x++)
            {
                for (int y = j - 15; y <= j + 15; y++)
                {
                    if (WorldGen.InWorld(x, y))
                    {
                        if (CalamityUtils.ParanoidTileRetrieval(x, y).HasTile &&
                            CalamityUtils.ParanoidTileRetrieval(x, y).TileType == (ushort)ModContent.TileType<SmallVines>())
                        {
                            nearbyVineCount++;
                        }
                    }
                }
            }
            if (Main.tile[i, j + 1] != null && nearbyVineCount < 5)
            {
                if (!Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType != (ushort)ModContent.TileType<SmallVines>())
                {
                    if (Main.tile[i, j + 1].LiquidAmount == 255 &&
                        Main.tile[i, j + 1].LiquidType == LiquidID.Water)
                    {
                        bool flag13 = false;
                        for (int y = num8; y > num8 - 10; y--)
                        {
                            if (Main.tile[i, y].BottomSlope)
                            {
                                flag13 = false;
                                break;
                            }
                            if (Main.tile[i, y].HasTile && !Main.tile[i, y].BottomSlope)
                            {
                                flag13 = true;
                                break;
                            }
                        }
                        if (flag13)
                        {
                            int num53 = i;
                            int num54 = j + 1;
                            Main.tile[num53, num54].TileType = (ushort)ModContent.TileType<SmallVines>();
                            Main.tile[num53, num54].Get<TileWallWireStateData>().HasTile = true;
                            WorldGen.SquareTileFrame(num53, num54, true);
                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendTileSquare(-1, num53, num54, 3, TileChangeType.None);
                            }
                        }
                        Main.tile[i, j].Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                        Main.tile[i, j].Get<TileWallWireStateData>().IsHalfBlock = false;
                    }
                }
            }
        }
    }
}
