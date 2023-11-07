using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace CalamityMod.World
{
    public class SunkenSea
    {
        //sides of the sunken sea (radiant reefs)
        //TODO: leftside barrier determines which side the barrier is placed on
        //also make the basalt wall a bit higher since the reefs go off to the side of the desert
        //Add cleanup step to convert sand hanging off edges into shellstone/navystone for all sunken sea biomes
        public static void PlaceRadiantReefs(int startPosX, int startPosY, bool LeftSideBarrier)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int distanceInTiles = (Main.maxTilesY >= 2400 ? 100 : 180) + (Main.maxTilesX - 4200) / 4200 * 200;
            float distance = distanceInTiles * 16f;
            float constant = distance * 2f / (float)Math.Sin(angle);

            float fociSpacing = distance * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //first, place a basalt barrier around where the biome will be
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.99f;

                        if (percent > blurPercent)
                        {
                            if (Y > origin.Y - 50)
                            {
                                if (LeftSideBarrier)
                                {
                                    if (Main.tile[X, Y].TileType != ModContent.TileType<Basalt>() && X <= origin.X)
                                    {
                                        WorldGen.TileRunner(X - 20, Y + 20, WorldGen.genRand.Next(35, 50), WorldGen.genRand.Next(35, 50), ModContent.TileType<Basalt>(), true, 0f, 0f, true, true);
                                    }
                                }
                                else
                                {
                                    if (Main.tile[X, Y].TileType != ModContent.TileType<Basalt>() && X >= origin.X)
                                    {
                                        WorldGen.TileRunner(X + 20, Y + 20, WorldGen.genRand.Next(35, 50), WorldGen.genRand.Next(35, 50), ModContent.TileType<Basalt>(), true, 0f, 0f, true, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.99f;

                        if (percent > blurPercent)
                        {
                            float outerEdgePercent = (percent - blurPercent) / (1f - blurPercent);
                            if (Y > origin.Y)
                            {
                                //place smaller shellstone clumps infront of the basalt so the basalt isnt actually inside of the biome itself
                                WorldGen.TileRunner(X, Y, WorldGen.genRand.Next(8, 12), WorldGen.genRand.Next(8, 12), ModContent.TileType<Shellstone>(), true, 0f, 0f, true, true);
                            }
                        }
                        else
                        {
                            //clear absolutely everything before generating the caverns
                            Main.tile[X, Y].ClearEverything();

                            //generate perlin noise caves
                            float horizontalOffsetNoise = CalamityUtils.PerlinNoise2D(X / 80f, Y / 80f, 5, unchecked(cavePerlinSeed + 1)) * 0.01f;
                            float cavePerlinValue = CalamityUtils.PerlinNoise2D(X / 1000f, Y / 750f, 5, cavePerlinSeed) + 0.5f + horizontalOffsetNoise;
                            float cavePerlinValue2 = CalamityUtils.PerlinNoise2D(X / 1000f, Y / 750f, 5, unchecked(cavePerlinSeed - 1)) + 0.5f;
                            float caveNoiseMap = (cavePerlinValue + cavePerlinValue2) * 0.5f;
                            float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.235f;

                            //kill or place tiles depending on the noise map
                            if (caveNoiseMap * caveNoiseMap > caveCreationThreshold)
                            {
                                WorldGen.KillTile(X, Y);
                            }
                            else
                            {
                                WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Shellstone>());
                            }

                            //place walls in the biome using a different "seed" so it differs from the cave generation
                            //this creates a neat effect where walls worm their way through the caverns while leaving openings for the background to show through
                            float horizontalOffsetNoiseWalls = CalamityUtils.PerlinNoise2D(X / 80f, Y / 80f, 5, unchecked(cavePerlinSeedWalls + 1)) * 0.01f;
                            float cavePerlinValueWalls = CalamityUtils.PerlinNoise2D(X / 1000f, Y / 750f, 5, cavePerlinSeedWalls) + 0.5f + horizontalOffsetNoiseWalls;
                            float cavePerlinValue2Walls = CalamityUtils.PerlinNoise2D(X / 1000f, Y / 750f, 5, unchecked(cavePerlinSeedWalls - 1)) + 0.5f;
                            float caveNoiseMapWalls = (cavePerlinValueWalls + cavePerlinValue2Walls) * 0.5f;
                            float caveCreationThresholdWalls = horizontalOffsetNoiseWalls * 3.5f + 0.235f;

                            if (caveNoiseMapWalls * caveNoiseMapWalls > caveCreationThresholdWalls)
                            {
                                WorldGen.PlaceWall(X, Y, ModContent.WallType<ShellstoneWall>());
                            }

                            Main.tile[X, Y].Get<LiquidData>().LiquidType = LiquidID.Water;
                            Main.tile[X, Y].LiquidAmount = byte.MaxValue;
                        }
                    }
                }
            }

            //cleanup
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        //clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                        {
                            WorldGen.KillTile(X, Y);
                        }

                        //kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
                }
            }

            //place extra tiles
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        //place sand clumps on top of exposed shellstone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>() && !Main.tile[X, Y - 1].HasTile)
                        {
                            for (int j = Y; j <= Y + 3; j++)
                            {
                                bool EnoughTilesBelow = Main.tile[X, Y + 1].TileType == ModContent.TileType<Shellstone>() && Main.tile[X, Y + 2].TileType == ModContent.TileType<Shellstone>() &&
                                Main.tile[X, Y + 3].TileType == ModContent.TileType<Shellstone>() && Main.tile[X, Y + 4].TileType == ModContent.TileType<Shellstone>();
                                 
                                if (EnoughTilesBelow && CanPlaceSandOnGround(X, Y))
                                {
                                    Main.tile[X, j].TileType = (ushort)ModContent.TileType<EutrophicSand>();
                                    Main.tile[X, j + 1].TileType = (ushort)ModContent.TileType<EutrophicSand>();
                                }
                            }
                        }
                    }
                }
            }

            //cleanup again
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        //clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                        {
                            WorldGen.KillTile(X, Y);
                        }

                        //kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
                }
            }
        }

        //middle of the sunken sea (polyp forest)
        //TODO: make an actual transition, and randomly place water caves along the edge of it to make it blend with the reefs more
        public static void PlacePolypForest(int startPosX, int startPosY)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int distanceInTiles = (Main.maxTilesY >= 2400 ? 20 : 150) + (Main.maxTilesX - 4200) / 4200 * 200;
            float distance = distanceInTiles * 16f;
            float constant = distance * 2f / (float)Math.Sin(angle);

            float fociSpacing = distance * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.98f;

                        //biome "blending" on the edges (disabled for now)
                        if (percent > blurPercent)
                        {
                            /*
                            float outerEdgePercent = (percent - blurPercent) / (1f - blurPercent);
                            if (WorldGen.genRand.NextFloat(1f) > outerEdgePercent && Y > origin.Y && Main.tile[X, Y].HasTile)
                            {
                                Main.tile[X, Y].TileType = (ushort)ModContent.TileType<Navystone>();
                                WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Navystone>());
                            }
                            */
                        }
                        else
                        {
                            //clear absolutely everything before generating the caverns
                            Main.tile[X, Y].ClearEverything();

                            //generate perlin noise caves
                            float horizontalOffsetNoise = CalamityUtils.PerlinNoise2D(X / 80f, Y / 80f, 5, unchecked(cavePerlinSeed + 1)) * 0.01f;
                            float cavePerlinValue = CalamityUtils.PerlinNoise2D(X / 700f, Y / 650f, 5, cavePerlinSeed) + 0.5f + horizontalOffsetNoise;
                            float cavePerlinValue2 = CalamityUtils.PerlinNoise2D(X / 700f, Y / 650f, 5, unchecked(cavePerlinSeed - 1)) + 0.5f;
                            float caveNoiseMap = (cavePerlinValue + cavePerlinValue2) * 0.5f;
                            float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.235f;

                            //kill or place tiles depending on the noise map
                            if (caveNoiseMap * caveNoiseMap > caveCreationThreshold)
                            {
                                WorldGen.KillTile(X, Y);
                            }
                            else
                            {
                                WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Limestone>());
                            }

                            //place walls in the biome using a different "seed" so it differs from the cave generation
                            //this creates a neat effect where walls worm their way through the caverns while leaving openings for the background to show through
                            float horizontalOffsetNoiseWalls = CalamityUtils.PerlinNoise2D(X / 80f, Y / 80f, 5, unchecked(cavePerlinSeedWalls + 1)) * 0.01f;
                            float cavePerlinValueWalls = CalamityUtils.PerlinNoise2D(X / 700f, Y / 650f, 5, cavePerlinSeedWalls) + 0.5f + horizontalOffsetNoiseWalls;
                            float cavePerlinValue2Walls = CalamityUtils.PerlinNoise2D(X / 700f, Y / 650f, 5, unchecked(cavePerlinSeedWalls - 1)) + 0.5f;
                            float caveNoiseMapWalls = (cavePerlinValueWalls + cavePerlinValue2Walls) * 0.5f;
                            float caveCreationThresholdWalls = horizontalOffsetNoiseWalls * 3.5f + 0.235f;

                            if (caveNoiseMapWalls * caveNoiseMapWalls > caveCreationThresholdWalls)
                            {
                                //temporarily use navystone
                                WorldGen.PlaceWall(X, Y, ModContent.WallType<NavystoneWall>());
                            }

                            Main.tile[X, Y].Get<LiquidData>().LiquidType = LiquidID.Water;
                            Main.tile[X, Y].LiquidAmount = byte.MaxValue;
                        }
                    }
                }
            }

            //cleanup
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        //clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                        {
                            WorldGen.KillTile(X, Y);
                        }

                        //kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
                }
            }

            //place extra tiles
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        //place sand on top of exposed limestone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Limestone>() && !Main.tile[X, Y - 1].HasTile)
                        {
                            for (int j = Y; j <= Y + 3; j++)
                            {
                                bool EnoughTilesBelow = Main.tile[X, Y + 1].TileType == ModContent.TileType<Limestone>() && Main.tile[X, Y + 2].TileType == ModContent.TileType<Limestone>() &&
                                Main.tile[X, Y + 3].TileType == ModContent.TileType<Limestone>() && Main.tile[X, Y + 4].TileType == ModContent.TileType<Limestone>();
                                 
                                if (EnoughTilesBelow && CanPlaceSandOnGround(X, Y))
                                {
                                    Main.tile[X, j].TileType = (ushort)ModContent.TileType<EutrophicSand>();
                                    Main.tile[X, j + 1].TileType = (ushort)ModContent.TileType<EutrophicSand>();
                                }
                            }
                        }
                    }
                }
            }
 
            //cleanup again
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        //clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                        {
                            WorldGen.KillTile(X, Y);
                        }

                        //kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
                }
            }
        }

        //bottom of the biome (gleaming burrows)
        //TODO: randomly place water caves along the top-half edge of it to make it blend with the reefs more (like the polyp forest)
        public static void PlaceGleamingBurrows(int startPosX, int startPosY)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int distanceInTiles = (Main.maxTilesY >= 2400 ? 150 : 200) + (Main.maxTilesX - 4200) / 4200 * 200;
            float distance = distanceInTiles * 16f;
            float constant = distance * 2f / (float)Math.Sin(angle);

            float fociSpacing = distance * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //first, place a basalt barrier around where the biome will be
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.99f;

                        if (percent > blurPercent)
                        {
                            float outerEdgePercent = (percent - blurPercent) / (1f - blurPercent);
                            if (Y > origin.Y)
                            {
                                if (Main.tile[X, Y].TileType != ModContent.TileType<Basalt>())
                                {
                                    WorldGen.TileRunner(X, Y + 20, WorldGen.genRand.Next(30, 35), WorldGen.genRand.Next(30, 35), ModContent.TileType<Basalt>(), true, 0f, 0f, true, true);
                                }
                            }
                        }
                    }
                }
            }

            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.99f;

                        //place barrier of basalt under the burrows
                        if (percent > blurPercent && Y > origin.Y)
                        {
                            //place smaller navystone clumps infront of the basalt so the basalt isnt actually inside of the biome itself
                            WorldGen.TileRunner(X, Y, WorldGen.genRand.Next(8, 12), WorldGen.genRand.Next(8, 12), ModContent.TileType<Navystone>(), true, 0f, 0f, true, false);
                        }
                        else
                        {
                            //clear absolutely everything before generating the caverns
                            Main.tile[X, Y].ClearEverything();

                            //generate perlin noise caves
                            float horizontalOffsetNoise = CalamityUtils.PerlinNoise2D(X / 80, Y / 80, 5, unchecked(cavePerlinSeed + 1)) * 0.01f;
                            float cavePerlinValue = CalamityUtils.PerlinNoise2D(X / 375f, Y / 375f, 5, cavePerlinSeed) + 0.5f + horizontalOffsetNoise;
                            float cavePerlinValue2 = CalamityUtils.PerlinNoise2D(X / 375f, Y / 375f, 5, unchecked(cavePerlinSeed - 1)) + 0.5f;
                            float caveNoiseMap = (cavePerlinValue + cavePerlinValue2) * 0.5f;
                            float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.235f;

                            //kill or place tiles depending on the noise map
                            if (caveNoiseMap * caveNoiseMap > caveCreationThreshold)
                            {
                                WorldGen.KillTile(X, Y);
                            }
                            else
                            {
                                WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Navystone>());
                            }

                            //place walls in the biome using a different "seed" so it differs from the cave generation
                            //this creates a neat effect where walls worm their way through the caverns while leaving openings for the background to show through
                            float horizontalOffsetNoiseWalls = CalamityUtils.PerlinNoise2D(X / 80, Y / 80, 5, unchecked(cavePerlinSeedWalls + 1)) * 0.01f;
                            float cavePerlinValueWalls = CalamityUtils.PerlinNoise2D(X / 375f, Y / 375f, 5, cavePerlinSeedWalls) + 0.5f + horizontalOffsetNoiseWalls;
                            float cavePerlinValue2Walls = CalamityUtils.PerlinNoise2D(X / 375f, Y / 375f, 5, unchecked(cavePerlinSeedWalls - 1)) + 0.5f;
                            float caveNoiseMapWalls = (cavePerlinValueWalls + cavePerlinValue2Walls) * 0.5f;
                            float caveCreationThresholdWalls = horizontalOffsetNoiseWalls * 3.5f + 0.235f;

                            if (caveNoiseMapWalls * caveNoiseMapWalls > caveCreationThresholdWalls)
                            {
                                WorldGen.PlaceWall(X, Y, ModContent.WallType<NavystoneWall>());
                            }

                            Main.tile[X, Y].Get<LiquidData>().LiquidType = LiquidID.Water;
                            Main.tile[X, Y].LiquidAmount = byte.MaxValue;
                        }
                    }
                }
            }

            //cleanup the perlin caves
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        //clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                        {
                            WorldGen.KillTile(X, Y);
                        }

                        //kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
                }
            }

            //place extra tiles
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        //place sand on top of exposed navystone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Navystone>() && !Main.tile[X, Y - 1].HasTile)
                        {
                            for (int j = Y; j <= Y + 3; j++)
                            {
                                bool EnoughTilesBelow = Main.tile[X, Y + 1].TileType == ModContent.TileType<Navystone>() && Main.tile[X, Y + 2].TileType == ModContent.TileType<Navystone>() &&
                                Main.tile[X, Y + 3].TileType == ModContent.TileType<Navystone>() && Main.tile[X, Y + 4].TileType == ModContent.TileType<Navystone>();
                                 
                                if (EnoughTilesBelow && CanPlaceSandOnGround(X, Y))
                                {
                                    Main.tile[X, j].TileType = (ushort)ModContent.TileType<EutrophicSand>();
                                    Main.tile[X, j + 1].TileType = (ushort)ModContent.TileType<EutrophicSand>();
                                }
                            }
                        }
                    }
                }
            }

            //cleanup again
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        //clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                        {
                            WorldGen.KillTile(X, Y);
                        }

                        //kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
                }
            }
        }

        public static void SunkenSeaAmbience()
        {
            for (int X = 20; X < Main.maxTilesX - 20; X++)
            {
                for (int Y = 20; Y < Main.maxTilesY - 20; Y++)
                {

                }
            }
        }

        public static bool CanPlaceSandOnGround(int X, int Y)
        {
            int numTiles = 0;

            for (int i = X - 3; i <= X + 3; i++)
            {
                for (int j = Y; j <= Y + 5; j++)
                {
                    if (Main.tile[i, j].HasTile)
                    {
                        numTiles++;
                    }
                }
            }

            if (numTiles >= 25)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //method to make sure things only generate in each biome circle
        public static bool CheckInEllipse(Point tile, Vector2 focus1, Vector2 focus2, float distanceConstant, Vector2 center, out float distance, bool collapse = false)
        {
            Vector2 point = tile.ToWorldCoordinates();
            if (collapse) //Collapse ensures the ellipse is shrunk down a lot in terms of distance.
            {
                float distY = center.Y - point.Y;
                point.Y -= distY * 3f;
            }
            float distance1 = Vector2.Distance(point, focus1);
            float distance2 = Vector2.Distance(point, focus2);
            distance = distance1 + distance2;
            return distance <= distanceConstant;
        }
    }
}
