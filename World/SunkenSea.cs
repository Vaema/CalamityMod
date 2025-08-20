using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using CalamityMod.Utilities;
using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using ReLogic.Threading;
using ReLogic.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace CalamityMod.World
{
    public class SunkenSea
    {
        /*
        checklist:

        -Add the wall corals properly
        -Make sunken sea walls produce water
        cut a half oval out of the top of forest in anticipation of structure placement
         */

        public static void ParallelFor(int fromInclusive, int toExclusive, int step, Action<int> action)
        {
            FasterParallel.For(fromInclusive, toExclusive, (start, end, _) =>
            {
                for (int i = start; i < end; i += step)
                    action.Invoke(i);
            });
        }

        public static void PlaceTimelessShores(int startPosX, int startPosY)
        {
            int biomeSize = 250 + (Main.maxTilesX / 180);

            //
            // Places a trapezoid of circles to serve as the main area for the biome.
            //
            const float steepness = 25f;
            static float trapezoidLateralSteep(float x)
            {
                if (x <= 1f / steepness)
                    return steepness * x;
                else if (x > 1f / steepness && x < 1f - 1f / steepness)
                    return 1f;
                else
                    return -steepness * x + steepness;
            }
            ParallelFor(startPosX - biomeSize - 5, startPosX + biomeSize + 20, 15, (X) =>
            {
                float height = MathHelper.Lerp(startPosY + 35, startPosY - 15, trapezoidLateralSteep(Utils.GetLerpValue(startPosX - biomeSize - 20, startPosX + biomeSize + 20, X)));
                for (int Y = startPosY + 25; Y >= height; Y -= 10)
                {
                    ShapeData circle = new ShapeData();
                    WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(30), Actions.Chain(new GenAction[]
                    {
                        // new Modifiers.RadialDither(1, 30).Output(circle),
                        new Modifiers.Blotches().Output(circle),
                    }));
                    WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                }
            });

            //
            // Makes the bottom borders of the Timeless Shores curved downwards.
            //
            const int totalCuveDepth = 90;
            ParallelFor(startPosX - biomeSize - 10, startPosX - biomeSize + 200, 10, (x) =>
            {
                int curveDepth = (int)MathHelper.Lerp(totalCuveDepth, 0f, MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize - 17, startPosX - biomeSize + 200, x, true)));
                for (int y = startPosY + 50; y <= startPosY + 70 + curveDepth; y += 5)
                {
                    ShapeData circle = new();
                    WorldUtils.Gen(new Point(x, y), new Shapes.Slime(30), Actions.Chain(new GenAction[]
                    {
                        // new Modifiers.RadialDither(1, 30).Output(circle),
                        new Modifiers.Blotches().Output(circle)
                    }));
                    WorldUtils.Gen(new Point(x, y), new ModShapes.All(circle), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                }
            });
            ParallelFor(startPosX + biomeSize - 200, startPosX + biomeSize + 10, 5, (x) =>
            {
                int curveDepth = (int)MathHelper.Lerp(totalCuveDepth, 0f, MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize + 20, startPosX + biomeSize - 200, x, true)));
                for (int y = startPosY + 50; y <= startPosY + 70 + curveDepth; y += 5)
                {
                    ShapeData circle = new();
                    WorldUtils.Gen(new Point(x, y), new Shapes.Slime(30), Actions.Chain(new GenAction[]
                    {
                        // new Modifiers.RadialDither(1, 30).Output(circle),
                        new Modifiers.Blotches().Output(circle)
                    }));
                    WorldUtils.Gen(new Point(x, y), new ModShapes.All(circle), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                }
            });

            // Clear a smaller square out of the center where the biomes stuff will be
            // Added curves to the top of the biome via curve depth -hpu 02/19/25
            ParallelFor(startPosX - biomeSize + 10, startPosX + biomeSize - 10, 1, (X) =>
            {
                int curveDepth = (int)MathHelper.Lerp(totalCuveDepth, 0f, MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize + 20, startPosX + biomeSize - 200, X, true)));
                int curveDepth2 = (int)MathHelper.Lerp(totalCuveDepth, 0f, MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize - 17, startPosX - biomeSize + 200, X, true)));

                int random = WorldGen.genRand.Next(-40, 10);
                for (int Y = startPosY - 25 + curveDepth + curveDepth2; Y <= startPosY + 50; Y++)
                {
                    ShapeData circle = new ShapeData();
                    GenAction blotchMod = new Modifiers.Blotches(2, 0.4);

                    WorldUtils.Gen(new Point(X, Y + random), new Shapes.Mound(WorldGen.genRand.Next(7, 10), 5), Actions.Chain(new GenAction[]
                    {
                        blotchMod.Output(circle)
                    }));
                    WorldUtils.Gen(new Point(X, Y + random), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                    {
                        new Actions.Clear()
                    }));
                }
            });

            //
            // Places slopes at the sides of the shores to actually make shores.
            //
            ParallelFor(startPosX - biomeSize - 15, startPosX - biomeSize + 200, 20, (moundX) =>
            {
                int RandomX = Main.rand.Next(15, 20);
                ShapeData mound = new();
                GenAction blotchMod = new Modifiers.Blotches().Output(mound);
                int moundY = startPosY + 75;
                int moundHeight = (int)MathHelper.Lerp(60f, 5f, MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize + 27, startPosX - biomeSize + 200, moundX)));

                WorldUtils.Gen(new Point(moundX, moundY), new Shapes.Mound(30, moundHeight), blotchMod);
                WorldUtils.Gen(new Point(moundX, moundY), new ModShapes.All(mound), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
                WorldUtils.Gen(new Point(moundX, moundY), new ModShapes.All(mound), new Actions.PlaceWall((ushort)ModContent.WallType<RunestoneWall>()));
            });
            ParallelFor(startPosX + biomeSize - 200, startPosX + biomeSize - 15, 20, (moundX) =>
            {
                ShapeData mound = new();
                GenAction blotchMod = new Modifiers.Blotches().Output(mound);
                int moundY = startPosY + 75;
                int moundHeight = (int)MathHelper.Lerp(60f, 5f, MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize - 27, startPosX + biomeSize - 200, moundX)));

                WorldUtils.Gen(new Point(moundX, moundY), new Shapes.Mound(30, moundHeight), blotchMod);
                WorldUtils.Gen(new Point(moundX, moundY), new ModShapes.All(mound), new Actions.SetTile((ushort)ModContent.TileType<Runestone>()));
            });


            //
            // Makes a large platform in the middle of the Timeless Shores.
            //
            const int platformSize = 130;
            const int platformHeight = 35; // Remember that because of Terraria's coordinate system, making it lower actually makes it higher.
            const int platformDepth = 35; // How deep it goes. Like above, the higher it is, the lower it goes.
            ParallelFor(startPosX - platformSize / 2, startPosX + platformSize / 2, 5, (platformX) =>
            {
                // This makes the "stem" of the platform be thinner towards the base and then expand on the top.
                int thickness = (int)(MathF.Pow(CalamityUtils.Convert01To010(Utils.GetLerpValue(startPosX - platformSize / 2, startPosX + platformSize / 2, platformX)), 3f) * platformDepth) + platformHeight;
                for (int platformY = startPosY + platformHeight; platformY <= startPosY + thickness; platformY++)
                {
                    WorldUtils.Gen(new Point(platformX, platformY), new Shapes.Circle(5), Actions.Chain(new GenAction[]
                    {
                        // Clear all tiles and place tiles.
                        new Actions.ClearTile(),
                        new Actions.PlaceTile((ushort)ModContent.TileType<Runestone>())
                    }));
                }
            });
            ParallelFor(startPosX - platformSize / 2, startPosX + platformSize / 2, 1, (x) =>
            {
                float interpolator = CalamityUtils.Convert01To010(Utils.GetLerpValue(startPosX - platformSize / 2, startPosX + platformSize / 2, x, true));
                int elevation = (int)MathHelper.Lerp(4f, 1f, interpolator);
                WorldUtils.Gen(new Point(x, startPosY + platformHeight - 7), new Shapes.Circle(elevation), Actions.Chain(new GenAction[] { new Actions.Clear(), }));
            });

            // Plateau
            ParallelFor(startPosX - biomeSize, startPosX + biomeSize, 100, (wallPillarX) =>
            {
                int randomDisplacement = WorldGen.genRand.Next(-27, 46);
                int RandomX = Main.rand.Next(15, 20);
                int RandomH = Main.rand.Next(20, 35);

                for (int wallPillarY = startPosY + RandomH; wallPillarY <= startPosY + 60; wallPillarY += 10)
                {
                    // Along the height of the pillar, it becomes shorter the more closer to the center we get.
                    int pillarWidth = (int)(CalamityUtils.Convert01To010(Utils.GetLerpValue(startPosY - RandomH, startPosY + 45, wallPillarY)) * 25);

                    ShapeData rectangle = new ShapeData();
                    GenAction blotchFilter = new Modifiers.Blotches(1, 0.4);
                    GenAction ditherFilter = new Modifiers.Dither(Math.Min(Utils.GetLerpValue(startPosY + 30, startPosY + 70, wallPillarY) + 0.25, 1));

                    WorldUtils.Gen(new Point(randomDisplacement + wallPillarX + pillarWidth / 2, wallPillarY), new Shapes.Circle(RandomX - pillarWidth + 3, RandomX), Actions.Chain(new GenAction[]
                    {
                        // Applies a blotch filter to the rectangle.
                        blotchFilter.Output(rectangle)
                    }));

                    WorldUtils.Gen(new Point(randomDisplacement + wallPillarX + pillarWidth / 4, wallPillarY), new ModShapes.All(rectangle), Actions.Chain(new GenAction[]
                    {
                        new Actions.SetTile((ushort)ModContent.TileType<Runestone>())
                    }))
                    ;
                }

            });

            //place water below the clear barrier for the islands so that theres water inbetween them
            ParallelFor(startPosX - biomeSize + 35, startPosX + biomeSize - 35, 1, (WaterX) =>
            {
                for (int WaterY = startPosY + 37; WaterY <= startPosY + 60; WaterY++)
                {
                    Main.tile[WaterX, WaterY].Get<LiquidData>().LiquidType = LiquidID.Water;
                    Main.tile[WaterX, WaterY].LiquidAmount = byte.MaxValue;
                }
            });

            //
            // Generates pillars made out of walls at random points in the Timeless Shores.
            //
            const int furthestDistanceFromCenter = 100;
            ParallelFor(startPosX - (biomeSize / 2) - furthestDistanceFromCenter, startPosX + (biomeSize / 2) + furthestDistanceFromCenter, 100, (wallPillarX) =>
            {
                int randomDisplacementX = WorldGen.genRand.Next(-10, 10);
                int RandomX = Main.rand.Next(3, 6);

                for (int wallPillarY = startPosY - 75; wallPillarY <= startPosY + 80; wallPillarY += 10)
                {
                    // Along the height of the pillar, it becomes shorter the more closer to the center we get.
                    int pillarWidth = (int)(CalamityUtils.Convert01To010(Utils.GetLerpValue(startPosY - 75, startPosY + 50, wallPillarY)) * 25);

                    ShapeData rectangle = new ShapeData();
                    GenAction blotchFilter = new Modifiers.Blotches(2, 0.4);
                    GenAction ditherFilter = new Modifiers.Dither(Math.Min(Utils.GetLerpValue(startPosY + 40, startPosY + 70, wallPillarY) + 0.25, 1));

                    WorldUtils.Gen(new Point(randomDisplacementX + wallPillarX + pillarWidth / 2, wallPillarY), new Shapes.Rectangle(25 - pillarWidth + 3, 15), Actions.Chain(new GenAction[]
                    {
                        ditherFilter.Output(rectangle),     // Applies a dithering filter to the rectangle.
                        blotchFilter.Output(rectangle),     // Applies a blotch filter to the rectangle.
                    }));

                    WorldUtils.Gen(new Point(randomDisplacementX + wallPillarX + pillarWidth / 2, wallPillarY), new ModShapes.All(rectangle), Actions.Chain(new GenAction[]
                    {
                        new Actions.PlaceWall((ushort)ModContent.WallType<RunestoneWall>()) // The shape places walls.
                    }))
                    ;
                }
            });

            //
            // Makes the transition area to the Timeless Shores.
            // Replaces tiles, walls, and places some water spots.
            //
            ParallelFor(startPosX - biomeSize - 30, startPosX + biomeSize + 30, 20, (x) =>
            {
                int curveDepth = (int)MathHelper.Lerp(totalCuveDepth, 0f, MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize + 30, startPosX + biomeSize - 200, x, true)));
                int curveDepth2 = (int)MathHelper.Lerp(totalCuveDepth, 0f, MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize - 30, startPosX - biomeSize + 200, x, true)));

                for (int y = startPosY - 5; y >= startPosY - 149; y -= 15)
                {
                    float interpolator = Utils.GetLerpValue(startPosY - 90, startPosY - 150, y, true);

                    // This lerp does absolutely nothing.
                    float ditherStrength = MathHelper.Lerp(0f, 0f, interpolator);

                    WorldUtils.Gen(new Point(x, y + curveDepth + curveDepth2), new Shapes.Circle(15), Actions.Chain(new GenAction[]
                    {
                            new Modifiers.OnlyTiles(TileID.Sand, TileID.HardenedSand),
                            new Modifiers.Dither(ditherStrength + 0f),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<Dunesand>()),
                    }));

                    // Changing these makes the walls dither higher up than tiles, to prevent the biome from changing earlier than expected. Still looks nice imo :)
                    interpolator = Utils.GetLerpValue(startPosY - 90, startPosY - 200, y, true);
                    ditherStrength = MathHelper.Lerp(0f, 0f, interpolator);

                    WorldUtils.Gen(new Point(x, y + curveDepth + curveDepth2), new Shapes.Circle(15), Actions.Chain(new GenAction[]
                    {
                            new Modifiers.OnlyWalls(WallID.Sandstone, WallID.HardenedSand),
                            new Modifiers.Dither(ditherStrength),
                            new Actions.ClearWall(),
                            new Actions.PlaceWall((ushort)ModContent.WallType<RunestoneWall>()),
                    }));

                    // I forgot to change this one. :peepotired:
                    interpolator = Utils.GetLerpValue(startPosY - 90, startPosY - 150, y, true);
                    ditherStrength = MathHelper.Lerp(0f, 0f, interpolator);
                    WorldUtils.Gen(new Point(x, y + curveDepth + curveDepth2), new Shapes.Circle(15), Actions.Chain(new GenAction[]
                    {
                            new Modifiers.OnlyTiles(TileID.Sandstone),
                            new Modifiers.Dither(ditherStrength),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<Runestone>()),
                    }));
                    interpolator = Utils.GetLerpValue(startPosY - 90, startPosY - 150, y, true);
                    ditherStrength = MathHelper.Lerp(0f, 0f, interpolator);
                    WorldUtils.Gen(new Point(x, y + curveDepth + curveDepth2), new Shapes.Circle(15), Actions.Chain(new GenAction[]
                    {
                            new Modifiers.OnlyTiles((ushort)ModContent.TileType<Basalt>()),
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

                // Place layer of sand on valid runestone blocks.
                for (int X = startPosX - biomeSize; X < startPosX + biomeSize; X += 1)
                {
                    for (int Y = startPosY - 100; Y <= startPosY + 100; Y++)
                    {
                        bool canPlaceSand =
                            Main.tile[X, Y].TileType == ModContent.TileType<Runestone>() &&
                            !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile &&
                            !Main.tile[X, Y - 3].HasTile && !Main.tile[X, Y - 4].HasTile &&
                            !Main.tile[X, Y - 5].HasTile;

                        if (canPlaceSand)
                            PlaceSand(X, Y, 5, ModContent.TileType<Dunesand>());

                        // Clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                            WorldGen.KillTile(X, Y);

                        // Kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                            WorldGen.KillTile(X, Y);

                    }
                }
                
                // Slope tiles and Soil Spawn
                for (int X = startPosX - biomeSize; X < startPosX + biomeSize; X += 1)
                {
                    for (int Y = startPosY - 300; Y <= startPosY + 200; Y++)
                    {
                        Tile.SmoothSlope(X, Y);

                        if (Main.tile[X, Y].TileType == ModContent.TileType<Dunesand>())
                        {
                            if (WorldGen.genRand.NextBool(25) && !Main.tile[X - 1, Y].HasTile)
                            {
                                ushort[] Soil = new ushort[] { (ushort)ModContent.TileType<AridSoil>() };

                                ShapeData circle = new ShapeData();
                                GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                                WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(3, 7)), Actions.Chain(new GenAction[]
                                {
                                    blotchMod.Output(circle)
                                }));

                                WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                                {
                                    new Modifiers.OnlyTiles((ushort)ModContent.TileType<Dunesand>()),
                                    new Actions.ClearTile(), new Actions.PlaceTile(WorldGen.genRand.Next(Soil))
                                }));

                            }
                        }
                    }
                }
            });
        }

        // Sides of the sunken sea (radiant reefs)
        public static void PlaceRadiantReefs(int startPosX, int startPosY)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.20f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int biomeSize = 280 + (Main.maxTilesX / 180);
            float actualSize = biomeSize * 16f;
            float constant = actualSize * 2f / (float)Math.Sin(angle);

            float fociSpacing = actualSize * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 20f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            // Generate the actual caverns
            bool runOnce = false; // This is used to make the wall on the left side go boom on the second run
            for (int X = origin.X - biomeSize - 170; X <= origin.X + biomeSize + 170; X++)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.99f;
                        if (percent > blurPercent)
                        {
                            // Place smaller shellstone clumps infront of the basalt so the basalt isnt actually inside of the biome itself
                            if (!runOnce)
                            {
                                WorldGen.TileRunner(X, Y, WorldGen.genRand.Next(8, 12), WorldGen.genRand.Next(8, 12), ModContent.TileType<Shellstone>(), true, 0f, 0f, true, true);
                                runOnce = true;
                            }

                            // Place smaller shellstone clumps infront of the basalt so the basalt isnt actually inside of the biome itself
                            if (runOnce)
                                WorldGen.TileRunner(X, Y, WorldGen.genRand.Next(8, 12), WorldGen.genRand.Next(8, 12), ModContent.TileType<Shellstone>(), false, 0f, 0f, true, true);
                        }
                        else
                        {
                            // Clear absolutely everything before generating the caverns
                            Main.tile[X, Y].ClearEverything();

                            // Generate perlin noise caves
                            float horizontalOffsetNoise = CalamityUtils.PerlinNoise2D(X / 90f, Y / 90f, 3, unchecked(cavePerlinSeed + 1)) * 0.01f;
                            float cavePerlinValue = CalamityUtils.PerlinNoise2D(X / 150f, Y / 90f, 3, cavePerlinSeed) + 0.5f + horizontalOffsetNoise;
                            float cavePerlinValue2 = CalamityUtils.PerlinNoise2D(X / 150f, Y / 90f, 3, unchecked(cavePerlinSeed - 1)) + 0.5f;
                            float caveNoiseMap = (cavePerlinValue + cavePerlinValue2) * 0.5f;
                            float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.380f;

                            // Kill or place tiles depending on the noise map
                            if (caveNoiseMap * caveNoiseMap > caveCreationThreshold)
                                WorldGen.KillTile(X, Y);
                            else
                                WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Shellstone>());

                            // Place walls in the biome using a different "seed" so it differs from the cave generation
                            // This creates a neat effect where walls worm their way through the caverns while leaving openings for the background to show through
                            float horizontalOffsetNoiseWalls = CalamityUtils.PerlinNoise2D(X / 80f, Y / 80f, 2, unchecked(cavePerlinSeedWalls + 1)) * 0.01f;
                            float cavePerlinValueWalls = CalamityUtils.PerlinNoise2D(X / 200f, Y / 160f, 4, cavePerlinSeedWalls) + 0.5f + horizontalOffsetNoiseWalls;
                            float cavePerlinValue2Walls = CalamityUtils.PerlinNoise2D(X / 200f, Y / 160f, 4, unchecked(cavePerlinSeedWalls - 1)) + 0.5f;
                            float caveNoiseMapWalls = (cavePerlinValueWalls + cavePerlinValue2Walls) * 0.5f;
                            float caveCreationThresholdWalls = horizontalOffsetNoiseWalls * 3.5f + 0.280f;

                            if (caveNoiseMapWalls * caveNoiseMapWalls > caveCreationThresholdWalls)
                                WorldGen.PlaceWall(X, Y, ModContent.WallType<ShellstoneWall>());
                            if (caveNoiseMapWalls * caveNoiseMapWalls < (caveCreationThresholdWalls - (caveNoiseMapWalls * 0.2f)))
                                WorldGen.KillTile(X, Y);

                            Main.tile[X, Y].Get<LiquidData>().LiquidType = LiquidID.Water;
                            Main.tile[X, Y].LiquidAmount = byte.MaxValue;
                        }
                    }
                }
            }

            // Cleanup
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        // Clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                            WorldGen.KillTile(X, Y);

                        // Kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                            WorldGen.KillTile(X, Y);
                    }
                }
            }

            // Place layer of sand blocks on valid surfaces
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        bool canPlaceSand =
                            Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>() &&
                            !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile &&
                            !Main.tile[X, Y - 3].HasTile && !Main.tile[X, Y - 4].HasTile &&
                            !Main.tile[X, Y - 5].HasTile;

                        // Place sand clumps on top of exposed shellstone
                        if (canPlaceSand)
                            PlaceSand(X, Y, 5, ModContent.TileType<EutrophicSand>());
                    }
                }
            }

            // Cleanup again
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 3; Y <= origin.Y + verticalRadius + 3; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, Y < origin.Y))
                    {
                        // Clean tiles that are sticking out (aka tiles only attached to one tile on one side)
                        bool OnlyRight = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile;
                        bool OnlyLeft = !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyDown = !Main.tile[X, Y - 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;
                        bool OnlyUp = !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile;

                        if (OnlyRight || OnlyLeft || OnlyDown || OnlyUp)
                            WorldGen.KillTile(X, Y);

                        // Kill random single floating tiles
                        if (!Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y + 1].HasTile && !Main.tile[X - 1, Y].HasTile && !Main.tile[X + 1, Y].HasTile)
                            WorldGen.KillTile(X, Y);

                        // If any sand is floating, put tiles below it
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

        // Middle of the sunken sea (polyp forest)
        public static void PlacePolypForest(int startPosX, int startPosY)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY - 200);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.15f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int biomeSize = 150 + (Main.maxTilesX / 180);
            float actualSize = biomeSize * 16f;
            float constant = actualSize * 2f / (float)Math.Sin(angle);

            float fociSpacing = actualSize * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 16f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            // Place the polyp forest caverns
            for (int X = origin.X - biomeSize - 3; X <= origin.X + biomeSize + 3; X++)
            {
                for (int Y = (int)(origin.Y + verticalRadius * 0.4f) + 3; Y >= origin.Y - verticalRadius - 3; Y--)
                {
                    // Modify this part so that the generation happens from bottom to top
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, false, Y < origin.Y))
                    {
                        float percent = dist / constant;
                        float blurPercent = 0.98f;

                        // Biome "blending" on the edges (disabled for now)
                        if (percent > blurPercent)
                        {

                            if (Main.tile[X, Y].HasTile && Main.tile[X, Y].TileType != ModContent.TileType<Limestone>())
                            {
                                ShapeData circle = new ShapeData();
                                GenAction blotchMod = new Modifiers.Blotches(4, 0.7);
                                WorldUtils.Gen(new Point(X, Y), new Shapes.Slime(WorldGen.genRand.Next(1, 3)), Actions.Chain(new GenAction[]
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
                            float cavePerlinValue = CalamityUtils.PerlinNoise2D(X / 800f, Y / 450f, 5, cavePerlinSeed) + 0.5f + horizontalOffsetNoise;
                            float cavePerlinValue2 = CalamityUtils.PerlinNoise2D(X / 800f, Y / 450f, 5, unchecked(cavePerlinSeed - 1)) + 0.5f;
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
                            float cavePerlinValueWalls = CalamityUtils.PerlinNoise2D(X / 600f, Y / 350f, 5, cavePerlinSeedWalls) + 0.5f + horizontalOffsetNoiseWalls;
                            float cavePerlinValue2Walls = CalamityUtils.PerlinNoise2D(X / 600f, Y / 350f, 5, unchecked(cavePerlinSeedWalls - 1)) + 0.5f;
                            float caveNoiseMapWalls = (cavePerlinValueWalls + cavePerlinValue2Walls) * 0.5f;
                            float caveCreationThresholdWalls = horizontalOffsetNoiseWalls * 3.5f + 0.3f;

                            if (caveNoiseMapWalls * caveNoiseMapWalls > caveCreationThresholdWalls)
                            {
                                //temporarily use navystone
                                WorldGen.PlaceWall(X, Y, ModContent.WallType<LimestoneWall>());
                            }

                            if ((caveNoiseMapWalls + 0.085f) * (caveNoiseMapWalls + 0.085f) > caveCreationThresholdWalls)
                            {
                                WorldGen.PlaceWall(X, Y, ModContent.WallType<ScarletSeaGrassWall>());
                            }
                            Main.tile[X, Y].Get<LiquidData>().LiquidType = LiquidID.Water;
                            Main.tile[X, Y].LiquidAmount = byte.MaxValue;
                        }
                    }
                }
            }

            //cleanup
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
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

            //place extra tiles
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
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
                            PlaceSand(X, Y, 4, ModContent.TileType<PolypSand>());
                        }

                    }
                }
            }
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
            {
                for (int Y = (int)(origin.Y - verticalRadius * 0.4f) - 20; Y <= origin.Y + verticalRadius + 20; Y++)
                {
                    if (CheckInBiomeArea(new Point(X, Y), topFoci, bottomFoci, constant, center, out float dist, false, Y > origin.Y))
                    {
                        bool canPlaceGrass = false;

                        //place grass on top of exposed sand
                        if (Main.tile[X, Y].TileType == ModContent.TileType<PolypSand>() && !Main.tile[X, Y - 1].HasTile && !Main.tile[X, Y - 2].HasTile &&
                        !Main.tile[X, Y - 3].HasTile && !Main.tile[X, Y - 4].HasTile && !Main.tile[X, Y - 5].HasTile)
                        {
                            canPlaceGrass = true;
                        }

                        if (canPlaceGrass)
                        {
                            PlaceSand(X, Y, 0, ModContent.TileType<ScarletSeaGrassTile>());
                        }
                    }
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Limestone>())
                    {
                        if (WorldGen.genRand.NextBool(25) && !Main.tile[X - 1, Y].HasTile)
                        {
                            ushort[] Soil = new ushort[] { (ushort)ModContent.TileType<LimestoneCobble>() };

                            ShapeData circle = new ShapeData();
                            GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(3, 7)), Actions.Chain(new GenAction[]
                            {
                                    blotchMod.Output(circle)
                            }));

                            WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                            {
                                    new Modifiers.OnlyTiles((ushort)ModContent.TileType<Limestone>()),
                                    new Actions.ClearTile(), new Actions.PlaceTile(WorldGen.genRand.Next(Soil))
                            }));

                        }
                    }
                }
            }


            //cleanup again
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
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
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 10)
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

                                WorldUtils.Gen(new Point(X, Y + 5), new Shapes.Rectangle(10, 10), Actions.Chain(new GenAction[]
                                {
                                        blotchMod.Output(circle)
                                }));
                                WorldUtils.Gen(new Point(X, Y + 5), new ModShapes.All(circle), Actions.Chain(new GenAction[]
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
                                WorldUtils.Gen(new Point(X, Y), new Shapes.Slime(WorldGen.genRand.Next(1, 3)), Actions.Chain(new GenAction[]
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
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
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
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
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

        //Very bottom of the biome (Clam Den)
        public static void PlaceClamDen(int startPosX, int startPosY)
        {
            int cavePerlinSeed = WorldGen.genRand.Next();
            int cavePerlinSeedWalls = WorldGen.genRand.Next();

            Point origin = new Point(startPosX, startPosY);
            Vector2 center = origin.ToVector2() * 16f + new Vector2(8f);

            float angle = MathHelper.Pi * 0.70f;
            float otherAngle = MathHelper.PiOver2 - angle;

            int biomeSize = 80 + (Main.maxTilesX / 180);
            float actualSize = biomeSize * 16f;
            float constant = actualSize * 2f / (float)Math.Sin(angle);

            float fociSpacing = actualSize * (float)Math.Sin(otherAngle) / (float)Math.Sin(angle);
            int verticalRadius = (int)(constant / 10f);

            Vector2 fociOffset = Vector2.UnitY * fociSpacing;
            Vector2 topFoci = center - fociOffset;
            Vector2 bottomFoci = center + fociOffset;

            //place the Clam Den caverns
            for (int X = origin.X - biomeSize - 190; X <= origin.X + biomeSize + 190; X++)
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
                                WorldUtils.Gen(new Point(X, Y), new Shapes.Slime(WorldGen.genRand.Next(1, 3)), Actions.Chain(new GenAction[]
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
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
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
            for (int X = origin.X - biomeSize - 3; X < origin.X + biomeSize + 3; X += 1)
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
                            PlaceSand(X, Y, 4, ModContent.TileType<WhitePearlPile>());
                        }
                    }
                }
            }

            //cleanup again
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

                        //if any sand is floating, put tiles below it
                        if (Main.tile[X, Y].TileType == ModContent.TileType<WhitePearlPile>() && !Main.tile[X, Y + 1].HasTile)
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
            int biomeSize = 250 + (Main.maxTilesX / 180);

            int XLeft = GenVars.UndergroundDesertLocation.Left + 40;
            int XRight = GenVars.UndergroundDesertLocation.Right - 70;
            //place blocks of basalt all the way down to hell, here it just places one block and wall so it doesnt hurt preformance
            for (int X = startPosX - biomeSize - 60; X < startPosX + biomeSize + 60; X += 1)
            {
                //const int totalCuveDepth = -120;
                //const int totalCuveDepth2 = 120;
                int basaltbordernoise = Main.rand.Next(180, 185);
                int basaltbordernoisetop = Main.rand.Next(645, 650);
                const int totalCuveDepth = -220;
                const int totalCuveDepth2 = 90;
                int curveDepth = (int)MathHelper.Lerp(
                            totalCuveDepth,
                            0f,
                            MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize + 60, startPosX + biomeSize - 200, X, true)));
                int curveDepth2 = (int)MathHelper.Lerp(
                        totalCuveDepth,
                        0f,
                        MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize - 60, startPosX - biomeSize + 200, X, true)));
                int curveDepthtop = (int)MathHelper.Lerp(
                            totalCuveDepth2,
                            0f,
                            MathF.Sqrt(Utils.GetLerpValue(startPosX + biomeSize + 60, startPosX + biomeSize - 200, X, true)));
                int curveDepth2top = (int)MathHelper.Lerp(
                        totalCuveDepth2,
                        0f,
                        MathF.Sqrt(Utils.GetLerpValue(startPosX - biomeSize - 60, startPosX - biomeSize + 200, X, true)));

                for (int Y = startPosY - 650 + curveDepthtop + curveDepth2top; Y <= Main.maxTilesY - 185 + curveDepth + curveDepth2; Y++)
                {
                    Main.tile[X, Y].ClearEverything();
                    WorldGen.PlaceTile(X, Y, (ushort)ModContent.TileType<Basalt>());
                    WorldGen.PlaceWall(X, Y, ModContent.WallType<LargeBasaltWall>());
                }
            }

            //place caverns and lava
            for (int X = startPosX - biomeSize - (Main.maxTilesX / 25); X <= startPosX + biomeSize + (Main.maxTilesX / 25); X++)
            {
                for (int Y = startPosY; Y <= Main.maxTilesY - 190; Y++)
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
            for (int X = startPosX - biomeSize - (Main.maxTilesX / 25); X < startPosX + biomeSize + (Main.maxTilesX / 25); X += 1)
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
            for(int X = startPosX - biomeSize - (Main.maxTilesX / 25); X < startPosX + biomeSize + (Main.maxTilesX / 25); X += 1)
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
                    Tile.SmoothSlope(X, Y);
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
                    if (WorldGen.genRand.NextBool(800) && ((Main.tile[X, Y].TileType == ModContent.TileType<EutrophicSand>() && !Main.tile[X, Y - 1].HasTile) ||
                    (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>() && !Main.tile[X, Y + 1].HasTile)))
                    {
                        ushort[] Corals = new ushort[] { (ushort)ModContent.TileType<CyanCoral>(), (ushort)ModContent.TileType<LimeCoral>(),
                        (ushort)ModContent.TileType<MagentaCoral>(), (ushort)ModContent.TileType<OrangeCoral>(), (ushort)ModContent.TileType<YellowCoral>() };

                        ShapeData circle = new ShapeData();
                        GenAction blotchMod = new Modifiers.Blotches(2, 0.4);
                        WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(WorldGen.genRand.Next(1, 3)), Actions.Chain(new GenAction[]
                        {
                            blotchMod.Output(circle)
                        }));

                        WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle), Actions.Chain(new GenAction[]
                        {
                            new Actions.ClearTile(), new Actions.PlaceTile(WorldGen.genRand.Next(Corals))
                        }));
                    }
                    //Timeless Shores ambiant tiles
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Dunesand>())
                    {
                        //Driftwood Ambiance
                        if (WorldGen.genRand.NextBool(70))
                        {
                            ushort[] DriftwoodPiles = new ushort[] { (ushort)ModContent.TileType<DriftwoodAmbient1>(), (ushort)ModContent.TileType<DriftwoodAmbient2>(), (ushort)ModContent.TileType<DriftwoodAmbient3>(), (ushort)ModContent.TileType<DriftwoodAmbient4>(), (ushort)ModContent.TileType<DriftwoodAmbient5>(), (ushort)ModContent.TileType<DriftwoodAmbient6>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(DriftwoodPiles));
                        }
                    }
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Runestone>())
                    {
                        //Driftwood Ambiance
                        if (WorldGen.genRand.NextBool(100))
                        {
                            ushort[] DriftwoodPiles = new ushort[] { (ushort)ModContent.TileType<DriftwoodAmbient1>(), (ushort)ModContent.TileType<DriftwoodAmbient2>(), (ushort)ModContent.TileType<DriftwoodAmbient3>(), (ushort)ModContent.TileType<DriftwoodAmbient4>(), (ushort)ModContent.TileType<DriftwoodAmbient5>(), (ushort)ModContent.TileType<DriftwoodAmbient6>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(DriftwoodPiles));
                        }
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
                    if (Main.tile[X, Y].TileType == ModContent.TileType<SeaPrism>())
                    {
                        int RandStyle() => WorldGen.genRand.Next(8);
                        int style = RandStyle();
                        //Medium Sea Prism down
                        if (WorldGen.genRand.NextBool(9))
                        {
                            ushort[] CrystalRandom = new ushort[] { (ushort)ModContent.TileType<MediumSeaPrismCrystal>() };
                            WorldGen.PlaceObject(X, Y + 1, WorldGen.genRand.Next(CrystalRandom), true, 0, 0, style);
                        }
                        if (WorldGen.genRand.NextBool(9))
                        {
                            ushort[] CrystalRandom = new ushort[] { (ushort)ModContent.TileType<MediumSeaPrismCrystal>() };
                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(CrystalRandom), true, 0, 0, style);
                        }
                        if (WorldGen.genRand.NextBool(9))
                        {
                            ushort[] CrystalRandom = new ushort[] { (ushort)ModContent.TileType<MediumSeaPrismCrystal>() };
                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(CrystalRandom), true, 0, 0, style);
                        }
                        if (WorldGen.genRand.NextBool(9))
                        {
                            ushort[] CrystalRandom = new ushort[] { (ushort)ModContent.TileType<MediumSeaPrismCrystal>() };
                            WorldGen.PlaceObject(X, Y + 1, WorldGen.genRand.Next(CrystalRandom), true, 0, 0, style);
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
                            (ushort)ModContent.TileType<NavystonePile2>(), (ushort)ModContent.TileType<NavystonePile3>(), (ushort)ModContent.TileType<NavystoneAmbient>(),
                            (ushort)ModContent.TileType<NavystoneAmbient2>(), (ushort)ModContent.TileType<NavystoneAmbient3>()};

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(Piles));
                        }
                    }
                    //Polyp Forest Ambient tiles
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Limestone>())
                    {
                        int RandStyle() => WorldGen.genRand.Next(8);
                        int style = RandStyle();
                        //BranchCoralsOnLimestone
                        if (WorldGen.genRand.NextBool(15))
                        {
                            ushort[] BranchCorals = new ushort[] { (ushort)ModContent.TileType<BranchCoral>() };
                            WorldGen.PlaceObject(X, Y + 1, WorldGen.genRand.Next(BranchCorals), true, 0, 0, style);
                        }
                        if (WorldGen.genRand.NextBool(15))
                        {
                            ushort[] BranchCorals = new ushort[] { (ushort)ModContent.TileType<BranchCoral>() };
                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(BranchCorals), true, 0, 0, style);
                        }
                    }
                    if (Main.tile[X, Y].TileType == ModContent.TileType<ScarletSeaGrassTile>())
                    {
                        int RandStyle() => WorldGen.genRand.Next(8);
                        int style = RandStyle();
                        if (WorldGen.genRand.NextBool(50) && !Main.tile[X - 1, Y].HasTile)
                            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(3), Actions.Chain(new GenAction[]
                        {
                            new Modifiers.OnlyTiles((ushort)ModContent.TileType<ScarletSeaGrassTile>(),(ushort)ModContent.TileType<PolypSand>()),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<PinkPearlPile>()),
                        }));
                        //BranchCoralsOnSand
                        if (WorldGen.genRand.NextBool(25))
                        {
                            ushort[] BranchCorals = new ushort[] { (ushort)ModContent.TileType<BranchCoral>() };
                            WorldGen.PlaceObject(X, Y + 1, WorldGen.genRand.Next(BranchCorals), true, 0, 0, style);
                        }
                        if (WorldGen.genRand.NextBool(25))
                        {
                            ushort[] BranchCorals = new ushort[] { (ushort)ModContent.TileType<BranchCoral>() };
                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(BranchCorals), true, 0, 0, style);
                        }
                        if (WorldGen.genRand.NextBool(25))
                        {
                            ushort[] DigitateCorals = new ushort[] { (ushort)ModContent.TileType<DigitateCoral>(),
                            (ushort)ModContent.TileType<DigitateCoral3>(), (ushort)ModContent.TileType<DigitateCoral2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(DigitateCorals));
                        }
                        if (WorldGen.genRand.NextBool(8))
                        {
                            ushort[] FryCorals = new ushort[] { (ushort)ModContent.TileType<FryCoral>(),
                            (ushort)ModContent.TileType<FryCoral3>(), (ushort)ModContent.TileType<FryCoral2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(FryCorals));
                        }
                        if (WorldGen.genRand.NextBool(8))
                        {
                            ushort[] WideScarletSeagrasss = new ushort[] { (ushort)ModContent.TileType<WideScarletSeagrass>(),
                            (ushort)ModContent.TileType<WideScarletSeagrass3>(), (ushort)ModContent.TileType<WideScarletSeagrass2>(), (ushort)ModContent.TileType<WideScarletSeagrass4>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(WideScarletSeagrasss));
                        }
                        if (WorldGen.genRand.NextBool(8))
                        {
                            ushort[] StalkCorals = new ushort[] { (ushort)ModContent.TileType<StalkCoral>(),
                            (ushort)ModContent.TileType<StalkCoral3>(), (ushort)ModContent.TileType<StalkCoral2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(StalkCorals));
                        }
                        if (WorldGen.genRand.NextBool(10))
                        {
                            ushort[] FryCorals = new ushort[] { (ushort)ModContent.TileType<FryCoral>(),
                            (ushort)ModContent.TileType<FryCoral3>(), (ushort)ModContent.TileType<FryCoral2>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(FryCorals));
                        }
                        if (WorldGen.genRand.NextBool(10))
                        {
                            ushort[] TallDigitateCorals = new ushort[] { (ushort)ModContent.TileType<TallDigitateCoral>(),
                            (ushort)ModContent.TileType<TallDigitateCoral3>(), (ushort)ModContent.TileType<TallDigitateCoral2>(), (ushort)ModContent.TileType<TallDigitateCoral4>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(TallDigitateCorals));
                        }

                        //Driftwood Ambiance
                        if (WorldGen.genRand.NextBool(15))
                        {
                            ushort[] DriftwoodPiles = new ushort[] { (ushort)ModContent.TileType<DriftwoodAmbient1>(), (ushort)ModContent.TileType<DriftwoodAmbient2>(), (ushort)ModContent.TileType<DriftwoodAmbient3>(), (ushort)ModContent.TileType<DriftwoodAmbient4>(), (ushort)ModContent.TileType<DriftwoodAmbient5>(), (ushort)ModContent.TileType<DriftwoodAmbient6>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(DriftwoodPiles));
                        }
                    }

                    //radiant reefs ambient tiles
                    if (Main.tile[X, Y].TileType == ModContent.TileType<EutrophicSand>())
                    {
                        if (WorldGen.genRand.NextBool(100) && !Main.tile[X - 1, Y].HasTile)
                            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(7), Actions.Chain(new GenAction[]
                        {
                            new Modifiers.OnlyTiles((ushort)ModContent.TileType<EutrophicSand>()),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<WhitePearlPile>()),
                        }));
                        //Driftwood Ambiance
                        if (WorldGen.genRand.NextBool(15))
                        {
                            ushort[] DriftwoodPiles = new ushort[] { (ushort)ModContent.TileType<DriftwoodAmbient1>(), (ushort)ModContent.TileType<DriftwoodAmbient2>(), (ushort)ModContent.TileType<DriftwoodAmbient3>(), (ushort)ModContent.TileType<DriftwoodAmbient4>(), (ushort)ModContent.TileType<DriftwoodAmbient5>(), (ushort)ModContent.TileType<DriftwoodAmbient6>() };

                            WorldGen.PlaceObject(X, Y - 1, WorldGen.genRand.Next(DriftwoodPiles));
                        }
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
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Shellstone>() && Main.tile[X, Y].Slope == 0 && !Main.tile[X, Y + 1].HasTile && !Main.tile[X, Y + 2].HasTile)
                    {
                        if (WorldGen.genRand.NextBool())
                        {
                            WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<RefractiveHangingCoral>());
                        }
                    }
                    if (Main.tile[X, Y].TileType == ModContent.TileType<RefractiveHangingCoral>())
                    {
                        CalamityUtils.GrowVines(X, Y, WorldGen.genRand.Next(1, 4), (ushort)ModContent.TileType<RefractiveHangingCoral>());
                    }

                    //grow depth vines on navystone
                    if (Main.tile[X, Y].TileType == ModContent.TileType<Navystone>() && Main.tile[X, Y].Slope == 0 && !Main.tile[X, Y + 1].HasTile && !Main.tile[X, Y + 2].HasTile)
                    {
                        // 18APR2025: Ozzatron: removed guaranteed RNG check here, since it was intended to be guaranteed
                        WorldGen.PlaceTile(X, Y + 1, (ushort)ModContent.TileType<DepthVines>());
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
                    // Pearl Autism
                    if (Main.tile[X, Y].TileType == ModContent.TileType<VolcanicSand>())
                    {
                        if (WorldGen.genRand.NextBool(50) && !Main.tile[X - 1, Y].HasTile)
                            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(3), Actions.Chain(new GenAction[]
                        {
                            new Modifiers.OnlyTiles((ushort)ModContent.TileType<VolcanicSand>()),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<BlackPearlPile>()),
                        }));
                    }
                    if (Main.tile[X, Y].TileType == ModContent.TileType<HardenedEutrophicSand>())
                    {
                        if (WorldGen.genRand.NextBool(30) && !Main.tile[X - 1, Y].HasTile)
                            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(3), Actions.Chain(new GenAction[]
                        {
                            new Modifiers.OnlyTiles((ushort)ModContent.TileType<HardenedEutrophicSand>()),
                            new Actions.ClearTile(),
                            new Actions.PlaceTile((ushort)ModContent.TileType<WhitePearlPile>()),
                        }));
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
            GenAction blotchMod2 = new Modifiers.Blotches(1, 0.1);

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
                blotchMod2.Output(circle3)
            }));
            WorldUtils.Gen(new Point(X, Y), new ModShapes.All(circle3), Actions.Chain(new GenAction[]
            {
                new Actions.ClearTile(),
                new Actions.SetLiquid(),
            }));

            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(radius - 4), Actions.Chain(new GenAction[]
            {
                new Actions.PlaceWall((ushort)ModContent.WallType<SeaPrismWall>())
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
                (ushort)ModContent.TileType<ScarletSeaGrassTile>(),
                (ushort)ModContent.TileType<CyanCoral>(),
                (ushort)ModContent.TileType<MagentaCoral>(),
                (ushort)ModContent.TileType<OrangeCoral>(),
                (ushort)ModContent.TileType<YellowCoral>(),
                (ushort)ModContent.TileType<LimeCoral>(),
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

            for (int x = 20; x < Main.maxTilesX - 20; x += 1)
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
