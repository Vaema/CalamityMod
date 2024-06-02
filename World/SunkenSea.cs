using System;
using System.Collections.Generic;
using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace CalamityMod.World
{
    public class SunkenSea
    {
        /*
        Dylandoe checklist:
        
        -Make ambient tiles naturally grow on sunken sea tiles/sands (only eutrophic sand is done)
        -Add basalt slab block
        -Add navystone brick wall  
        -Add the wall corals properly
        -Make sunken sea walls produce water
        */

        public static void PlaceTimelessShores(int startPosX, int startPosY)
        {
            int biomeSize = 250 + (Main.maxTilesX / 180);

            //place a box of circles to serve as the main "area" for the biome
            for (int X = startPosX - biomeSize; X <= startPosX + biomeSize; X += 25)
            {
                for (int Y = startPosY - 50; Y <= startPosY + 50; Y += 20)
                {
                    ShapeData circle = new ShapeData();
                    GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                    
                    WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(25), Actions.Chain(new GenAction[]
                    {
                        blotchMod.Output(circle)
                    }));
                    WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                    {
                        new Actions.Clear(),
                        new Actions.PlaceTile((ushort)ModContent.TileType<Runestone>())
                    }));
                }
            }

            //clear a smaller square out of the center where the biomes stuff will be
            for (int X = startPosX - biomeSize + 35; X <= startPosX + biomeSize - 35; X++)
            {
                for (int Y = startPosY - 50; Y <= startPosY + 50; Y++)
                {
                    ShapeData circle = new ShapeData();
                    GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                    
                    WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(2, 5)), Actions.Chain(new GenAction[]
                    {
                        blotchMod.Output(circle)
                    }));
                    WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                    {
                        new Actions.Clear()
                    }));
                }
            }

            //place little islands
            for (int MoundX = startPosX - biomeSize + 35; MoundX <= startPosX + biomeSize - 35; MoundX += WorldGen.genRand.Next(60, 120))
            {
				int MoundY = startPosY + 60;

				ShapeData mound = new ShapeData();
				GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
				WorldUtils.Gen(new Point(MoundX, MoundY), new Shapes.Mound(30, 45), Actions.Chain(new GenAction[]
				{
					blotchMod.Output(mound)
				}));
				WorldUtils.Gen(new Point(MoundX, MoundY), new ModShapes.All(mound), Actions.Chain(new GenAction[]
				{
					new Actions.Clear(), new Actions.PlaceTile((ushort)ModContent.TileType<Runestone>())
				}));

                //place a wall pillar above an island
                PlaceShoresWallPillar(startPosY, MoundX);
			}

            //clear the tops off of the mounds so they have flat surfaces the player can walk on
            for (int MoundClearX = startPosX - biomeSize + 37; MoundClearX <= startPosX + biomeSize - 37; MoundClearX++)
            {
                for (int MoundClearY = startPosY - 35; MoundClearY <= startPosY + 35; MoundClearY++)
                {
                    WorldGen.KillTile(MoundClearX, MoundClearY);
                }
            }

            //place water below the clear barrier for the islands so that theres water inbetween them
            for (int WaterX = startPosX - biomeSize + 35; WaterX <= startPosX + biomeSize - 35; WaterX++)
            {
                for (int WaterY = startPosY + 37; WaterY <= startPosY + 50; WaterY++)
                {
                    Main.tile[WaterX, WaterY].Get<LiquidData>().LiquidType = LiquidID.Water;
                    Main.tile[WaterX, WaterY].LiquidAmount = byte.MaxValue;
                }
            }

            //slope tiles
            for (int X = startPosX - biomeSize; X <= startPosX + biomeSize; X++)
            {
                for (int Y = startPosY - 50; Y <= startPosY + 50; Y++)
                {
                    Tile.SmoothSlope(X, Y);
                }
            }

            //place layer of sand on valid runestone blocks
            for (int X = startPosX - biomeSize; X <= startPosX + biomeSize; X++)
            {
                for (int Y = startPosY - 50; Y <= startPosY + 50; Y++)
                {
                    bool canPlaceSand = false;

                    if (Main.tile[X, Y].TileType == ModContent.TileType<Runestone>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile && 
                    !Main.tile[X, Y - 3].HasTile && !Main.tile[X, Y - 4].HasTile && !Main.tile[X, Y - 5].HasTile)
                    {
                        canPlaceSand = true;
                    }

                    if (canPlaceSand)
                    {
                        PlaceSand(X, Y, 5, ModContent.TileType<RuneSand>());
                    }
                }
            }
        }

        //places a pillar of walls along the height of the timeless shores biome
        public static void PlaceShoresWallPillar(int startPosY, int XPosition)
        {
            for (int Y = startPosY - 55; Y <= startPosY + 50; Y++)
            {
                int RandomX =  + WorldGen.genRand.Next(-8, 8);

                ShapeData circle = new ShapeData();
                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                WorldUtils.Gen(new Point(XPosition + RandomX, Y), new Shapes.Circle(WorldGen.genRand.Next(3, 7)), Actions.Chain(new GenAction[]
                {
                    blotchMod.Output(circle)
                }));
                WorldUtils.Gen(new Point(XPosition + RandomX, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                {
                    new Actions.PlaceWall((ushort)ModContent.WallType<RunestoneWall>())
                }));
            }
        }

        //sides of the sunken sea (radiant reefs)
        //LeftSideBarrier is used to make the barrier of basalt either on the left or right, since the sunken sea is meant to have barriers on both sides and the reefs is generated twice
        public static void PlaceRadiantReefs(int startPosX, int startPosY, bool LeftSideBarrier)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int biomeSize = 180 + (Main.maxTilesX / 180);
            float actualSize = biomeSize * 16f;
            float constant = actualSize * 2f / (float)Math.Sin(angle);

            float fociSpacing = actualSize * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //first, place a basalt barrier on the left and right of the biome
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.99f;

                        if (percent > blurPercent)
                        {
                            if (Y > origin.Y - 60)
                            {
                                if (LeftSideBarrier)
                                {
                                    if (X <= origin.X && Main.tile[X - 40, Y].TileType != ModContent.TileType<Basalt>())
                                    {
                                        ShapeData circle = new ShapeData();
                                        GenAction blotchMod = new Modifiers.Blotches(2, 0.4);

                                        WorldUtils.Gen(new Point(X - 40, Y), new Shapes.Circle(60), Actions.Chain(new GenAction[]
                                        {
                                            blotchMod.Output(circle)
                                        }));
                                        WorldUtils.Gen(new Point(X - 40, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                        {
                                            new Actions.Clear(),
                                            new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>())
                                        }));
                                    }
                                }
                                else
                                {
                                    if (X >= origin.X && Main.tile[X + 40, Y].TileType != ModContent.TileType<Basalt>())
                                    {
                                        ShapeData circle = new ShapeData();
                                        GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                                        
                                        WorldUtils.Gen(new Point(X + 40, Y), new Shapes.Circle(60), Actions.Chain(new GenAction[]
                                        {
                                            blotchMod.Output(circle)
                                        }));
                                        WorldUtils.Gen(new Point(X + 40, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
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

            //generate the actual caverns
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.99f;

                        if (percent > blurPercent)
                        {
                            //place smaller shellstone clumps infront of the basalt so the basalt isnt actually inside of the biome itself
                            WorldGen.TileRunner(X, Y, WorldGen.genRand.Next(8, 12), WorldGen.genRand.Next(8, 12), ModContent.TileType<Shellstone>(), true, 0f, 0f, true, true);
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
                            float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.280f;

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
                            float caveCreationThresholdWalls = horizontalOffsetNoiseWalls * 3.5f + 0.280f;

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
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
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

            //place layer of sand blocks on valid surfaces
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        bool canPlaceSand = false;

                        //place sand clumps on top of exposed shellstone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile && 
                        !Main.tile[X, Y - 3].HasTile && !Main.tile[X, Y - 4].HasTile && !Main.tile[X, Y - 5].HasTile)
                        {
                            canPlaceSand = true;
                        }

                        if (canPlaceSand)
                        {
                            PlaceSand(X, Y, 5, ModContent.TileType<EutrophicSand>());
                        }
                    }
                }
            }

            //cleanup again
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
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
        public static void PlacePolypForest(int startPosX, int startPosY)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int biomeSize = 120 + (Main.maxTilesX / 180);
            float actualSize = biomeSize * 16f;
            float constant = actualSize * 2f / (float)Math.Sin(angle);

            float fociSpacing = actualSize * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //place the polyp forest caverns
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.98f;

                        //biome "blending" on the edges (disabled for now)
                        if (percent > blurPercent)
                        {
                            if (Main.tile[X, Y].HasTile && Main.tile[X, Y].TileType != ModContent.TileType<Limestone>())
                            {
                                ShapeData circle = new ShapeData();
                                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                                WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(1, 3)), Actions.Chain(new GenAction[]
                                {
                                    blotchMod.Output(circle)
                                }));

                                WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                {
                                    new Actions.ClearTile(), new Actions.PlaceTile((ushort)ModContent.TileType<Limestone>())
                                }));
                            }
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
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
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
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        bool canPlaceSand = false;

                        //place sand clumps on top of exposed limestone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Limestone>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile && 
                        !Main.tile[X, Y - 3].HasTile && !Main.tile[X, Y - 4].HasTile && !Main.tile[X, Y - 5].HasTile)
                        {
                            canPlaceSand = true;
                        }

                        if (canPlaceSand)
                        {
                            PlaceSand(X, Y, 5, ModContent.TileType<PolypSand>());
                        }
                    }
                }
            }
 
            //cleanup again
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
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
        public static void PlaceGleamingBurrows(int startPosX, int startPosY)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int biomeSize = 220 + (Main.maxTilesX / 180);
            float actualSize = biomeSize * 16f;
            float constant = actualSize * 2f / (float)Math.Sin(angle);

            float fociSpacing = actualSize * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //place another barrier so the gleaming burrows doesnt just burst into the gully
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X += 10)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y += 2)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.98f;

                        if (percent > blurPercent)
                        {
                            if (Y > origin.Y + 20)
                            {
                                ShapeData circle = new ShapeData();
                                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                                
                                WorldUtils.Gen(new Point(X, Y + 10), new Shapes.Circle(20), Actions.Chain(new GenAction[]
                                {
                                    blotchMod.Output(circle)
                                }));
                                WorldUtils.Gen(new Point(X, Y + 10), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                {
                                    new Actions.Clear(),
                                    new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>())
                                }));
                            }
                        }
                    }
                }
            }

            //place the gleaming burrows caverns
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
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

                            //place clean transition between the burrows and the other biomes
                            if (Y < origin.Y && Main.tile[X, Y].HasTile && Main.tile[X, Y].TileType != ModContent.TileType<Navystone>())
                            {
                                ShapeData circle = new ShapeData();
                                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                                WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(1, 3)), Actions.Chain(new GenAction[]
                                {
                                    blotchMod.Output(circle)
                                }));

                                WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                {
                                    new Actions.ClearTile(), new Actions.PlaceTile((ushort)ModContent.TileType<Navystone>())
                                }));
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
                            float caveCreationThresholdWalls = horizontalOffsetNoiseWalls * 5.5f + 0.235f;

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
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
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
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        bool canPlaceSand = false;

                        //place sand clumps on top of exposed navystone
                        if (Main.tile[X, Y].TileType == ModContent.TileType<Navystone>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile && !Main.tile[X, Y - 3].HasTile)
                        {
                            canPlaceSand = true;
                        }

                        if (canPlaceSand)
                        {
                            PlaceSand(X, Y, 4, ModContent.TileType<HardenedEutrophicSand>());
                        }
                    }
                }
            }

            //cleanup again, also place geodes
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                    {
                        //place geodes
                        if (WorldGen.genRand.NextBool(800) && Main.tile[X, Y].TileType == ModContent.TileType<Navystone>())
                        {
                            PlaceGeode(X, Y, WorldGen.genRand.Next(10, 17));
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
        public static void PlaceBasaltGully(int startPosX, int startPosY)
        {
            int biomeSize = 230 + (Main.maxTilesX / 180);

            int XLeft = GenVars.UndergroundDesertLocation.Left;
            int XRight = GenVars.UndergroundDesertLocation.Right;

            //place circles of basalt along the 2 edges of the area
            for (int Y = startPosY - 30; Y <= Main.maxTilesY - 260; Y += 20)
            {
                ShapeData circle = new ShapeData();
                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);

                int radius = WorldGen.genRand.Next(30, 45);
                int RandomX = Main.rand.Next(-25, 25);
                
                WorldUtils.Gen(new Point(XLeft, Y), new Shapes.Circle(radius), Actions.Chain(new GenAction[]
                {
                    blotchMod.Output(circle)
                }));

                WorldUtils.Gen(new Point(XLeft, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                {
                    new Actions.Clear(),
                    new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>()),
                    new Actions.PlaceWall(WallID.LavaUnsafe1)
                }));

                WorldUtils.Gen(new Point(XRight, Y), new Shapes.Circle(radius), Actions.Chain(new GenAction[]
                {
                    blotchMod.Output(circle)
                }));

                WorldUtils.Gen(new Point(XRight, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                {
                    new Actions.Clear(),
                    new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>()),
                    new Actions.PlaceWall(WallID.LavaUnsafe1)
                }));
            }

            //place blocks of basalt all the way down to hell, here it just places one block and wall so it doesnt hurt preformance
            for (int X = startPosX - biomeSize - 25; X <= startPosX + biomeSize + 25; X++)
            {
                for (int Y = startPosY - 30; Y <= Main.maxTilesY - 250; Y++)
                {
                    Main.tile[X, Y].ClearEverything();
                    WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Basalt>());
                    WorldGen.PlaceWall(X, Y, WallID.LavaUnsafe1);
                }
            }

            //place another wall of circles along the bottom of the biome so it doesnt just end unnaturally
            for (int X = startPosX - biomeSize; X <= startPosX + biomeSize; X += 20)
            {
                int Y = Main.maxTilesY - 250;

                ShapeData circle = new ShapeData();
                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                
                WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(25), Actions.Chain(new GenAction[]
                {
                    blotchMod.Output(circle)
                }));

                WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                {
                    new Actions.Clear(),
                    new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>()),
                    new Actions.PlaceWall(WallID.LavaUnsafe1)
                }));
            }

            //place caverns and lava
            for (int X = startPosX - biomeSize - (Main.maxTilesX / 25); X <= startPosX + biomeSize + (Main.maxTilesX / 25); X++)
            {
                for (int Y = startPosY; Y <= Main.maxTilesY - 210; Y++)
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
            for (int X = startPosX - biomeSize - (Main.maxTilesX / 25); X <= startPosX + biomeSize + (Main.maxTilesX / 25); X++)
            {
                for (int Y = startPosY; Y <= Main.maxTilesY - 210; Y++)
                {
                    bool canPlaceSand = false;

                    //place sand clumps on top of exposed basalt
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Basalt>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile)
                    {
                        canPlaceSand = true;
                    }

                    if (canPlaceSand)
                    {
                        PlaceSand(X, Y, 3, ModContent.TileType<VolcanicSand>());
                    }
                }
            }

            //cleanup
            for (int X = startPosX - biomeSize - (Main.maxTilesX / 25); X <= startPosX + biomeSize + (Main.maxTilesX / 25); X++)
            {
                for (int Y = startPosY; Y <= Main.maxTilesY - 210; Y++)
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

        //cleanup is done separately because for whatever reason it keeps placing water inside of the gully if the cleanup is done before it
        public static void BasaltGullyLavaCleanup(int startPosX, int startPosY)
        {
            int biomeSize = 230 + (Main.maxTilesX / 180);

            for (int X = startPosX - biomeSize - (Main.maxTilesX / 25); X <= startPosX + biomeSize + (Main.maxTilesX / 25); X++)
            {
                for (int Y = startPosY + 50; Y <= Main.maxTilesY - 200; Y++)
                {
                    if (Main.tile[X, Y].WallType == WallID.LavaUnsafe1)
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

            //just loop through the whole world and check for the specific tiles in the sunken sea because im lazy
            for (int X = 20; X <= Main.maxTilesX - 20; X++)
            {
                for (int Y = 20; Y <= Main.maxTilesY - 20; Y++)
                {
                    //place coral blobs in the radiant reefs
                    if (WorldGen.genRand.NextBool(100) && ((Main.tile[X, Y].TileType == ModContent.TileType<EutrophicSand>() && !Main.tile[X, Y - 1].HasTile) ||
                    (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>() && !Main.tile[X, Y + 1].HasTile)))
                    {
                        ushort[] Corals = new ushort[] { (ushort)ModContent.TileType<CyanCoral>(), (ushort)ModContent.TileType<LimeCoral>(),
                        (ushort)ModContent.TileType<MagentaCoral>(), (ushort)ModContent.TileType<OrangeCoral>(), (ushort)ModContent.TileType<YellowCoral>() };

                        ShapeData circle = new ShapeData();
                        GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                        WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(2, 4)), Actions.Chain(new GenAction[]
                        {
                            blotchMod.Output(circle)
                        }));

                        WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                        {
                            new Actions.ClearTile(), new Actions.PlaceTile(WorldGen.genRand.Next(Corals))
                        }));
                    }

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

                        //anemonies
                        if (WorldGen.genRand.NextBool(10))
                        {
                            WorldGen.PlaceObject(X, Y - 1, (ushort)ModContent.TileType<SeaAnemone>());
                        }

                        //giant navystone piles
                        if (WorldGen.genRand.NextBool(10))
                        {
                            ushort[] GiantPiles = new ushort[] { (ushort)ModContent.TileType<GiantNavystone1>(), (ushort)ModContent.TileType<GiantNavystone2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(GiantPiles));
                        }

                        //small navystone piles
                        if (WorldGen.genRand.NextBool(5))
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
                        if (WorldGen.genRand.NextBool(4))
                        {
                            ushort[] BlueCorals = new ushort[] { (ushort)ModContent.TileType<MediumCoral3>(), (ushort)ModContent.TileType<BlueCoralTree>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(BlueCorals));
                        }

                        //brown coral trees
                        if (WorldGen.genRand.NextBool(4))
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

                    //grow depth vines on navystone
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Navystone>() && Main.tile[X, Y].Slope == 0 && !Main.tile[X, Y + 1].HasTile && !Main.tile[X, Y + 2].HasTile)
                    {
                        if (WorldGen.genRand.NextBool(7))
                        {
                            WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<DepthVines>());
                        }
                    }
                    if (Main.tile[X, Y].TileType == ModContent.TileType<DepthVines>())
                    {
                        CalamityUtils.GrowVines(X, Y, WorldGen.genRand.Next(1, 4), (ushort)ModContent.TileType<DepthVines>());
                    }

                    //wall corals
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>())
                    {   
                        if (WorldGen.genRand.NextBool(5) && !Main.tile[X + 1, Y].HasTile)
                        {
                            ushort[] WallCorals = new ushort[] { (ushort)ModContent.TileType<WallCoral1>(), (ushort)ModContent.TileType<WallCoral2>(), 
                            (ushort)ModContent.TileType<WallCoral3>(), (ushort)ModContent.TileType<WallCoral4>(), (ushort)ModContent.TileType<TableCoral>(), 
                            (ushort)ModContent.TileType<TableCoral2>(), (ushort)ModContent.TileType<TableCoral3>() };

                            WorldGen.PlaceTile(X + 2, Y, WorldGen.genRand.Next(WallCorals), true, false, -1, 0);
                        }

                        if (WorldGen.genRand.NextBool(5) && !Main.tile[X - 1, Y].HasTile)
                        {
                            ushort[] WallCorals = new ushort[] { (ushort)ModContent.TileType<WallCoral1>(), (ushort)ModContent.TileType<WallCoral2>(), 
                            (ushort)ModContent.TileType<WallCoral3>(), (ushort)ModContent.TileType<WallCoral4>(), (ushort)ModContent.TileType<TableCoral>(), 
                            (ushort)ModContent.TileType<TableCoral2>(), (ushort)ModContent.TileType<TableCoral3>() };

                            WorldGen.PlaceTile(X - 2, Y, WorldGen.genRand.Next(WallCorals), true, false, -1, 0);
                        }
                    }
                }
            }
        }

        //check if theres enough tiles to place sand below
        public static bool PlaceSand(int X, int Y, int height, int tileType)
        {
            for (int j = Y; j <= Y + height; j++)
            {
                if (Main.tile[X, j].HasTile && Main.tile[X, j + 1].HasTile && Main.tile[X, j + 2].HasTile && Main.tile[X, j + 3].HasTile &&
                Main.tile[X - 1, j + 3].HasTile && Main.tile[X + 1, j + 3].HasTile)
                {
                    Main.tile[X, j].TileType = (ushort)tileType;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        //placing sea prism geodes
        public static bool PlaceGeode(int X, int Y, int radius)
        {
            int crystalNearby = 0;

            //check a 20 by 20 square for other geodes before placing
            for (int i = X - 20; i < X + 20; i++)
            {
                for (int j = Y - 20; j < Y + 20; j++)
                {
                    //dont allow geodes to place if another one is too close
                    if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == ModContent.TileType<SeaPrism>())
                    {
                        crystalNearby++;
                        if (crystalNearby > 0)
                        {
                            return false;
                        }
                    }
                }
            }

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

            return true;
        }

        //method to clean up small clumps of tiles (taken from the sulphur sea generation)
        public static void CleanOutSmallClumps()
        {
            List<ushort> blockTileTypes = new()
            {
                (ushort)ModContent.TileType<Shellstone>(),
                (ushort)ModContent.TileType<EutrophicSand>(),
                (ushort)ModContent.TileType<Limestone>(),
                (ushort)ModContent.TileType<PolypSand>(),
                (ushort)ModContent.TileType<Navystone>(),
                (ushort)ModContent.TileType<HardenedEutrophicSand>(),
                (ushort)ModContent.TileType<SeaPrism>(),
                (ushort)ModContent.TileType<Basalt>(),
                (ushort)ModContent.TileType<VolcanicSand>(),
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

        //method to make sure things only generate in each biome circle
        public static bool CheckInBiomeArea(Point tile, Vector2 focus1, Vector2 focus2, float distanceConstant, Vector2 center, out float distance, bool collapse = false)
        {
            Vector2 point = tile.ToWorldCoordinates();

            if (collapse)
            {
                float distY = center.Y - point.Y;
                point.Y -= distY * 4f;
            }

            float distance1 = Vector2.Distance(point, focus1);
            float distance2 = Vector2.Distance(point, focus2);
            distance = distance1 + distance2;

            return distance <= distanceConstant;
        }
    }
}
