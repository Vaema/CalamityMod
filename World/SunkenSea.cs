using System;
using System.Collections.Generic;
using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using ReLogic.Threading;
using Terraria;
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

        /*
         ena todo

        fix the fucking forest being shaved
        cut a half oval out of the top of forest in anticipation of structure placement
        change the type of noise both reefs and forest use (reefs more jagged, forest more flat)
        cut a curve out of the top of shores (the desert part)
        extend basalt to hell - DONE
            convert normal terrain blocks below basalt into basalt
        BLOTCHES EVERYWHERE
         */
        public static void PlaceTimelessShores(int startPosX, int startPosY)
        {
            int biomeSize = 250 + (Main.maxTilesX / 180);

            //
            // Places a trapezoid of circles to serve as the main area for the biome.
            //
            const float steepness = 15f;
            static float trapezoidLateralSteep(float x)
            {
                if (x <= 1f / steepness)
                    return steepness * x;
                else if (x > 1f / steepness && x < 1f - 1f / steepness)
                    return 1f;
                else
                    return -steepness * x + steepness;
            }
            FastParallel.For(startPosX - biomeSize - 20, startPosX + biomeSize + 20, (start, end, _) =>
            {
                for (int X = start; X <= end; X += 15)
                {
                    float height = MathHelper.Lerp(
                        startPosY + 50,
                        startPosY - 35,
                        trapezoidLateralSteep(Utils.GetLerpValue(startPosX - biomeSize - 20, startPosX + biomeSize + 20, X)));

                    for (int Y = startPosY + 50; Y >= height; Y -= 10)
                    {
                        ShapeData circle = new ShapeData();
                        WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(30), Actions.Chain(new GenAction[]
                        {
                            //new Modifiers.RadialDither(1, 30).Output(circle),
                            new Modifiers.Blotches().Output(circle),
                        }));
                        WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                    }
                }
            });

            //
            // Makes the bottom borders of the Timeless Shores curved downwards.
            //
            const int totalCuveDepth = 40;
            FastParallel.For(startPosX - biomeSize - 20, startPosX - biomeSize + 200, (start, end, _) =>
            {
                for (int x = start; x <= end; x += 10)
                {
                    int curveDepth = (int)MathHelper.Lerp(
                        totalCuveDepth,
                        0f,
                        MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize - 17, startPosX - biomeSize + 200, x, true)));

                    for (int y = startPosY + 50; y <= startPosY + 50 + curveDepth; y += 5)
                    {
                        ShapeData circle = new();
                        WorldUtils.Gen(new Point(x, y), new Shapes.Circle(30), Actions.Chain(new GenAction[]
                        {
                            //new Modifiers.RadialDither(1, 30).Output(circle),
                            new Modifiers.Blotches().Output(circle),
                        }));
                        WorldUtils.Gen(new Point(x, y), new ModShapes.All(circle), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                    }
                }
            });
            FastParallel.For(startPosX + biomeSize - 200, startPosX + biomeSize + 20, (start, end, _) =>
            {
                for (int x = start; x <= end; x += 5)
                {
                    int curveDepth = (int)MathHelper.Lerp(
                        totalCuveDepth,
                        0f,
                        MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize + 20, startPosX + biomeSize - 200, x, true)));

                    for (int y = startPosY + 50; y <= startPosY + 50 + curveDepth; y += 5)
                    {
                        ShapeData circle = new();
                        WorldUtils.Gen(new Point(x, y), new Shapes.Circle(30), Actions.Chain(new GenAction[]
                        {
                            //new Modifiers.RadialDither(1, 30).Output(circle),
                            new Modifiers.Blotches().Output(circle),
                        }));
                        WorldUtils.Gen(new Point(x, y), new ModShapes.All(circle), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                    }
                }
            });

            //clear a smaller square out of the center where the biomes stuff will be
            FastParallel.For(startPosX - biomeSize + 35, startPosX + biomeSize - 35, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
                {
                    for (int Y = startPosY - 65; Y <= startPosY + 50; Y++)
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
            });

            //
            // Places slopes at the sides of the shores to actually make shores.
            //
            FastParallel.For(startPosX - biomeSize + 20, startPosX - biomeSize + 200, (start, end, _) =>
            {
                for (int moundX = start; moundX <= end; moundX += 20)
                {
                    ShapeData mound = new();
                    GenAction blotchMod = new Modifiers.Blotches().Output(mound);
                    int moundY = startPosY + 60;
                    int moundHeight = (int)MathHelper.Lerp(60f, 5f, MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize + 27, startPosX - biomeSize + 200, moundX)));

                    WorldUtils.Gen(new Point(moundX, moundY), new Shapes.Mound(30, moundHeight), blotchMod);
                    WorldUtils.Gen(new Point(moundX, moundY), new ModShapes.All(mound), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                }
            });
            FastParallel.For(startPosX + biomeSize - 200, startPosX + biomeSize - 20, (start, end, _) =>
            {
                for (int moundX = start; moundX <= end; moundX += 20)
                {
                    ShapeData mound = new();
                    GenAction blotchMod = new Modifiers.Blotches().Output(mound);
                    int moundY = startPosY + 60;
                    int moundHeight = (int)MathHelper.Lerp(60f, 5f, MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize - 27, startPosX + biomeSize - 200, moundX)));

                    WorldUtils.Gen(new Point(moundX, moundY), new Shapes.Mound(30, moundHeight), blotchMod);
                    WorldUtils.Gen(new Point(moundX, moundY), new ModShapes.All(mound), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                }
            });

            //
            // Makes a large platform in the middle of the Timeless Shores.
            //
            const int platformSize = 130;
            const int platformHeight = 35; // Remember that because of Terraria's coordinate system, making it lower actually makes it higher.
            const int platformDepth = 35; // How deep it goes. Like above, the higher it is, the lower it goes.
            FastParallel.For(startPosX - platformSize / 2, startPosX + platformSize / 2, (start, end, _) =>
            {
                for (int platformX = start; platformX <= end; platformX += 5)
                {
                    // This makes the "stem" of the platform be thinner towards the base and then expand on the top.
                    int thickness = (int)(MathF.Pow(CalamityUtils.Convert01To010(Utils.GetLerpValue(startPosX - platformSize / 2, startPosX + platformSize / 2, platformX)), 3f) * platformDepth) + platformHeight;

                    for (int platformY = startPosY + platformHeight; platformY <= startPosY + thickness; platformY++)
                    {
                        WorldUtils.Gen(new Point(platformX, platformY), new Shapes.Circle(5), Actions.Chain(new GenAction[]
                        {
                            new Actions.Clear(),                                                // Clears all tiles and walls.
                            new Actions.PlaceTile((ushort)ModContent.TileType<Runestone>())     // Places tiles.
                        }));
                    }
                }
            });
            FastParallel.For(startPosX - platformSize / 2, startPosX + platformSize / 2, (start, end, _) =>
            {
                for (int x = start; x <= end; x++)
                {
                    float interpolator = CalamityUtils.Convert01To010(Utils.GetLerpValue(startPosX - platformSize / 2, startPosX + platformSize / 2, x, true));
                    int elevation = (int)MathHelper.Lerp(4f, 1f, interpolator);
                    WorldUtils.Gen(new Point(x, startPosY + platformHeight - 7), new Shapes.Circle(elevation), Actions.Chain(new GenAction[] { new Actions.Clear(), }));
                }
            });

            //
            // Makes a hole into the actual Sunken Sea at the botoom-left and bottom-right of the middle platform of the Timeless Shores.
            //
            const int furthestDistanceFromCenter = 55;
            const int shortestDistanceFromCenter = 35;
            const int holeStart = 50;
            const int holeDepth = 35;
            FastParallel.For(startPosX - furthestDistanceFromCenter, startPosX - shortestDistanceFromCenter, (start, end, _) =>
            {
                for (int holeX = start; holeX <= end; holeX++)
                {
                    for (int holeY = startPosY + holeStart; holeY <= startPosY + holeStart + holeDepth; holeY += 3)
                    {
                        ShapeData circle = new();

                        // As the hole gets deeper, we want the holes to be bigger and more blurry, so it's not so monotonous.
                        int sizeIncrement = (int)Utils.Remap(holeY, startPosY + holeStart, startPosY + holeStart + holeDepth, 1, 20);

                        // may or may not be useful later
                        //GenAction dither = new Modifiers.RadialDither(2 * sizeIncrement, 5 * sizeIncrement);

                        WorldUtils.Gen(new Point(holeX, holeY), new Shapes.Circle(sizeIncrement), Actions.Chain(new GenAction[]
                        {
                            new Actions.ClearTile(),    // The shape removes tiles.
                            new Actions.SetLiquid()     // The shape adds liquid (Default to max water).
                        }));
                    }
                }
            });
            FastParallel.For(startPosX + shortestDistanceFromCenter, startPosX + furthestDistanceFromCenter, (start, end, _) =>
            {
                for (int holeX = start; holeX <= end; holeX++)
                {
                    for (int holeY = startPosY + holeStart; holeY <= startPosY + holeStart + holeDepth; holeY += 3)
                    {
                        ShapeData circle = new();

                        // As the hole gets deeper, we want the holes to be bigger and more blurry, so it's not so monotonous.
                        int sizeIncrement = (int)Utils.Remap(holeY, startPosY + holeStart, startPosY + holeStart + holeDepth, 1, 20);

                        WorldUtils.Gen(new Point(holeX, holeY), new Shapes.Circle(sizeIncrement), Actions.Chain(new GenAction[]
                        {
                            new Actions.ClearTile(),    // The shape removes tiles.
                            new Actions.SetLiquid()     // The shape adds liquid (Default to max water).
                        }));
                    }
                }
            });

            //place water below the clear barrier for the islands so that theres water inbetween them
            FastParallel.For(startPosX - biomeSize + 35, startPosX + biomeSize - 35, (start, end, _) =>
            {
                for (int WaterX = start; WaterX <= end; WaterX++)
                {
                    for (int WaterY = startPosY + 37; WaterY <= startPosY + 60; WaterY++)
                    {
                        Main.tile[WaterX, WaterY].Get<LiquidData>().LiquidType = LiquidID.Water;
                        Main.tile[WaterX, WaterY].LiquidAmount = byte.MaxValue;
                    }
                }
            });

            //slope tiles
            FastParallel.For(startPosX - biomeSize, startPosX + biomeSize, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
                {
                    for (int Y = startPosY - 50; Y <= startPosY + 50; Y++)
                    {
                        Tile.SmoothSlope(X, Y);
                    }
                }
            });

            //place layer of sand on valid runestone blocks
            FastParallel.For(startPosX - biomeSize, startPosX + biomeSize, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });

            //
            // Generates pillars made out of walls at random points in the Timeless Shores.
            //
            FastParallel.For(startPosX - biomeSize, startPosX + biomeSize, (start, end, _) =>
            {
                for (int wallPillarX = start; wallPillarX <= end; wallPillarX += 1000)
                {
                    int randomDisplacementX = WorldGen.genRand.Next(-10, 30);

                    for (int wallPillarY = startPosY - 75; wallPillarY <= startPosY + 70; wallPillarY += 10)
                    {
                        // Along the height of the pillar, it becomes shorter the more closer to the center we get.
                        int pillarWidth = (int)(CalamityUtils.Convert01To010(Utils.GetLerpValue(startPosY - 75, startPosY + 50, wallPillarY)) * 25);

                        ShapeData rectangle = new ShapeData();
                        GenAction blotchFilter = new Modifiers.Blotches(2, 0.4);
                        GenAction ditherFilter = new Modifiers.Dither(Math.Min(Utils.GetLerpValue(startPosY + 40, startPosY + 70, wallPillarY) + 0.25, 1));

                        WorldUtils.Gen(new Point(randomDisplacementX + wallPillarX + pillarWidth / 2, wallPillarY), new Shapes.Rectangle(25 - pillarWidth + 3, 12), Actions.Chain(new GenAction[]
                        {
                            ditherFilter.Output(rectangle),     // Applies a dithering filter to the rectangle.
                            blotchFilter.Output(rectangle),     // Applies a blotch filter to the rectangle.
                        }));

                        WorldUtils.Gen(new Point(randomDisplacementX + wallPillarX + pillarWidth / 2, wallPillarY), new ModShapes.All(rectangle), Actions.Chain(new GenAction[]
                        {
                            new Actions.PlaceWall((ushort)ModContent.WallType<RunestoneWall>()) // The shape places walls.
                        }));
                    }
                }
            });

            //
            // Makes the transition area to the Timeless Shores.
            // Replaces tiles, walls, and places some water spots.
            //
            FastParallel.For(startPosX - biomeSize - 25, startPosX + biomeSize + 25, (start, end, _) =>
            {
                for (int x = start; x <= end; x += 20)
                {
                    for (int y = startPosY - 70; y >= startPosY - 200; y -= 15)
                    {
                        float interpolator = Utils.GetLerpValue(startPosY - 90, startPosY - 150, y, true);
                        float ditherStrength = MathHelper.Lerp(0f, 0.95f, interpolator);

                        WorldUtils.Gen(new Point(x, y), new Shapes.Circle(15), Actions.Chain(new GenAction[]
                        {
                            new Modifiers.OnlyTiles(TileID.Sand, TileID.HardenedSand),
                            new Modifiers.Dither(ditherStrength + 0.2f),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<RuneSand>()),
                        }));
                        // changing these makes the walls dither higher up than tiles, to prevent the biome from changing earlier than expected. still looks nice imo :)
                        interpolator = Utils.GetLerpValue(startPosY - 90, startPosY - 200, y, true);
                        ditherStrength = MathHelper.Lerp(0f, 0.95f, interpolator);

                        WorldUtils.Gen(new Point(x, y), new Shapes.Circle(15), Actions.Chain(new GenAction[]
                        {
                            new Modifiers.OnlyWalls(WallID.Sandstone, WallID.HardenedSand),
                            new Modifiers.Dither(ditherStrength),
                            new Actions.ClearWall(),
                            new Actions.PlaceWall((ushort)ModContent.WallType<RunestoneWall>()),
                        }));

                        // i forgot to change this one. :peepotired:
                        interpolator = Utils.GetLerpValue(startPosY - 90, startPosY - 150, y, true);
                        ditherStrength = MathHelper.Lerp(0f, 0.95f, interpolator);
                        WorldUtils.Gen(new Point(x, y), new Shapes.Circle(15), Actions.Chain(new GenAction[]
                        {
                            new Modifiers.OnlyTiles(TileID.Sandstone),
                            new Modifiers.Dither(ditherStrength),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<Runestone>()),
                        }));

                        if (Main.tile[x, y].Get<LiquidData>().LiquidType == LiquidID.Lava)
                            WorldUtils.Gen(new Point(x, y), new Shapes.Rectangle(5, 5), new Actions.SetLiquid(LiquidID.Lava, 0));

                        if (WorldGen.genRand.NextBool(5) && y < startPosY - 100)
                        {
                            WorldUtils.Gen(new Point(x, y), new Shapes.Circle(WorldGen.genRand.Next(5, 7)), Actions.Chain(new GenAction[]
                            {
                                new Modifiers.IsNotSolid().Output(new()),
                                new Actions.SetLiquid(),
                            }));
                        }
                    }
                }
            });
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
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
                                        if (X <= origin.X && Main.tile[X - 40, Y - 25].TileType != ModContent.TileType<Basalt>())
                                        {
                                            ShapeData circle = new ShapeData();
                                            GenAction blotchMod = new Modifiers.Blotches(2, 0.4);

                                            WorldUtils.Gen(new Point(X - 40, Y - 25), new Shapes.Circle(60), Actions.Chain(new GenAction[]
                                            {
                                                blotchMod.Output(circle)
                                            }));
                                            WorldUtils.Gen(new Point(X - 40, Y - 25), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                            {
                                                new Actions.Clear(),
                                                new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>())
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        if (X >= origin.X && Main.tile[X + 40, Y - 25].TileType != ModContent.TileType<Basalt>())
                                        {
                                            ShapeData circle = new ShapeData();
                                            GenAction blotchMod = new Modifiers.Blotches(2, 0.4);

                                            WorldUtils.Gen(new Point(X + 40, Y - 25), new Shapes.Circle(60), Actions.Chain(new GenAction[]
                                            {
                                                blotchMod.Output(circle)
                                            }));
                                            WorldUtils.Gen(new Point(X + 40, Y - 25), new ModShapes.All(circle), Actions.Chain(new GenAction[]
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
            });

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
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });

            //place layer of sand blocks on valid surfaces
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });

            //cleanup again
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });
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
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, false, Y > origin.Y))
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
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
                {
                    for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                    {
                        if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, false, Y > origin.Y))
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
            });

            //place extra tiles
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
                {
                    for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 20; Y <= origin.Y + verticalRadius + 20; Y++)
                    {
                        if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, false, Y > origin.Y))
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
            });

            //cleanup again
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
                {
                    for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                    {
                        if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, false, Y > origin.Y))
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
            });
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
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X += 10)
                {
                    for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y += 2)
                    {
                        if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, true))
                        {
                            float percent = dist / constant;
                            float blurPercent = 0.98f;

                            if (percent > blurPercent)
                            {
                                if (Y > origin.Y - 10)
                                {
                                    ShapeData circle = new ShapeData();
                                    GenAction blotchMod = new Modifiers.Blotches(10, 0.4);

                                    WorldUtils.Gen(new Point(X, Y - 10), new Shapes.Circle(20), Actions.Chain(new GenAction[]
                                    {
                                        blotchMod.Output(circle)
                                    }));
                                    WorldUtils.Gen(new Point(X, Y - 10), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                    {
                                        new Actions.Clear(),
                                        new Actions.PlaceTile((ushort)ModContent.TileType<Basalt>())
                                    }));
                                }
                            }
                        }
                    }
                }
            });

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
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });

            //place extra tiles
            FastParallel.For(origin.X - biomeSize - 3, origin.X + biomeSize + 3, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });

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
            int biomeSize = 200 + (Main.maxTilesX / 180);

            int XLeft = GenVars.UndergroundDesertLocation.Left + 30;
            int XRight = GenVars.UndergroundDesertLocation.Right - 30;

            //place circles of basalt along the 2 edges of the area
            for (int Y = startPosY - 60; Y <= Main.maxTilesY - 230; Y += 20)
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
            FastParallel.For(startPosX - biomeSize - 25, startPosX + biomeSize + 25, (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
                {
                    for (int Y = startPosY - 30; Y <= Main.maxTilesY - 200; Y++)
                    {
                        Main.tile[X, Y].ClearEverything();
                        WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Basalt>());
                        WorldGen.PlaceWall(X, Y, WallID.LavaUnsafe1);
                    }
                }
            });

            //place another wall of circles along the bottom of the biome so it doesnt just end unnaturally
            FastParallel.For(startPosX - biomeSize, startPosX + biomeSize, (start, end, _) =>
            {
                for (int X = start; X <= end; X += 20)
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
            });

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
            FastParallel.For(startPosX - biomeSize - (Main.maxTilesX / 25), startPosX + biomeSize + (Main.maxTilesX / 25), (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });

            //cleanup
            FastParallel.For(startPosX - biomeSize - (Main.maxTilesX / 25), startPosX + biomeSize + (Main.maxTilesX / 25), (start, end, _) =>
            {
                for (int X = start; X <= end; X++)
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
            });
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

            FastParallel.For(20, Main.maxTilesX - 20, (start, end, _) =>
            {
                for (int x = start; x < end; x++)
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
            });
        }

        //method to make sure things only generate in each biome circle
        public static bool CheckInBiomeArea(Point tile, Vector2 focus1, Vector2 focus2, float distanceConstant, Vector2 center, out float distance, bool collapse = false, bool collapseBottom = false)
        {
            Vector2 point = tile.ToWorldCoordinates();

            if (collapse)
            {
                float distY = center.Y - point.Y;
                point.Y -= distY * 4f;
            }
            if (collapseBottom)
            {
                float distY = center.Y - point.Y;
                point.Y += distY * 4f;
            }
            float distance1 = Vector2.Distance(point, focus1);
            float distance2 = Vector2.Distance(point, focus2);
            distance = distance1 + distance2;

            return distance <= distanceConstant;
        }
    }
}
