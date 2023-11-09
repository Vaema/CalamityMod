using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using CalamityMod.Walls;
using Terraria.GameContent.Generation;

namespace CalamityMod.World
{
    public class SunkenSea
    {
        //sides of the sunken sea (radiant reefs)
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
                            if (Y > origin.Y - 60)
                            {
                                if (LeftSideBarrier)
                                {
                                    if (X <= origin.X && Main.tile[X, Y].TileType != ModContent.TileType<Basalt>())
                                    {
                                        ShapeData circle = new ShapeData();
                                        GenAction blotchMod = new Modifiers.Blotches(2, 0.4);

                                        WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(60), Actions.Chain(new GenAction[]
                                        {
                                            blotchMod.Output(circle)
                                        }));
                                        WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                        {
                                            new Actions.Clear(),
                                            new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>())
                                        }));
                                    }
                                }
                                else
                                {
                                    if (X >= origin.X && Main.tile[X, Y].TileType != ModContent.TileType<Basalt>())
                                    {
                                        ShapeData circle = new ShapeData();
                                        GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                                        
                                        WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(60), Actions.Chain(new GenAction[]
                                        {
                                            blotchMod.Output(circle)
                                        }));
                                        WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                        {
                                            new Actions.Clear(),
                                            new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>())
                                        }));
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
                            if (Y > origin.Y - 60)
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
                        bool PlaceSand = false;

                        //place sand clumps on top of exposed shellstone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile)
                        {
                            PlaceSand = true;
                        }

                        if (PlaceSand)
                        {
                            for (int SandY = Y; SandY <= Y + 4; SandY++)
                            {
                                if (EnoughTilesInArea(X, SandY, 3, 5, 30, false) && (!Main.tile[X, SandY - 1].HasTile || Main.tile[X, SandY - 1].TileType == ModContent.TileType<EutrophicSand>()))
                                {
                                    Main.tile[X, SandY].TileType = (ushort)ModContent.TileType<EutrophicSand>();
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

                        //if any sand is floating, put tiles below it
                        if (Main.tile[X, Y].TileType == ModContent.TileType<EutrophicSand>() && !Main.tile[X, Y + 1].HasTile)
                        {
                            WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<Shellstone>());
                            Main.tile[X, Y].TileType = (ushort)ModContent.TileType<Shellstone>();
                        }

                        Tile.SmoothSlope(X, Y);
                    }
                }
            }
        }

        //middle of the sunken sea (polyp forest)
        //TODO: make an actual transition, and randomly place small water caves along the edge of it to make it blend with the reefs more
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

            //place the polyp forest caverns
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
                            if (WorldGen.genRand.NextFloat(1f) > outerEdgePercent && Y < origin.Y && Main.tile[X, Y].HasTile)
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
                                WorldGen.PlaceWall(X, Y, ModContent.WallType<LimestoneWall>());
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
                        bool PlaceSand = false;

                        //place sand clumps on top of exposed limestone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Limestone>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile)
                        {
                            PlaceSand = true;
                        }

                        if (PlaceSand)
                        {
                            for (int SandY = Y; SandY <= Y + 4; SandY++)
                            {
                                if (EnoughTilesInArea(X, SandY, 3, 5, 30, false) && (!Main.tile[X, SandY - 1].HasTile || Main.tile[X, SandY - 1].TileType == ModContent.TileType<PolypSand>()))
                                {
                                    Main.tile[X, SandY].TileType = (ushort)ModContent.TileType<PolypSand>();
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

                        //if any sand is floating, put tiles below it
                        if (Main.tile[X, Y].TileType == ModContent.TileType<PolypSand>() && !Main.tile[X, Y + 1].HasTile)
                        {
                            WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<Limestone>());
                            Main.tile[X, Y].TileType = (ushort)ModContent.TileType<Limestone>();
                        }

                        Tile.SmoothSlope(X, Y);
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

            int distanceInTiles = (Main.maxTilesY >= 2400 ? 150 : 235) + (Main.maxTilesX - 4200) / 4200 * 200;
            float distance = distanceInTiles * 16f;
            float constant = distance * 2f / (float)Math.Sin(angle);

            float fociSpacing = distance * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //place the gleaming burrows caverns
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
                            if (Y > origin.Y - 50)
                            {
                                //place smaller navystone clumps infront of the basalt so the basalt isnt actually inside of the biome itself
                                WorldGen.TileRunner(X, Y, WorldGen.genRand.Next(8, 12), WorldGen.genRand.Next(8, 12), ModContent.TileType<Navystone>(), true, 0f, 0f, true, true);
                            }
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
                        bool PlaceSand = false;

                        //place sand clumps on top of exposed navystone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Navystone>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile)
                        {
                            PlaceSand = true;
                        }

                        if (PlaceSand)
                        {
                            for (int SandY = Y; SandY <= Y + 4; SandY++)
                            {
                                if (EnoughTilesInArea(X, SandY, 3, 5, 30, false) && (!Main.tile[X, SandY - 1].HasTile || Main.tile[X, SandY - 1].TileType == ModContent.TileType<HardenedEutrophicSand>()))
                                {
                                    Main.tile[X, SandY].TileType = (ushort)ModContent.TileType<HardenedEutrophicSand>();
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
                        //place geodes
                        if (WorldGen.genRand.NextBool(600) && Main.tile[X, Y].TileType == ModContent.TileType<Navystone>() && 
                        Main.tile[X, Y - 1].HasTile && Main.tile[X, Y + 1].HasTile && Main.tile[X - 1, Y].HasTile && Main.tile[X + 1, Y].HasTile)
                        {
                            if (EnoughTilesInArea(X, Y, 7, 7, 35, true))
                            {
                                PlaceGeode(X, Y, WorldGen.genRand.Next(10, 17));
                            }
                        }

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

                        //if any sand is floating, put tiles below it
                        if (Main.tile[X, Y].TileType == ModContent.TileType<EutrophicSand>() && !Main.tile[X, Y + 1].HasTile)
                        {
                            WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<Navystone>());
                            Main.tile[X, Y].TileType = (ushort)ModContent.TileType<Navystone>();
                        }

                        Tile.SmoothSlope(X, Y);
                    }
                }
            }
        }

        //basalt biome underneath the sunken sea
        public static void PlaceBasaltBiome(int startPosX, int startPosY)
        {
            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int distanceInTiles = (Main.maxTilesY >= 2400 ? 150 : 235) + (Main.maxTilesX - 4200) / 4200 * 200;
            float distance = distanceInTiles * 16f;
            float constant = distance * 2f / (float)Math.Sin(angle);

            float fociSpacing = distance * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //first, place a basalt barrier around where the gleaming burrows will be
            for (int X = origin.X - distanceInTiles - 3; X <= origin.X + distanceInTiles + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInEllipse(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.97f;

                        if (percent > blurPercent)
                        {
                            if (Y > origin.Y - 10 && Main.tile[X, Y].TileType != ModContent.TileType<Basalt>())
                            {
                                ShapeData circle = new ShapeData();
                                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                                
                                WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(60), Actions.Chain(new GenAction[]
                                {
                                    blotchMod.Output(circle)
                                }));
                                WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                {
                                    new Actions.Clear(),
                                    new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>())
                                }));

                                //place a giant area of basalt below the sunken sea
                                for (int j = Y; j <= Main.maxTilesY - 250; j += 15)
                                {
                                    int radius = WorldGen.genRand.Next(30, 45);
                                    int RandomX = Main.rand.Next(-25, 25);

                                    WorldUtils.Gen(new Point(X + RandomX, j), new Shapes.Circle(radius), Actions.Chain(new GenAction[]
                                    {
                                        blotchMod.Output(circle)
                                    }));

                                    List<ushort> WallIDs = new()
                                    {
                                        WallID.LavaUnsafe1, WallID.LavaUnsafe2, WallID.LavaUnsafe3, WallID.LavaUnsafe4,
                                    };

                                    //place blocks
                                    WorldUtils.Gen(new Point(X + RandomX, j), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                    {
                                        new Actions.Clear(),
                                        new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>()),
                                        new Actions.PlaceWall(WorldGen.genRand.Next(WallIDs))
                                    }));
                                }
                            }
                        }
                    }
                }
            }

            //place caverns and lava
            for (int X = origin.X - distanceInTiles - 250; X <= origin.X + distanceInTiles + 250; X++)
            {
                for (int Y = origin.Y + 50; Y <= Main.maxTilesY - 210; Y++)
                {
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Basalt>())
                    {
                        //place caves
                        if (WorldGen.genRand.NextBool(350))
                        {
                            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(5, 12)), Actions.Chain(new Modifiers.Blotches(
                            WorldGen.genRand.Next(3, 5), WorldGen.genRand.Next(3, 5)), new Actions.ClearTile()));
                        }

                        //place lava
                        if (WorldGen.genRand.NextBool(2000))
                        {
                            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(5, 12)), Actions.Chain(new Modifiers.Blotches(
                            WorldGen.genRand.Next(3, 5), WorldGen.genRand.Next(3, 5)), new Actions.SetLiquid(LiquidID.Lava, 255)));
                        }
                    }
                }
            }

            //place sand blocks
            for (int X = origin.X - distanceInTiles - 250; X <= origin.X + distanceInTiles + 250; X++)
            {
                for (int Y = origin.Y + 50; Y <= Main.maxTilesY - 200; Y++)
                {
                    bool PlaceSand = false;

                    //place sand clumps on top of exposed basalt
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Basalt>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile)
                    {
                        PlaceSand = true;
                    }

                    if (PlaceSand)
                    {
                        for (int SandY = Y; SandY <= Y + 4; SandY++)
                        {
                            if (EnoughTilesInArea(X, SandY, 3, 5, 25, false) && (!Main.tile[X, SandY - 1].HasTile || Main.tile[X, SandY - 1].TileType == ModContent.TileType<VolcanicSand>()))
                            {
                                Main.tile[X, SandY].TileType = (ushort)ModContent.TileType<VolcanicSand>();
                            }
                        }
                    }
                }
            }

            //cleanup
            for (int X = origin.X - distanceInTiles - 250; X <= origin.X + distanceInTiles + 250; X++)
            {
                for (int Y = origin.Y + 50; Y <= Main.maxTilesY - 210; Y++)
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

                    //if any sand is floating, put tiles below it
                    if (Main.tile[X, Y].TileType == ModContent.TileType<VolcanicSand>() && !Main.tile[X, Y + 1].HasTile)
                    {
                        WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<Basalt>());
                        Main.tile[X, Y].TileType = (ushort)ModContent.TileType<Basalt>();
                    }
                }
            }
        }

        public static void BasaltBiomeLavaCleanup(int startPosX, int startPosY)
        {
            Point origin = new Point(startPosX, startPosY);

            int distanceInTiles = (Main.maxTilesY >= 2400 ? 150 : 235) + (Main.maxTilesX - 4200) / 4200 * 200;

            for (int X = origin.X - distanceInTiles - 250; X <= origin.X + distanceInTiles + 250; X++)
            {
                for (int Y = origin.Y + 50; Y <= Main.maxTilesY - 210; Y++)
                {
                    List<ushort> WallIDs = new()
                    {
                        WallID.LavaUnsafe1, WallID.LavaUnsafe2, WallID.LavaUnsafe3, WallID.LavaUnsafe4,
                    };

                    if (WallIDs.Contains(Main.tile[X, Y].WallType))
                    {
                        //get rid of water
                        if (Main.tile[X, Y].LiquidType == LiquidID.Water)
                        {
                            Main.tile[X, Y].Get<LiquidData>().LiquidType = LiquidID.Lava;
                        }

                        //get rid of obsidian blocks
                        if (Main.tile[X, Y].TileType == TileID.Obsidian)
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
                }
            }
        }

        //place all the ambient tiles in the sunken sea
        public static void PlaceSunkenSeaAmbience()
        {
            //first clean up unnecessary chunks of tiles
            CleanOutSmallClumps();

            for (int X = 20; X <= Main.maxTilesX - 20; X++)
            {
                for (int Y = 20; Y <= Main.maxTilesY - 20; Y++)
                {
                    //gleaming burrows ambient tiles
                    if (Main.tile[X, Y].TileType == ModContent.TileType<HardenedEutrophicSand>())
                    {   
                        //brain coral
                        if (WorldGen.genRand.NextBool(10))
                        {
                            WorldGen.PlaceObject(X, Y - 1, (ushort)ModContent.TileType<BrainCoral>());
                        }
                        //small brain coral
                        if (WorldGen.genRand.NextBool(5))
                        {
                            WorldGen.PlaceObject(X, Y - 1, (ushort)ModContent.TileType<SmallBrainCoral>());
                        }

                        //tube coral
                        if (WorldGen.genRand.NextBool(12))
                        {
                            WorldGen.PlaceObject(X, Y - 1, (ushort)ModContent.TileType<TubeCoral>());
                        }
                        //small tube coral
                        if (WorldGen.genRand.NextBool(8))
                        {
                            WorldGen.PlaceObject(X, Y - 1, (ushort)ModContent.TileType<SmallTubeCoral>());
                        }

                        //giant navystone piles
                        if (WorldGen.genRand.NextBool(15))
                        {
                            ushort[] GiantPiles = new ushort[] { (ushort)ModContent.TileType<GiantNavystone1>(), (ushort)ModContent.TileType<GiantNavystone2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(GiantPiles));
                        }

                        //small navystone piles
                        if (WorldGen.genRand.NextBool(10))
                        {
                            ushort[] Piles = new ushort[] { (ushort)ModContent.TileType<NavystonePile1>(), 
                            (ushort)ModContent.TileType<NavystonePile2>(), (ushort)ModContent.TileType<NavystonePile3>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(Piles));
                        }
                    }
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Navystone>())
                    {
                        //stalactites
                        if (WorldGen.genRand.NextBool(10))
                        {
                            ushort[] Stalactites = new ushort[] { (ushort)ModContent.TileType<SunkenStalactite1>(),
                            (ushort)ModContent.TileType<SunkenStalactite2>(), (ushort)ModContent.TileType<SunkenStalactite3>() };

                            WorldGen.PlaceObject(X, Y + 2, WorldGen.genRand.Next(Stalactites));
                        }
                        //small stalactites
                        if (WorldGen.genRand.NextBool(8))
                        {
                            WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<SunkenStalactitesSmall>(), true, false, -1, 0);
                        }

                        //stalagmites
                        if (WorldGen.genRand.NextBool(12))
                        {
                            ushort[] Stalagmites = new ushort[] { (ushort)ModContent.TileType<SunkenStalagmite1>(),
                            (ushort)ModContent.TileType<SunkenStalagmite2>(), (ushort)ModContent.TileType<SunkenStalagmite3>() };

                            WorldGen.PlaceObject(X, Y - 2, WorldGen.genRand.Next(Stalagmites));
                        }
                        //small stalagmites
                        if (WorldGen.genRand.NextBool(10))
                        {
                            WorldGen.PlaceTile(X, Y - 1, (ushort)ModContent.TileType<SunkenStalagmitesSmall>(), true, false, -1, 0);
                        }

                        //giant navystone piles
                        if (WorldGen.genRand.NextBool())
                        {
                            ushort[] GiantPiles = new ushort[] { (ushort)ModContent.TileType<GiantNavystone1>(), (ushort)ModContent.TileType<GiantNavystone2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(GiantPiles));
                        }

                        //small navystone piles
                        if (WorldGen.genRand.NextBool())
                        {
                            ushort[] Piles = new ushort[] { (ushort)ModContent.TileType<NavystonePile1>(), 
                            (ushort)ModContent.TileType<NavystonePile2>(), (ushort)ModContent.TileType<NavystonePile3>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(Piles));
                        }
                    }

                    //radiant reefs ambient tiles
                    if (Main.tile[X, Y].TileType == ModContent.TileType<EutrophicSand>())
                    {
                        //multi-colored corals
                        if (WorldGen.genRand.NextBool(3))
                        {
                            ushort[] ColoredCorals = new ushort[] { (ushort)ModContent.TileType<CoralPileGiant>(), 
                            (ushort)ModContent.TileType<CoralPileLarge>(), (ushort)ModContent.TileType<MediumCoral2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(ColoredCorals));
                        }

                        //blue coral trees
                        if (WorldGen.genRand.NextBool(6))
                        {
                            ushort[] BlueCorals = new ushort[] { (ushort)ModContent.TileType<MediumCoral3>(), (ushort)ModContent.TileType<BlueCoralTree>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(BlueCorals));
                        }

                        //brown coral trees
                        if (WorldGen.genRand.NextBool(6))
                        {
                            ushort[] BrownCorals = new ushort[] { (ushort)ModContent.TileType<BrownCoral1>(), (ushort)ModContent.TileType<BrownCoral2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(BrownCorals));
                        }

                        //fan coral
                        if (WorldGen.genRand.NextBool(10))
                        {
                            WorldGen.PlaceObject(X, Y - 1, (ushort)ModContent.TileType<FanCoral>());
                        }

                        //misc corals
                        if (WorldGen.genRand.NextBool())
                        {
                            ushort[] MiscCorals = new ushort[] { (ushort)ModContent.TileType<MediumCoral>(), 
                            (ushort)ModContent.TileType<SmallWideCoral>(), (ushort)ModContent.TileType<SmallWideCoral2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(MiscCorals));
                        }
                    }

                    /*
                    //wall corals
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>())
                    {   
                        if (WorldGen.genRand.NextBool(8) && !Main.tile[X - 1, Y].HasTile)
                        {
                            ushort[] WallCorals = new ushort[] { (ushort)ModContent.TileType<TableCoral>(), (ushort)ModContent.TileType<WallCorals>() };

                            WorldGen.PlaceTile(X - 2, Y, WorldGen.genRand.Next(WallCorals), true, false, -1, 0);
                        }

                        if (WorldGen.genRand.NextBool(8) && !Main.tile[X + 1, Y].HasTile)
                        {
                            ushort[] WallCorals = new ushort[] { (ushort)ModContent.TileType<TableCoral>(), (ushort)ModContent.TileType<WallCorals>() };

                            WorldGen.PlaceTile(X + 2, Y, WorldGen.genRand.Next(WallCorals), true, false, -1, 0);
                        }
                    }
                    */
                }
            }
        }

        public static void PlaceGeode(int X, int Y, int radius)
        {
            ShapeData circle1 = new ShapeData();
            ShapeData circle2 = new ShapeData();
            ShapeData circle3 = new ShapeData();

            GenAction blotchMod = new Modifiers.Blotches(2, 0.4);

            //first circle of navystone
            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(radius), Actions.Chain(new GenAction[]
            {
                blotchMod.Output(circle1)
            }));
            WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle1), Actions.Chain(new GenAction[]
            {
                new Actions.ClearTile(),
                new Actions.PlaceTile((ushort)ModContent.TileType<Navystone>())
            }));

            //second circle of prisms
            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(radius - 4), Actions.Chain(new GenAction[]
            {
                blotchMod.Output(circle2)
            }));
            WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle2), Actions.Chain(new GenAction[]
            {
                new Actions.ClearTile(),
                new Actions.PlaceTile((ushort)ModContent.TileType<SeaPrism>())
            }));

            //clear out the middle of the circle
            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(radius - 9), Actions.Chain(new GenAction[]
            {
                blotchMod.Output(circle3)
            }));
            WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle3), Actions.Chain(new GenAction[]
            {
                new Actions.ClearTile(),
                new Actions.SetLiquid(),
                new Actions.PlaceWall((ushort)ModContent.WallType<NavystoneWall>())
            }));
        }

        //this method basically checks how many tiles are in a set square, and if there is enough then allow sand to be placed on the ground
        public static bool EnoughTilesInArea(int X, int Y, int halfWidth, int height, int Threshold, bool GeodeCheck)
        {
            int numTiles = 0;

            for (int i = X - halfWidth; i <= X + halfWidth; i++)
            {
                for (int j = Y; j <= Y + height; j++)
                {
                    if (Main.tile[i, j].HasTile)
                    {
                        numTiles++;
                    }

                    //GeodeCheck specificially makes sure there are no sea prisms in the area while placing a geode to prevent overlapping
                    if (GeodeCheck && Main.tile[i, j].TileType == ModContent.TileType<SeaPrism>())
                    {
                        return false;
                    }
                }
            }

            if (numTiles >= Threshold)
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

            if (collapse)
            {
                float distY = center.Y - point.Y;
                point.Y -= distY * 3f;
            }

            float distance1 = Vector2.Distance(point, focus1);
            float distance2 = Vector2.Distance(point, focus2);
            distance = distance1 + distance2;

            return distance <= distanceConstant;
        }

        //method to clean up small clumps of tiles (taken from the sulphur sea generation)
        public static void CleanOutSmallClumps()
        {
            List<ushort> blockTileTypes = new()
            {
                (ushort)ModContent.TileType<Navystone>(),
                (ushort)ModContent.TileType<EutrophicSand>(),
                (ushort)ModContent.TileType<HardenedEutrophicSand>(),
                (ushort)ModContent.TileType<Limestone>(),
                (ushort)ModContent.TileType<PolypSand>(),
                (ushort)ModContent.TileType<Shellstone>(),
                (ushort)ModContent.TileType<Basalt>(),
                (ushort)ModContent.TileType<SeaPrism>(),
            };
            
            void getAttachedPoints(int x, int y, List<Point> points)
            {
                Tile t = CalamityUtils.ParanoidTileRetrieval(x, y);
                Point p = new(x, y);
                
                if (!blockTileTypes.Contains(t.TileType) || !t.HasTile || points.Count > 75 || points.Contains(p))
                {
                    return;
                }

                points.Add(p);

                getAttachedPoints(x + 1, y, points);
                getAttachedPoints(x - 1, y, points);
                getAttachedPoints(x, y + 1, points);
                getAttachedPoints(x, y - 1, points);
            }

            for (int x = 20; x < Main.maxTilesX - 20; x++)
            {
                for (int y = 20; y < Main.maxTilesY - 20; y++)
                {
                    List<Point> chunkPoints = new();
                    getAttachedPoints(x, y, chunkPoints);

                    int cutoffLimit = 75;
                    if (chunkPoints.Count >= 1 && chunkPoints.Count < cutoffLimit)
                    {
                        foreach (Point p in chunkPoints)
                        {
                            WorldUtils.Gen(p, new Shapes.Rectangle(1, 1), Actions.Chain(new GenAction[]
                            {
                                new Actions.ClearTile(true),
                                new Actions.SetLiquid()
                            }));
                        }
                    }
                }
            }
        }
    }
}
