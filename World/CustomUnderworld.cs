using System;
using System.Collections.Generic;
using CalamityMod.Tiles.Underworld;
using CalamityMod.Schematics;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static CalamityMod.Schematics.SchematicManager;

namespace CalamityMod.World
{
    public class CustomUnderworld
    {
        // Ash island variables
        private const double AshIslandGenStart = 0.36;
        private const double AshIslandGenStart_Remix = 0.38;
        private const double AshIslandGenEnd = 0.64;
        private const double AshIslandGenEnd_Remix = 0.62;
        private const int AshIslandDepthMaxAboveTheUnderworldFloor = 135;
        private const int AshIslandHeightMaxAboveTheUnderworldFloor = 160;
        private const int MaxIslands = 8;
        private const int SmallWorldIslands = 4;
        private const int MediumWorldIslands = 6;
        private const int LargeWorldIslands = MaxIslands;
        private const int SmallWorldOuterIslands = 1;
        private const int MediumAndLargeWorldOuterIslands = 2;

        // Pillar variables
        private const int MaxPillarHeight = 68;
        private const int RandomizedPillarSectionHeight = 4;
        private const int MaxPillarDepthAboveTheUnderworldFloor = 40;

        public static void NewUnderworld()
        {
            // Generate lower Underworld ash
            int ashMin = 160;
            int ashMax = 190;
            int ashDepth = Main.maxTilesY - WorldGen.genRand.Next(ashMin, ashMax);
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                ashDepth += WorldGen.genRand.Next(-3, 4);
                if (ashDepth < Main.maxTilesY - ashMax)
                    ashDepth = Main.maxTilesY - ashMax;

                if (ashDepth > Main.maxTilesY - ashMin)
                    ashDepth = Main.maxTilesY - ashMin;

                for (int y = ashDepth - 20 - WorldGen.genRand.Next(3); y < Main.maxTilesY; y++)
                {
                    if (y >= ashDepth)
                    {
                        Main.tile[x, y].Get<TileWallWireStateData>().HasTile = false;
                        Main.tile[x, y].Get<LiquidData>().LiquidType = LiquidID.Water;
                    }
                    else
                        Main.tile[x, y].TileType = TileID.Ash;
                }
            }

            // Generate lava
            int lavaMin = 80;
            int lavaMax = 90;
            int lavaDepth = Main.maxTilesY - WorldGen.genRand.Next(lavaMin, lavaMax + 1);
            int lavaMaxDepth = Main.maxTilesY - lavaMin;
            int lavaMaxHeight = Main.maxTilesY - lavaMax;
            for (int x = 10; x < Main.maxTilesX - 10; x++)
            {
                lavaDepth += WorldGen.genRand.Next(-5, 6);
                if (lavaDepth > lavaMaxDepth)
                    lavaDepth = lavaMaxDepth;

                if (lavaDepth < lavaMaxHeight)
                    lavaDepth = lavaMaxHeight;

                for (int y = lavaDepth; y < Main.maxTilesY - 10; y++)
                {
                    if (!Main.tile[x, y].HasTile)
                    {
                        Main.tile[x, y].Get<LiquidData>().LiquidType = LiquidID.Lava;
                        Main.tile[x, y].LiquidAmount = byte.MaxValue;
                    }
                }
            }

            // Ash splotches
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (WorldGen.genRand.NextBool(50))
                {
                    int y = Main.maxTilesY - 65;
                    while (!Main.tile[x, y].HasTile && y > Main.maxTilesY - 135)
                        y--;

                    WorldGen.TileRunner(WorldGen.genRand.Next(Main.maxTilesX), y + WorldGen.genRand.Next(20, 50), WorldGen.genRand.Next(15, 20), 1000, TileID.Ash, addTile: true, 0D, WorldGen.genRand.Next(1, 3), noYChange: true);
                }
            }

            // Some cursed water function
            Liquid.QuickWater(-2);

            // More Ash splotches
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (WorldGen.genRand.NextBool(13))
                {
                    int y = Main.maxTilesY - 65;
                    while ((Main.tile[x, y].LiquidAmount > 0 || Main.tile[x, y].HasTile) && y > Main.maxTilesY - 140)
                        y--;

                    if (WorldGen.genRand.NextBool(3) || !(x > Main.maxTilesX * 0.4) || !(x < Main.maxTilesX * 0.6))
                        WorldGen.TileRunner(x, y - WorldGen.genRand.Next(2, 5), WorldGen.genRand.Next(5, 30), 1000, TileID.Ash, addTile: true, 0D, WorldGen.genRand.Next(1, 3), noYChange: true);

                    double ashSplotchSizeAdjustment = WorldGen.genRand.Next(1, 3);
                    if (WorldGen.genRand.NextBool(3))
                        ashSplotchSizeAdjustment *= 0.5;

                    if (WorldGen.genRand.NextBool(3) || !(x > Main.maxTilesX * 0.4) || !(x < Main.maxTilesX * 0.6))
                    {
                        if (WorldGen.genRand.NextBool(2))
                            WorldGen.TileRunner(x, y - WorldGen.genRand.Next(2, 5), (int)(WorldGen.genRand.Next(5, 15) * ashSplotchSizeAdjustment), (int)(WorldGen.genRand.Next(10, 15) * ashSplotchSizeAdjustment), TileID.Ash, addTile: true, 1D, 0.3);

                        if (WorldGen.genRand.NextBool(2))
                        {
                            ashSplotchSizeAdjustment = WorldGen.genRand.Next(1, 3);
                            WorldGen.TileRunner(x, y - WorldGen.genRand.Next(2, 5), (int)(WorldGen.genRand.Next(5, 15) * ashSplotchSizeAdjustment), (int)(WorldGen.genRand.Next(10, 15) * ashSplotchSizeAdjustment), TileID.Ash, addTile: true, -1D, 0.3);
                        }
                    }

                    WorldGen.TileRunner(x + WorldGen.genRand.Next(-10, 10), y + WorldGen.genRand.Next(-10, 10), WorldGen.genRand.Next(5, 15), WorldGen.genRand.Next(5, 10), -2, addTile: false, WorldGen.genRand.Next(-1, 3), WorldGen.genRand.Next(-1, 3));
                    if (WorldGen.genRand.NextBool(3))
                        WorldGen.TileRunner(x + WorldGen.genRand.Next(-10, 10), y + WorldGen.genRand.Next(-10, 10), WorldGen.genRand.Next(10, 30), WorldGen.genRand.Next(10, 20), -2, addTile: false, WorldGen.genRand.Next(-1, 3), WorldGen.genRand.Next(-1, 3));

                    if (WorldGen.genRand.NextBool(5))
                        WorldGen.TileRunner(x + WorldGen.genRand.Next(-15, 15), y + WorldGen.genRand.Next(-15, 10), WorldGen.genRand.Next(15, 30), WorldGen.genRand.Next(5, 20), -2, addTile: false, WorldGen.genRand.Next(-1, 3), WorldGen.genRand.Next(-1, 3));
                }
            }

            // Cursed lava placement tile runner, thanks redigit
            for (int i = 0; i < Main.maxTilesX; i++)
                WorldGen.TileRunner(WorldGen.genRand.Next(20, Main.maxTilesX - 20), WorldGen.genRand.Next(Main.maxTilesY - 180, Main.maxTilesY - 10), WorldGen.genRand.Next(2, 7), WorldGen.genRand.Next(2, 7), -2);

            // Another cursed lava placement tile runner...seriously, why the fuck is this?
            for (int i = 0; i < Main.maxTilesX * 2; i++)
                WorldGen.TileRunner(WorldGen.genRand.Next((int)(Main.maxTilesX * 0.35), (int)(Main.maxTilesX * 0.65)), WorldGen.genRand.Next(Main.maxTilesY - 180, Main.maxTilesY - 10), WorldGen.genRand.Next(5, 20), WorldGen.genRand.Next(5, 10), -2);

            // Place two lines of lava at maxTilesY - 145 and maxTilesY - 144
            if (Main.zenithWorld)
            {
                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    if (!Main.tile[x, Main.maxTilesY - 145].HasTile)
                    {
                        Main.tile[x, Main.maxTilesY - 145].LiquidAmount = byte.MaxValue;
                        Main.tile[x, Main.maxTilesY - 145].Get<LiquidData>().LiquidType = LiquidID.Lava;
                    }

                    if (!Main.tile[x, Main.maxTilesY - 144].HasTile)
                    {
                        Main.tile[x, Main.maxTilesY - 144].LiquidAmount = byte.MaxValue;
                        Main.tile[x, Main.maxTilesY - 144].Get<LiquidData>().LiquidType = LiquidID.Lava;
                    }
                }
            }

            // Place hellstone splotches
            for (int i = 0; i < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 0.0008); i++)
                WorldGen.TileRunner(WorldGen.genRand.Next(Main.maxTilesX), WorldGen.genRand.Next(Main.maxTilesY - 140, Main.maxTilesY), WorldGen.genRand.Next(2, 7), WorldGen.genRand.Next(3, 7), TileID.Hellstone);

            // Remix world stuff, the Ash islands in the middle

            // Start generating islands at this point
            int ashIslandX = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? AshIslandGenStart_Remix : AshIslandGenStart));

            // Stop generating islands at this point
            int ashIslandX2 = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? AshIslandGenEnd_Remix : AshIslandGenEnd));

            // Ash island gen limits
            int ashIslandDepthLimit = Main.maxTilesY - AshIslandDepthMaxAboveTheUnderworldFloor;
            int ashIslandHeightLimit = Main.maxTilesY - AshIslandHeightMaxAboveTheUnderworldFloor;

            // Multiple islands in non-remix
            if (!WorldGen.remixWorldGen)
            {
                // Large = 8, Medium = 6, Small = 4
                int numIslands = (int)(Main.maxTilesX / 4200f * 4f);
                bool smallWorld = numIslands == SmallWorldIslands;

                // Total extra distance between islands for lava lakes
                int totalExtraDistanceBetweenAshIslands = (int)((double)Main.maxTilesX * 0.04);

                // Extra distance per island
                // Due to this being done on both sides of each island, it is divided by 2
                int extraDistanceBetweenAshIslands_PerIslandSide = totalExtraDistanceBetweenAshIslands / numIslands / 2;

                // Calculate distance between islands
                int distanceBetweenIslands = (ashIslandX2 - ashIslandX) / numIslands;

                // Used for island height randomization
                int minHeightForOuterIslands = 8;
                int[] randomHeightAdjustmentLimits = new int[MaxIslands]
                {
                    28,
                    24,
                    20,
                    16,
                    12,
                    8,
                    4,
                    0
                };

                // Used for island edge drop off randomization
                // Taller islands have steeper drop offs
                // This is also used to decrease the width of the islands
                int minDropOffForOuterIslands = 11;
                int[] randomDropOffAdjustmentLimits = new int[MaxIslands]
                {
                    3,
                    4,
                    5,
                    7,
                    9,
                    11,
                    15,
                    19
                };

                // Loop to gen the islands
                int chosenIslandSize;
                int previouslyChosenIslandSize = -1;
                for (int i = 0; i < numIslands; i++)
                {
                    // Do not repeat the same island size twice in a row
                    do chosenIslandSize = WorldGen.genRand.Next(MaxIslands);
                    while (previouslyChosenIslandSize == chosenIslandSize);
                    previouslyChosenIslandSize = chosenIslandSize;

                    int randomizedIslandDropOffAdjustment = randomDropOffAdjustmentLimits[chosenIslandSize];
                    int randomizedIslandHeightAdjustment = randomHeightAdjustmentLimits[chosenIslandSize];

                    // Islands on the edges are taller in order to have more structures
                    bool isTallerIsland = smallWorld ? (i == 0 || i == numIslands - SmallWorldOuterIslands) : (i < MediumAndLargeWorldOuterIslands || i >= numIslands - MediumAndLargeWorldOuterIslands);
                    if (isTallerIsland && randomizedIslandHeightAdjustment > minHeightForOuterIslands)
                    {
                        randomizedIslandHeightAdjustment = minHeightForOuterIslands;
                        randomizedIslandDropOffAdjustment = minDropOffForOuterIslands;
                    }

                    int randomizedAshIslandDepthLimit = ashIslandDepthLimit + randomizedIslandHeightAdjustment;
                    int randomizedAshIslandHeightLimit = ashIslandHeightLimit + randomizedIslandHeightAdjustment;

                    int ashIslandXAdjustment = distanceBetweenIslands * i;
                    int ashIslandX2Adjustment = distanceBetweenIslands * (numIslands - i - 1);

                    // Decrease the width of each island randomly
                    // Taller islands have a greater reduction in width
                    int randomWidthReduction_LeftSide = WorldGen.genRand.Next(11) + (randomizedIslandDropOffAdjustment - 4);
                    int randomWidthReduction_RightSide = WorldGen.genRand.Next(11) + (randomizedIslandDropOffAdjustment - 4);
                    int ashIslandTilePlacementX = ashIslandX + ashIslandXAdjustment + extraDistanceBetweenAshIslands_PerIslandSide + randomWidthReduction_LeftSide;
                    int ashIslandTilePlacementX2 = ashIslandX2 - ashIslandX2Adjustment - extraDistanceBetweenAshIslands_PerIslandSide - randomWidthReduction_RightSide;
                    int ashIslandGenLimiter = Main.maxTilesY - 1;
                    bool ashIslandGenLimitHit = false;
                    Liquid.QuickWater(-2);
                    for (; ashIslandGenLimiter < Main.maxTilesY - 1 || ashIslandTilePlacementX < ashIslandTilePlacementX2; ashIslandTilePlacementX++)
                    {
                        // Less random ash island terrain to make unmodified traversal less annoying
                        if (!ashIslandGenLimitHit)
                        {
                            // Steeper ash island edges
                            ashIslandGenLimiter -= WorldGen.genRand.Next(1, randomizedIslandDropOffAdjustment);
                            if (ashIslandGenLimiter < randomizedAshIslandDepthLimit)
                                ashIslandGenLimitHit = true;
                        }
                        else if (ashIslandTilePlacementX >= ashIslandTilePlacementX2)
                        {
                            // Steeper ash island edges
                            ashIslandGenLimiter += WorldGen.genRand.Next(1, randomizedIslandDropOffAdjustment);
                            if (ashIslandGenLimiter > Main.maxTilesY - 1)
                                ashIslandGenLimiter = Main.maxTilesY - 1;
                        }
                        else
                        {
                            if ((ashIslandTilePlacementX <= Main.maxTilesX / 2 - 5 || ashIslandTilePlacementX >= Main.maxTilesX / 2 + 5) && WorldGen.genRand.NextBool(4))
                            {
                                // More randomized terrain depending on island type
                                // Lower islands have smoother terrain
                                // Islands on the edges have smoother terrain
                                if (isTallerIsland)
                                {
                                    if (WorldGen.genRand.NextBool(4))
                                        ashIslandGenLimiter += WorldGen.genRand.Next(-1, 2);
                                    else if (WorldGen.genRand.NextBool(8))
                                        ashIslandGenLimiter += WorldGen.genRand.Next(-2, 3);
                                }
                                else
                                {
                                    switch (chosenIslandSize)
                                    {
                                        default:
                                        case 0:
                                        case 1:
                                            if (WorldGen.genRand.NextBool(4))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-1, 2);
                                            else if (WorldGen.genRand.NextBool(8))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-2, 3);
                                            break;

                                        case 2:
                                        case 3:
                                            if (WorldGen.genRand.NextBool(3))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-1, 2);
                                            else if (WorldGen.genRand.NextBool(6))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-2, 3);
                                            else if (WorldGen.genRand.NextBool(9))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-3, 4);
                                            break;

                                        case 4:
                                        case 5:
                                            if (WorldGen.genRand.NextBool())
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-1, 2);
                                            else if (WorldGen.genRand.NextBool(4))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-2, 3);
                                            else if (WorldGen.genRand.NextBool(6))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-3, 4);
                                            break;

                                        case 6:
                                        case 7:
                                            if (WorldGen.genRand.NextBool())
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-1, 2);
                                            else if (WorldGen.genRand.NextBool(3))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-2, 3);
                                            else if (WorldGen.genRand.NextBool(4))
                                                ashIslandGenLimiter += WorldGen.genRand.Next(-3, 4);
                                            break;
                                    }
                                }
                            }

                            if (ashIslandGenLimiter < randomizedAshIslandHeightLimit)
                                ashIslandGenLimiter = randomizedAshIslandHeightLimit;

                            if (ashIslandGenLimiter > randomizedAshIslandDepthLimit)
                                ashIslandGenLimiter = randomizedAshIslandDepthLimit;
                        }

                        for (int y = ashIslandGenLimiter; y > ashIslandGenLimiter - 10; y--)
                            Main.tile[ashIslandTilePlacementX, y].LiquidAmount = 0;

                        for (int y = ashIslandGenLimiter; y < Main.maxTilesY; y++)
                        {
                            Main.tile[ashIslandTilePlacementX, y].Clear(TileDataType.All);
                            Main.tile[ashIslandTilePlacementX, y].Get<TileWallWireStateData>().HasTile = true;
                            Main.tile[ashIslandTilePlacementX, y].TileType = TileID.Ash;
                        }
                    }

                    // This is necessary due to earlier calculations
                    int startX = ashIslandX + ashIslandXAdjustment + extraDistanceBetweenAshIslands_PerIslandSide + randomWidthReduction_LeftSide;
                    int endX = ashIslandTilePlacementX2 + extraDistanceBetweenAshIslands_PerIslandSide * 2;

                    // Small holes
                    double holeFrequency = 0.00005 / numIslands;
                    for (int j = 0; j < (int)((double)(Main.maxTilesX * Main.maxTilesY) * holeFrequency); j++)
                        WorldGen.TileRunner(WorldGen.genRand.Next(startX, endX), WorldGen.genRand.Next(randomizedAshIslandHeightLimit + 30, Main.maxTilesY), WorldGen.genRand.Next(4, 7), WorldGen.genRand.Next(4, 7), -2);

                    // Place smaller hellstone splotches in the ash island
                    // I don't want there to be too many here because I don't want to encourage players to destroy the environmental Wall of Flesh arena
                    double hellstoneFrequency = 0.0002 / numIslands;
                    for (int j = 0; j < (int)((double)(Main.maxTilesX * Main.maxTilesY) * hellstoneFrequency); j++)
                        WorldGen.TileRunner(WorldGen.genRand.Next(startX, endX), WorldGen.genRand.Next(randomizedAshIslandHeightLimit, Main.maxTilesY), WorldGen.genRand.Next(1, 5), WorldGen.genRand.Next(2, 5), TileID.Hellstone);
                }
            }
            else
            {
                int ashIslandTilePlacementX = ashIslandX;
                int ashIslandTilePlacementX2 = ashIslandX2;
                int ashIslandGenLimiter = Main.maxTilesY - 1;
                bool ashIslandGenLimitHit = false;
                Liquid.QuickWater(-2);
                for (; ashIslandGenLimiter < Main.maxTilesY - 1 || ashIslandTilePlacementX < ashIslandTilePlacementX2; ashIslandTilePlacementX++)
                {
                    // Less random ash island terrain to make unmodified traversal less annoying
                    if (!ashIslandGenLimitHit)
                    {
                        // Steeper ash island edges
                        ashIslandGenLimiter -= WorldGen.genRand.Next(1, 4);
                        if (ashIslandGenLimiter < ashIslandDepthLimit)
                            ashIslandGenLimitHit = true;
                    }
                    else if (ashIslandTilePlacementX >= ashIslandTilePlacementX2)
                    {
                        // Steeper ash island edges
                        ashIslandGenLimiter += WorldGen.genRand.Next(1, 4);
                        if (ashIslandGenLimiter > Main.maxTilesY - 1)
                            ashIslandGenLimiter = Main.maxTilesY - 1;
                    }
                    else
                    {
                        if ((ashIslandTilePlacementX <= Main.maxTilesX / 2 - 5 || ashIslandTilePlacementX >= Main.maxTilesX / 2 + 5) && WorldGen.genRand.NextBool(4))
                        {
                            if (WorldGen.genRand.NextBool(3))
                                ashIslandGenLimiter += WorldGen.genRand.Next(-1, 2);
                            else if (WorldGen.genRand.NextBool(6))
                                ashIslandGenLimiter += WorldGen.genRand.Next(-2, 3);
                            else if (WorldGen.genRand.NextBool(8))
                                ashIslandGenLimiter += WorldGen.genRand.Next(-4, 5);
                        }

                        if (ashIslandGenLimiter < ashIslandHeightLimit)
                            ashIslandGenLimiter = ashIslandHeightLimit;

                        if (ashIslandGenLimiter > ashIslandDepthLimit)
                            ashIslandGenLimiter = ashIslandDepthLimit;
                    }

                    for (int y = ashIslandGenLimiter; y > ashIslandGenLimiter - 10; y--)
                        Main.tile[ashIslandTilePlacementX, y].LiquidAmount = 0;

                    for (int y = ashIslandGenLimiter; y < Main.maxTilesY; y++)
                    {
                        Main.tile[ashIslandTilePlacementX, y].Clear(TileDataType.All);
                        Main.tile[ashIslandTilePlacementX, y].Get<TileWallWireStateData>().HasTile = true;
                        Main.tile[ashIslandTilePlacementX, y].TileType = TileID.Ash;
                    }
                }
            }

            // Generate a line of background walls from the approximate underworld lava line and down
            int maxWallHeight = MaxPillarHeight;
            int randomizedWallSectionsHeight = RandomizedPillarSectionHeight;
            int maxWallTypes = 4;
            int newWallTypeStart = maxWallHeight / maxWallTypes;
            int maxWallDepth = Main.maxTilesY - MaxPillarDepthAboveTheUnderworldFloor;
            int smoulderingStoneWallStartDepth = maxWallDepth - maxWallHeight;
            int cinderWallStartDepth = smoulderingStoneWallStartDepth + newWallTypeStart;
            int emberWallStartDepth = cinderWallStartDepth + newWallTypeStart;
            int magmaWallStartDepth = emberWallStartDepth + newWallTypeStart;
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = smoulderingStoneWallStartDepth; y < maxWallDepth; y++)
                {
                    if (y < smoulderingStoneWallStartDepth + randomizedWallSectionsHeight)
                    {
                        switch (smoulderingStoneWallStartDepth + randomizedWallSectionsHeight - y)
                        {
                            case 4:
                                if (WorldGen.genRand.NextBool(5))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;

                            case 3:
                                if (WorldGen.genRand.NextBool(3))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;

                            case 2:
                                if (WorldGen.genRand.NextBool())
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;

                            case 1:
                                if (WorldGen.genRand.Next(4) > 0)
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;
                        }
                    }
                    else if (y < cinderWallStartDepth)
                    {
                        Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                    }
                    else if (y < cinderWallStartDepth + randomizedWallSectionsHeight)
                    {
                        switch (cinderWallStartDepth + randomizedWallSectionsHeight - y)
                        {
                            case 4:
                                if (WorldGen.genRand.NextBool(5))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;

                            case 3:
                                if (WorldGen.genRand.NextBool(3))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;

                            case 2:
                                if (WorldGen.genRand.NextBool())
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;

                            case 1:
                                if (WorldGen.genRand.Next(4) > 0)
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                                break;
                        }
                    }
                    else if (y < emberWallStartDepth)
                    {
                        Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                    }
                    else if (y < emberWallStartDepth + randomizedWallSectionsHeight)
                    {
                        switch (emberWallStartDepth + randomizedWallSectionsHeight - y)
                        {
                            case 4:
                                if (WorldGen.genRand.NextBool(5))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                break;

                            case 3:
                                if (WorldGen.genRand.NextBool(3))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                break;

                            case 2:
                                if (WorldGen.genRand.NextBool())
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                break;

                            case 1:
                                if (WorldGen.genRand.Next(4) > 0)
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe2;
                                break;
                        }
                    }
                    else if (y < magmaWallStartDepth)
                    {
                        Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                    }
                    else if (y < magmaWallStartDepth + randomizedWallSectionsHeight)
                    {
                        switch (magmaWallStartDepth + randomizedWallSectionsHeight - y)
                        {
                            case 4:
                                if (WorldGen.genRand.NextBool(5))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe3;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                break;

                            case 3:
                                if (WorldGen.genRand.NextBool(3))
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe3;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                break;

                            case 2:
                                if (WorldGen.genRand.NextBool())
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe3;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                break;

                            case 1:
                                if (WorldGen.genRand.Next(4) > 0)
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe3;
                                else
                                    Main.tile[x, y].WallType = WallID.LavaUnsafe1;
                                break;
                        }
                    }
                    else
                        Main.tile[x, y].WallType = WallID.LavaUnsafe3;
                }
            }

            // Generate background walls in underworld tiles from Main.maxTilesY - 200 and down
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = Main.maxTilesY - 200; y <= smoulderingStoneWallStartDepth + randomizedWallSectionsHeight; y++)
                {
                    // The placed wall must be surrounded by non-sloped, non-half brick full tiles, otherwise it won't look right
                    bool surroundedByFullTiles = false;
                    bool breakLoop = false;

                    int indexStartX = x - 1;
                    if (indexStartX < 0)
                        indexStartX = 0;

                    int indexEndX = x + 1;
                    if (indexEndX > Main.maxTilesX)
                        indexEndX = Main.maxTilesX;

                    for (int surroundingTileIndexX = indexStartX; surroundingTileIndexX <= indexEndX; surroundingTileIndexX++)
                    {
                        if (breakLoop)
                            break;

                        for (int surroundingTileIndexY = y - 1; surroundingTileIndexY <= y + 1; surroundingTileIndexY++)
                        {
                            if (Main.tile[surroundingTileIndexX, surroundingTileIndexY].HasTile &&
                                !Main.tile[surroundingTileIndexX, surroundingTileIndexY].IsHalfBlock &&
                                Main.tile[surroundingTileIndexX, surroundingTileIndexY].Slope == SlopeType.Solid)
                            {
                                surroundedByFullTiles = true;
                            }
                            else
                            {
                                surroundedByFullTiles = false;
                                breakLoop = true;
                                break;
                            }
                        }
                    }

                    if (surroundedByFullTiles)
                        Main.tile[x, y].WallType = WallID.LavaUnsafe4;
                }
            }

            // More cursed magic water function
            Liquid.QuickWater(-2);

            // Create grass on ash
            for (int x = ashIslandX; x < ashIslandX2 + 15; x++)
            {
                for (int y = Main.maxTilesY - 300; y < ashIslandDepthLimit + 30; y++)
                {
                    Main.tile[x, y].LiquidAmount = 0;

                    if (Main.tile[x, y].TileType == TileID.Ash && Main.tile[x, y].HasTile &&
                        (!Main.tile[x - 1, y - 1].HasTile || !Main.tile[x, y - 1].HasTile ||
                        !Main.tile[x + 1, y - 1].HasTile || !Main.tile[x - 1, y].HasTile ||
                        !Main.tile[x + 1, y].HasTile || !Main.tile[x - 1, y + 1].HasTile ||
                        !Main.tile[x, y + 1].HasTile || !Main.tile[x + 1, y + 1].HasTile))
                        Main.tile[x, y].TileType = TileID.AshGrass;
                }
            }

            // Obsidian and hellstone towers...
            AddHellHouses();
        }

        public static void NewUnderworldStructures()
        {
            if (!WorldGen.remixWorldGen)
            {
                // Generate some structures on the ash islands
                int ashIslandX = (int)((double)Main.maxTilesX * AshIslandGenStart);
                int ashIslandX2 = (int)((double)Main.maxTilesX * AshIslandGenEnd);
                bool cragsLocationIsLeft = GenVars.dungeonLocation < Main.maxTilesX / 2;
                int ashIslandDepthLimit = Main.maxTilesY - AshIslandDepthMaxAboveTheUnderworldFloor;
                int ashIslandHeightLimit = Main.maxTilesY - AshIslandHeightMaxAboveTheUnderworldFloor;

                // Keep track of world size to adjust house distances
                float houseDistanceMult = Main.maxTilesX / 4200f;

                // Large = 8, Medium = 6, Small = 4
                int numIslands = (int)(Main.maxTilesX / 4200f * 4f);

                // Small and medium worlds get less structures
                bool smallWorld = numIslands == SmallWorldIslands;
                bool mediumWorld = numIslands == MediumWorldIslands;
                bool largeWorld = numIslands == LargeWorldIslands;

                // Total extra distance between islands for lava lakes
                int totalExtraDistanceBetweenAshIslands = (int)((double)Main.maxTilesX * 0.04);

                // Extra distance per island
                // The right side looks better with half of this adjustment subtracted
                int divisor = cragsLocationIsLeft ? 2 : 4;
                int extraDistanceBetweenAshIslands_PerIslandSide = totalExtraDistanceBetweenAshIslands / numIslands / divisor;

                // Calculate distance between islands
                int sideAdjustmentMultiplier = cragsLocationIsLeft ? 1 : -1;
                int distanceBetweenStructures = (ashIslandX2 - ashIslandX) / numIslands;
                int firstIslandWidth = distanceBetweenStructures + (extraDistanceBetweenAshIslands_PerIslandSide * sideAdjustmentMultiplier);
                int firstStructureDistanceFromIslandEdge = firstIslandWidth / 2;
                int distanceBetweenStructures_AfterFirstIsland = distanceBetweenStructures;

                // Pick an atrium type
                // Small worlds get a random single atrium
                // Medium and large worlds get a guaranteed shadow chest atrium and another random non-shadow chest atrium
                string atriumMapKey;
                int atriumType = smallWorld ? WorldGen.genRand.Next(3) : WorldGen.genRand.Next(2);
                int numAtriums = smallWorld ? 1 : 2;
                if (!smallWorld)
                {
                    switch (atriumType)
                    {
                        default:
                        case 0:
                            atriumMapKey = BrimstoneAtriumType1Key;
                            break;

                        case 1:
                            atriumMapKey = BrimstoneAtriumType3Key;
                            break;
                    }
                }
                else
                {
                    switch (atriumType)
                    {
                        default:
                        case 0:
                            atriumMapKey = BrimstoneAtriumType1Key;
                            break;

                        case 1:
                            atriumMapKey = BrimstoneAtriumType2Key;
                            break;

                        case 2:
                            atriumMapKey = BrimstoneAtriumType3Key;
                            break;
                    }
                }
                var atriumSchematic = TileMaps[atriumMapKey];

                // Pick stockade types
                List<int> stockades = new List<int>();
                int numStockades = largeWorld ? 2 : 1;
                int stockadeTypes = 4;
                do
                {
                    // Chose a random stockade to add to the list
                    int chosenStockade = WorldGen.genRand.Next(stockadeTypes);

                    // Don't choose the same stockade twice
                    bool alreadyContainsThisStockadeType = stockades.Contains(chosenStockade);

                    if (!alreadyContainsThisStockadeType)
                        stockades.Add(chosenStockade);
                }
                while (stockades.Count < numStockades);

                string[] stockadesToGenerate = new string[numStockades];
                for (int stockadeIndex = 0; stockadeIndex < numStockades; stockadeIndex++)
                {
                    switch (stockades[stockadeIndex])
                    {
                        default:
                        case 0:
                            stockadesToGenerate[stockadeIndex] = BarbedStockadeType1Key;
                            break;

                        case 1:
                            stockadesToGenerate[stockadeIndex] = BarbedStockadeType2Key;
                            break;

                        case 2:
                            stockadesToGenerate[stockadeIndex] = BarbedStockadeType3Key;
                            break;

                        case 3:
                            stockadesToGenerate[stockadeIndex] = BarbedStockadeType4Key;
                            break;
                    }
                }

                // Offsets for structures
                int atriumOffsetY = 8;

                int cacheOffsetY = 45;

                int sanctumOffsetY = 65;

                int strongholdOffsetX = cragsLocationIsLeft ? 40 : 20;
                int strongholdOffsetY = 3;
                int secondStrongholdOffsetY = 5;

                int dungeonOffsetX = cragsLocationIsLeft ? 30 : 50;
                int dungeonOffsetY = 4;

                int stockadeOffsetX = largeWorld ? strongholdOffsetX : 0;
                int secondStockadeOffsetX = dungeonOffsetX;
                int stockadeOffsetY = stockadesToGenerate[0] == BarbedStockadeType4Key ? -7 : 8;

                // Place structures
                if (cragsLocationIsLeft)
                {
                    //
                    // Place atriums
                    //
                    // Atrium location is on the crags side
                    int atriumGenX = ashIslandX + firstStructureDistanceFromIslandEdge;
                    int atriumGenY = ashIslandHeightLimit;
                    while (!Main.tile[atriumGenX, atriumGenY].HasTile)
                        atriumGenY++;

                    Point atriumPlacementPoint = new Point(atriumGenX, atriumGenY + atriumOffsetY);
                    SchematicAnchor anchorType = SchematicAnchor.Center;
                    bool place = true;
                    if (!smallWorld)
                    {
                        PlaceSchematic(BrimstoneAtriumType2Key, atriumPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));

                        // Protect the structure
                        Rectangle atriumProtectionArea = CalamityUtils.GetSchematicProtectionArea(atriumSchematic, atriumPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(atriumProtectionArea, 10);

                        // Move index further along to keep structures spread apart
                        atriumGenX += distanceBetweenStructures_AfterFirstIsland;

                        // Reset the Y index
                        atriumGenY = ashIslandHeightLimit;
                        while (!Main.tile[atriumGenX, atriumGenY].HasTile)
                            atriumGenY++;

                        // Placement point for the second atrium
                        atriumPlacementPoint = new Point(atriumGenX, atriumGenY + atriumOffsetY);
                    }

                    if (atriumMapKey == BrimstoneAtriumType2Key)
                        PlaceSchematic(atriumMapKey, atriumPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));
                    else
                        PlaceSchematic<Action<Chest>>(atriumMapKey, atriumPlacementPoint, anchorType, ref place);

                    // Protect the structure
                    Rectangle atriumProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(atriumSchematic, atriumPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(atriumProtectionArea2, 10);

                    //
                    // Place sanctums, strongholds, dungeons, and stockades
                    //
                    // All of these structures are on the demonic side
                    int sanctumGenX = ashIslandX2 - firstStructureDistanceFromIslandEdge;
                    int sanctumGenY = ashIslandDepthLimit + sanctumOffsetY;

                    int strongholdGenY = ashIslandHeightLimit;
                    while (!Main.tile[sanctumGenX + strongholdOffsetX, strongholdGenY].HasTile)
                        strongholdGenY++;

                    int dungeonGenY = ashIslandHeightLimit;
                    while (!Main.tile[sanctumGenX - dungeonOffsetX, dungeonGenY].HasTile)
                        dungeonGenY++;

                    int stockadeGenX = sanctumGenX - (smallWorld ? distanceBetweenStructures_AfterFirstIsland : distanceBetweenStructures_AfterFirstIsland * 2);
                    int stockadeGenY = ashIslandHeightLimit;
                    while (!Main.tile[stockadeGenX + stockadeOffsetX, stockadeGenY].HasTile)
                        stockadeGenY++;

                    Point sanctumPlacementPoint = new Point(sanctumGenX, sanctumGenY);
                    Point strongholdPlacementPoint = new Point(sanctumGenX + strongholdOffsetX, strongholdGenY + strongholdOffsetY);
                    Point dungeonPlacementPoint = new Point(sanctumGenX - dungeonOffsetX, dungeonGenY + dungeonOffsetY);
                    Point stockadePlacementPoint = new Point(stockadeGenX + stockadeOffsetX, stockadeGenY + stockadeOffsetY);
                    if (largeWorld)
                    {
                        PlaceSchematic<Action<Chest>>(stockadesToGenerate[0], stockadePlacementPoint, anchorType, ref place);

                        // Protect the structure
                        Rectangle stockadeProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[stockadesToGenerate[0]], stockadePlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(stockadeProtectionArea, 10);

                        // Reset the Y index for stockades
                        stockadeGenY = ashIslandHeightLimit;
                        while (!Main.tile[stockadeGenX - secondStockadeOffsetX, stockadeGenY].HasTile)
                            stockadeGenY++;

                        // Placement point for the second stockade
                        int secondStockadeOffsetY = stockadesToGenerate[1] == BarbedStockadeType4Key ? -7 : 8;
                        stockadePlacementPoint = new Point(stockadeGenX - secondStockadeOffsetX, stockadeGenY + secondStockadeOffsetY);
                    }
                    if (!smallWorld)
                    {
                        PlaceSchematic<Action<Chest>>(SanctumofOblivionType1Key, sanctumPlacementPoint, anchorType, ref place);

                        // Protect the structure
                        Rectangle sanctumProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[SanctumofOblivionType1Key], sanctumPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(sanctumProtectionArea, 10);

                        PlaceSchematic(HellstoneStrongholdType1Key, strongholdPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));

                        // Protect the structure
                        Rectangle strongholdProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[HellstoneStrongholdType1Key], strongholdPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(strongholdProtectionArea, 10);

                        PlaceSchematic<Action<Chest>>(DemonicDungeonType1Key, dungeonPlacementPoint, anchorType, ref place);

                        // Protect the structure
                        Rectangle dungeonProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[DemonicDungeonType1Key], dungeonPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(dungeonProtectionArea, 10);

                        // Move index further along to keep structures spread apart
                        sanctumGenX -= distanceBetweenStructures_AfterFirstIsland;

                        // Reset the Y index for strongholds
                        strongholdGenY = ashIslandHeightLimit;
                        while (!Main.tile[sanctumGenX + strongholdOffsetX, strongholdGenY].HasTile)
                            strongholdGenY++;

                        // Placement point for the second stronghold
                        strongholdPlacementPoint = new Point(sanctumGenX + strongholdOffsetX, strongholdGenY + secondStrongholdOffsetY);

                        // Reset the Y index for dungeons
                        dungeonGenY = ashIslandHeightLimit;
                        while (!Main.tile[sanctumGenX - dungeonOffsetX, dungeonGenY].HasTile)
                            dungeonGenY++;

                        // Placement point for the second dungeon
                        dungeonPlacementPoint = new Point(sanctumGenX - dungeonOffsetX, dungeonGenY + dungeonOffsetY);

                        // Placement point for the second sanctum
                        sanctumPlacementPoint = new Point(sanctumGenX, sanctumGenY);
                    }

                    // Large worlds have two stockades
                    string secondStockade = largeWorld ? stockadesToGenerate[1] : stockadesToGenerate[0];
                    PlaceSchematic<Action<Chest>>(secondStockade, stockadePlacementPoint, anchorType, ref place);

                    // Protect the structure
                    Rectangle stockadeProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondStockade], stockadePlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(stockadeProtectionArea2, 10);

                    // Small worlds only have one sanctum
                    string secondSanctum = smallWorld ? SanctumofOblivionType1Key : (WorldGen.genRand.NextBool() ? SanctumofOblivionType2Key : SanctumofOblivionType3Key);
                    PlaceSchematic<Action<Chest>>(secondSanctum, sanctumPlacementPoint, anchorType, ref place);

                    // Protect the structure
                    Rectangle sanctumProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondSanctum], sanctumPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(sanctumProtectionArea2, 10);

                    // Small worlds only have one stronghold
                    string secondStronghold = smallWorld ? HellstoneStrongholdType1Key : HellstoneStrongholdType2Key;
                    PlaceSchematic(secondStronghold, strongholdPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));

                    // Protect the structure
                    Rectangle strongholdProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondStronghold], strongholdPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(strongholdProtectionArea2, 10);

                    // Small worlds only have one dungeon
                    string secondDungeon = smallWorld ? DemonicDungeonType1Key : DemonicDungeonType2Key;
                    PlaceSchematic<Action<Chest>>(secondDungeon, dungeonPlacementPoint, anchorType, ref place);

                    // Protect the structure
                    Rectangle dungeonProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondDungeon], dungeonPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(dungeonProtectionArea2, 10);
                }
                else
                {
                    int atriumGenX = ashIslandX2 - firstStructureDistanceFromIslandEdge;
                    int atriumGenY = ashIslandHeightLimit;
                    while (!Main.tile[atriumGenX, atriumGenY].HasTile)
                        atriumGenY++;

                    Point atriumPlacementPoint = new Point(atriumGenX, atriumGenY + atriumOffsetY);
                    SchematicAnchor anchorType = SchematicAnchor.Center;
                    bool place = true;
                    if (!smallWorld)
                    {
                        PlaceSchematic(BrimstoneAtriumType2Key, atriumPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));

                        Rectangle atriumProtectionArea = CalamityUtils.GetSchematicProtectionArea(atriumSchematic, atriumPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(atriumProtectionArea, 10);

                        atriumGenX -= distanceBetweenStructures_AfterFirstIsland;

                        atriumGenY = ashIslandHeightLimit;
                        while (!Main.tile[atriumGenX, atriumGenY].HasTile)
                            atriumGenY++;

                        atriumPlacementPoint = new Point(atriumGenX, atriumGenY + atriumOffsetY);
                    }

                    if (atriumMapKey == BrimstoneAtriumType2Key)
                        PlaceSchematic(atriumMapKey, atriumPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));
                    else
                        PlaceSchematic<Action<Chest>>(atriumMapKey, atriumPlacementPoint, anchorType, ref place);

                    Rectangle atriumProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(atriumSchematic, atriumPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(atriumProtectionArea2, 10);

                    int sanctumGenX = ashIslandX + firstStructureDistanceFromIslandEdge;
                    int sanctumGenY = ashIslandDepthLimit + sanctumOffsetY;

                    int strongholdGenY = ashIslandHeightLimit;
                    while (!Main.tile[sanctumGenX - strongholdOffsetX, strongholdGenY].HasTile)
                        strongholdGenY++;

                    int dungeonGenY = ashIslandHeightLimit;
                    while (!Main.tile[sanctumGenX + dungeonOffsetX, dungeonGenY].HasTile)
                        dungeonGenY++;

                    int stockadeGenX = sanctumGenX + (smallWorld ? distanceBetweenStructures_AfterFirstIsland : distanceBetweenStructures_AfterFirstIsland * 2);
                    int stockadeGenY = ashIslandHeightLimit;
                    while (!Main.tile[stockadeGenX - stockadeOffsetX, stockadeGenY].HasTile)
                        stockadeGenY++;

                    Point sanctumPlacementPoint = new Point(sanctumGenX, sanctumGenY);
                    Point strongholdPlacementPoint = new Point(sanctumGenX - strongholdOffsetX, strongholdGenY + strongholdOffsetY);
                    Point dungeonPlacementPoint = new Point(sanctumGenX + dungeonOffsetX, dungeonGenY + dungeonOffsetY);
                    Point stockadePlacementPoint = new Point(stockadeGenX - stockadeOffsetX, stockadeGenY + stockadeOffsetY);
                    if (largeWorld)
                    {
                        PlaceSchematic<Action<Chest>>(stockadesToGenerate[0], stockadePlacementPoint, anchorType, ref place);

                        // Protect the structure
                        Rectangle stockadeProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[stockadesToGenerate[0]], stockadePlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(stockadeProtectionArea, 10);

                        // Reset the Y index for stockades
                        stockadeGenY = ashIslandHeightLimit;
                        while (!Main.tile[stockadeGenX + secondStockadeOffsetX, stockadeGenY].HasTile)
                            stockadeGenY++;

                        // Placement point for the second stockade
                        int secondStockadeOffsetY = stockadesToGenerate[1] == BarbedStockadeType4Key ? -7 : 8;
                        stockadePlacementPoint = new Point(stockadeGenX + secondStockadeOffsetX, stockadeGenY + secondStockadeOffsetY);
                    }
                    if (!smallWorld)
                    {
                        PlaceSchematic<Action<Chest>>(SanctumofOblivionType1Key, sanctumPlacementPoint, anchorType, ref place);

                        Rectangle sanctumProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[SanctumofOblivionType1Key], sanctumPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(sanctumProtectionArea, 10);

                        PlaceSchematic(HellstoneStrongholdType1Key, strongholdPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));

                        Rectangle strongholdProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[HellstoneStrongholdType1Key], strongholdPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(strongholdProtectionArea, 10);

                        PlaceSchematic<Action<Chest>>(DemonicDungeonType1Key, dungeonPlacementPoint, anchorType, ref place);

                        Rectangle dungeonProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[DemonicDungeonType1Key], dungeonPlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(dungeonProtectionArea, 10);

                        sanctumGenX += distanceBetweenStructures_AfterFirstIsland;

                        strongholdGenY = ashIslandHeightLimit;
                        while (!Main.tile[sanctumGenX - strongholdOffsetX, strongholdGenY].HasTile)
                            strongholdGenY++;

                        strongholdPlacementPoint = new Point(sanctumGenX - strongholdOffsetX, strongholdGenY + secondStrongholdOffsetY);

                        dungeonGenY = ashIslandHeightLimit;
                        while (!Main.tile[sanctumGenX + dungeonOffsetX, dungeonGenY].HasTile)
                            dungeonGenY++;

                        dungeonPlacementPoint = new Point(sanctumGenX + dungeonOffsetX, dungeonGenY + dungeonOffsetY);

                        sanctumPlacementPoint = new Point(sanctumGenX, sanctumGenY);
                    }

                    string secondStockade = largeWorld ? stockadesToGenerate[1] : stockadesToGenerate[0];
                    PlaceSchematic<Action<Chest>>(secondStockade, stockadePlacementPoint, anchorType, ref place);

                    Rectangle stockadeProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondStockade], stockadePlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(stockadeProtectionArea2, 10);

                    string secondSanctum = smallWorld ? SanctumofOblivionType1Key : (WorldGen.genRand.NextBool() ? SanctumofOblivionType2Key : SanctumofOblivionType3Key);
                    PlaceSchematic<Action<Chest>>(secondSanctum, sanctumPlacementPoint, anchorType, ref place);

                    Rectangle sanctumProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondSanctum], sanctumPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(sanctumProtectionArea2, 10);

                    string secondStronghold = smallWorld ? HellstoneStrongholdType1Key : HellstoneStrongholdType2Key;
                    PlaceSchematic(secondStronghold, strongholdPlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillAtriumChests));

                    Rectangle strongholdProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondStronghold], strongholdPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(strongholdProtectionArea2, 10);

                    string secondDungeon = smallWorld ? DemonicDungeonType1Key : DemonicDungeonType2Key;
                    PlaceSchematic<Action<Chest>>(secondDungeon, dungeonPlacementPoint, anchorType, ref place);

                    Rectangle dungeonProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondDungeon], dungeonPlacementPoint, anchorType);
                    CalamityUtils.AddProtectedStructure(dungeonProtectionArea2, 10);
                }

                // Pick cache types
                List<int> caches = new List<int>();
                int numCaches = largeWorld ? 8 : 4; // THIS NUMBER MUST BE EVEN!!!
                int cacheTypes = 6;
                int totalCachePositions = numCaches / 2;
                do
                {
                    // Chose a random cache to add to the list
                    int chosenCache = WorldGen.genRand.Next(cacheTypes);

                    // Don't choose the same cache twice
                    bool alreadyContainsThisCacheType = caches.Contains(chosenCache);

                    // Avoid an infinite loop by picking a random duplicate cache if the max is reached
                    bool pickRandomDuplicateCache = caches.Count >= cacheTypes;
                    if (pickRandomDuplicateCache)
                        pickRandomDuplicateCache = caches[caches.Count - totalCachePositions] != chosenCache;

                    if (!alreadyContainsThisCacheType || pickRandomDuplicateCache)
                        caches.Add(chosenCache);
                }
                while (caches.Count < numCaches);

                // Chose random caches to not place
                // 1 is not placed in a small/medium world
                // 3 are not placed in a large world
                List<int> cachesToNotPlace = new List<int>();
                int numCachesToNotPlace = largeWorld ? 3 : 1;
                do
                {
                    // Chose a random cache to add to the list
                    int chosenCache = WorldGen.genRand.Next(numCaches);

                    // Don't choose the same cache twice
                    bool alreadyContainsThisCacheType = cachesToNotPlace.Contains(chosenCache);
                    if (!alreadyContainsThisCacheType)
                        cachesToNotPlace.Add(chosenCache);
                }
                while (cachesToNotPlace.Count < numCachesToNotPlace);

                //
                // Place caches
                //
                int cacheGenX = ashIslandX + firstStructureDistanceFromIslandEdge + (distanceBetweenStructures_AfterFirstIsland * numAtriums);
                int cacheGenY = ashIslandDepthLimit + cacheOffsetY;
                int randomAdjustmentX = 0;
                int randomAdjustmentY = 0;
                int minRandomX = -30;
                int maxRandomX = -20;
                int minRandomY = 15;
                int maxRandomY = 20;
                for (int cacheIndex = 0; cacheIndex < totalCachePositions; cacheIndex++)
                {
                    randomAdjustmentX += WorldGen.genRand.Next(minRandomX, maxRandomX + 1);
                    randomAdjustmentX = (int)MathHelper.Clamp(randomAdjustmentX, minRandomX, maxRandomX);
                    randomAdjustmentY += WorldGen.genRand.Next(minRandomY, maxRandomY + 1);
                    randomAdjustmentY = (int)MathHelper.Clamp(randomAdjustmentY, minRandomY, maxRandomY);

                    string cacheMapKey;
                    switch (caches[cacheIndex])
                    {
                        default:
                        case 0:
                            cacheMapKey = BonescrapperCacheType1Key;
                            break;

                        case 1:
                            cacheMapKey = BonescrapperCacheType2Key;
                            break;

                        case 2:
                            cacheMapKey = BonescrapperCacheType3Key;
                            break;

                        case 3:
                            cacheMapKey = BonescrapperCacheType4Key;
                            break;

                        case 4:
                            cacheMapKey = BonescrapperCacheType5Key;
                            break;

                        case 5:
                            cacheMapKey = BonescrapperCacheType6Key;
                            break;
                    }

                    Point cachePlacementPoint = new Point(cacheGenX + randomAdjustmentX, cacheGenY + randomAdjustmentY);
                    SchematicAnchor anchorType = SchematicAnchor.Center;
                    bool place = true;

                    bool canPlaceEvenCache = true;
                    switch (cacheIndex)
                    {
                        default:
                        case 0:
                            canPlaceEvenCache = !cachesToNotPlace.Contains(0);
                            break;

                        case 1:
                            canPlaceEvenCache = !cachesToNotPlace.Contains(2);
                            break;

                        case 2:
                            canPlaceEvenCache = !cachesToNotPlace.Contains(4);
                            break;

                        case 3:
                            canPlaceEvenCache = !cachesToNotPlace.Contains(6);
                            break;
                    }

                    // Cache types 2, 3, and 6 have chests
                    if (canPlaceEvenCache)
                    {
                        if (cacheMapKey == BonescrapperCacheType2Key || cacheMapKey == BonescrapperCacheType3Key || cacheMapKey == BonescrapperCacheType6Key)
                            PlaceSchematic(cacheMapKey, cachePlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillCacheChests));
                        else
                            PlaceSchematic<Action<Chest>>(cacheMapKey, cachePlacementPoint, anchorType, ref place);

                        // Protect the structure
                        Rectangle cacheProtectionArea = CalamityUtils.GetSchematicProtectionArea(TileMaps[cacheMapKey], cachePlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(cacheProtectionArea, 5);
                    }

                    string secondCacheMapKey;
                    switch (caches[cacheIndex + totalCachePositions])
                    {
                        default:
                        case 0:
                            secondCacheMapKey = BonescrapperCacheType1Key;
                            break;

                        case 1:
                            secondCacheMapKey = BonescrapperCacheType2Key;
                            break;

                        case 2:
                            secondCacheMapKey = BonescrapperCacheType3Key;
                            break;

                        case 3:
                            secondCacheMapKey = BonescrapperCacheType4Key;
                            break;

                        case 4:
                            secondCacheMapKey = BonescrapperCacheType5Key;
                            break;

                        case 5:
                            secondCacheMapKey = BonescrapperCacheType6Key;
                            break;
                    }

                    bool canPlaceOddCache = true;
                    switch (cacheIndex)
                    {
                        default:
                        case 0:
                            canPlaceOddCache = !cachesToNotPlace.Contains(1);
                            break;

                        case 1:
                            canPlaceOddCache = !cachesToNotPlace.Contains(3);
                            break;

                        case 2:
                            canPlaceOddCache = !cachesToNotPlace.Contains(5);
                            break;

                        case 3:
                            canPlaceOddCache = !cachesToNotPlace.Contains(7);
                            break;
                    }

                    // Place second cache to the right of the first
                    if (canPlaceOddCache)
                    {
                        Point secondCachePlacementPoint = cachePlacementPoint + new Point(WorldGen.genRand.Next(60, 76), 0);
                        if (secondCacheMapKey == BonescrapperCacheType2Key || secondCacheMapKey == BonescrapperCacheType3Key || secondCacheMapKey == BonescrapperCacheType6Key)
                            PlaceSchematic(secondCacheMapKey, secondCachePlacementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillCacheChests));
                        else
                            PlaceSchematic<Action<Chest>>(secondCacheMapKey, secondCachePlacementPoint, anchorType, ref place);

                        // Protect the structure
                        Rectangle cacheProtectionArea2 = CalamityUtils.GetSchematicProtectionArea(TileMaps[secondCacheMapKey], secondCachePlacementPoint, anchorType);
                        CalamityUtils.AddProtectedStructure(cacheProtectionArea2, 5);
                    }

                    // Reset positions and move cache placement along the X axis
                    cacheGenX = ashIslandX + firstStructureDistanceFromIslandEdge + (distanceBetweenStructures_AfterFirstIsland * (numAtriums + cacheIndex + 1));
                    cacheGenY = ashIslandDepthLimit + cacheOffsetY;
                }
            }
        }

        public static void NewUnderworldPillars()
        {
            int maxWallHeight = MaxPillarHeight;
            int randomizedWallSectionsHeight = RandomizedPillarSectionHeight;
            int maxWallDepth = Main.maxTilesY - MaxPillarDepthAboveTheUnderworldFloor;
            int smoulderingStoneWallStartDepth = maxWallDepth - maxWallHeight;

            // Generate after houses to avoid complications
            // Place pillars of walls to show that the roof of the underworld is being held up
            // Do not place any pillars in the Brimstone Crags
            bool cragsLocationIsLeft = GenVars.dungeonLocation < Main.maxTilesX / 2;
            int brimstoneCragsSize = Main.maxTilesX / 5;
            int brimstoneCragsLocationStart = cragsLocationIsLeft ? (25 + brimstoneCragsSize) : ((Main.maxTilesX - brimstoneCragsSize) - 25);
            int pillarDistanceFromCragsLimit = 160;
            int pillarIndexStartX = (cragsLocationIsLeft ? brimstoneCragsLocationStart : 0) + pillarDistanceFromCragsLimit;
            int pillarIndexEndX = (cragsLocationIsLeft ? Main.maxTilesX : brimstoneCragsLocationStart) - pillarDistanceFromCragsLimit;
            int pillarIndexStarY = Main.maxTilesY - 200;
            int pillarIndexEndY = smoulderingStoneWallStartDepth + randomizedWallSectionsHeight;
            int pillarMidPointY = pillarIndexStarY + (pillarIndexEndY - pillarIndexStarY) / 2;
            int pillarCutOffCheckStart = pillarMidPointY - 10;
            int pillarMinWidth = 6;
            int pillarMinWidthPerSide = pillarMinWidth / 2;
            int pillarMinStartingWidth = 18;
            int pillarMaxWidth = 24;
            int pillarMaxWidthPerSide = pillarMaxWidth / 2;
            int pillarY = 0;
            int pillarLeftSize = 0;
            int pillarRightSize = 0;
            int topTileSectionSize = 10;
            int pillarTopTileSectionCutOff = pillarIndexStarY + topTileSectionSize;
            ushort pillarTileID = Main.zenithWorld ? TileID.PoopBlock : (ushort)ModContent.TileType<Dreadstone>();
            ushort pillarWallID = Main.zenithWorld ? WallID.PoopWall : WallID.RocksUnsafe3;

            // Use the x tile index to find pillar locations
            for (int x = pillarIndexStartX; x < pillarIndexEndX; x += WorldGen.genRand.Next(80, pillarDistanceFromCragsLimit + 1))
            {
                int pillarStartingWidth = WorldGen.genRand.Next(pillarMinStartingWidth, pillarMaxWidth + 1);
                pillarLeftSize = pillarStartingWidth / 2;
                pillarRightSize = pillarStartingWidth / 2;
                if (pillarStartingWidth % 2 != 0)
                {
                    if (WorldGen.genRand.NextBool())
                        pillarLeftSize++;
                    else
                        pillarRightSize++;
                }

                int startOfPillarX = x - pillarMaxWidthPerSide;
                int endOfPillarX = x + pillarMaxWidthPerSide;
                int wallsDetected = 0;
                bool tooManyWallsToPlacePillarRow = false;

                // Start the pillar at the top of the underworld and extend down to the hellstone wall
                for (int y = pillarIndexStarY; y <= pillarIndexEndY; y++)
                {
                    // The amount of walls required to stop generating the pillar when it's deeper than pillarCutoffCheckStart
                    int pillarCutOffSize = pillarLeftSize + pillarRightSize + 2;

                    // Get the amount of empty space on the left and right of the pillar's X mid point
                    int emptySpaceOnLeft = pillarMaxWidthPerSide - pillarLeftSize;
                    int emptySpaceOnRight = pillarMaxWidthPerSide - pillarRightSize;

                    // Used for wall cleanup around top tile section
                    int topTileSectionWallCleanupLeft = startOfPillarX + emptySpaceOnLeft + 1;
                    int topTileSectionWallCleanupRight = endOfPillarX - emptySpaceOnRight - 1;

                    // Create sections of pillar
                    for (int x2 = startOfPillarX; x2 <= endOfPillarX; x2++)
                    {
                        // Check if there are too many walls in a line (this ends the current pillar generation and moves to the next)
                        if (Main.tile[x2, y].WallType != WallID.None)
                        {
                            // Only try to kill further pillar gen if the gen is beyond the pillar's mid point minus 10 tiles
                            if (y > pillarCutOffCheckStart)
                                wallsDetected++;

                            if (wallsDetected > pillarCutOffSize)
                            {
                                tooManyWallsToPlacePillarRow = true;
                                break;
                            }
                        }
                        else
                        {
                            if (x2 >= startOfPillarX + emptySpaceOnLeft && x2 <= endOfPillarX - emptySpaceOnRight)
                            {
                                Main.tile[x2, y].WallType = pillarWallID;

                                // Blocks on top to hold the pillars
                                if (y < pillarTopTileSectionCutOff)
                                {
                                    // Clean up walls at the top so it doesn't look ugly
                                    if (x2 <= topTileSectionWallCleanupLeft || x2 >= topTileSectionWallCleanupRight || y == pillarIndexStarY)
                                        Main.tile[x2, y].WallType = WallID.None;

                                    Main.tile[x2, y].Get<TileWallWireStateData>().HasTile = true;
                                    Main.tile[x2, y].TileType = pillarTileID;
                                }
                            }
                        }
                    }

                    // Break out if too many walls are already in place
                    if (tooManyWallsToPlacePillarRow)
                        break;
                    else
                        wallsDetected = 0;

                    if (y < pillarMidPointY)
                    {
                        // Becomes more likely to become thinner the further down the pillar is before the mid point
                        pillarY++;
                        int chance = pillarY / 3;
                        if (chance < 2)
                            chance = 2;

                        if (!WorldGen.genRand.NextBool(chance))
                        {
                            switch (WorldGen.genRand.Next(4))
                            {
                                default:
                                    break;

                                case 0:
                                    pillarLeftSize--;
                                    break;

                                case 1:
                                    pillarRightSize--;
                                    break;

                                case 2:
                                    pillarLeftSize--;
                                    pillarRightSize--;
                                    break;
                            }

                            // Cap the min width of each side of the pillar
                            if (pillarLeftSize < pillarMinWidthPerSide)
                                pillarLeftSize = pillarMinWidthPerSide;
                            if (pillarRightSize < pillarMinWidthPerSide)
                                pillarRightSize = pillarMinWidthPerSide;
                        }
                    }
                    else
                    {
                        // Becomes more likely to become thicker the further down the pillar is beyond the mid point
                        pillarY -= 2;
                        int chance = pillarY;
                        if (chance < 2)
                            chance = 2;

                        if (WorldGen.genRand.NextBool(chance))
                        {
                            switch (WorldGen.genRand.Next(4))
                            {
                                default:
                                    break;

                                case 0:
                                    pillarLeftSize++;
                                    break;

                                case 1:
                                    pillarRightSize++;
                                    break;

                                case 2:
                                    pillarLeftSize++;
                                    pillarRightSize++;
                                    break;
                            }

                            // Cap the max width of each side of the pillar
                            if (pillarLeftSize > pillarMaxWidthPerSide)
                                pillarLeftSize = pillarMaxWidthPerSide;
                            if (pillarRightSize > pillarMaxWidthPerSide)
                                pillarRightSize = pillarMaxWidthPerSide;
                        }
                    }
                }
            }
        }

        public static void AshTreesAndGrass()
        {
            // Start generating islands at this point
            int ashIslandX = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? AshIslandGenStart_Remix : AshIslandGenStart));

            // Stop generating islands at this point
            int ashIslandX2 = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? AshIslandGenEnd_Remix : AshIslandGenEnd));

            // Ash island gen limits
            int ashIslandDepthLimit = Main.maxTilesY - AshIslandDepthMaxAboveTheUnderworldFloor;

            // Place ash trees
            for (int x = ashIslandX; x < ashIslandX2 + 15; x++)
            {
                for (int y = Main.maxTilesY - 200; y < ashIslandDepthLimit + 30; y++)
                {
                    if (Main.tile[x, y].TileType == TileID.AshGrass && Main.tile[x, y].HasTile && !Main.tile[x, y - 1].HasTile && WorldGen.genRand.NextBool(3))
                        WorldGen.TryGrowingTreeByType(TileID.TreeAsh, x, y);
                }
            }

            // Drunk world ash grass and trees
            if (WorldGen.drunkWorldGen)
            {
                for (int x = 25; x < Main.maxTilesX - 25; x++)
                {
                    for (int y = Main.maxTilesY - 300; y < Main.maxTilesY - 100 + WorldGen.genRand.Next(-1, 2); y++)
                    {
                        if (Main.tile[x, y].TileType == TileID.Ash && Main.tile[x, y].HasTile &&
                            (!Main.tile[x - 1, y - 1].HasTile || !Main.tile[x, y - 1].HasTile ||
                            !Main.tile[x + 1, y - 1].HasTile || !Main.tile[x - 1, y].HasTile ||
                            !Main.tile[x + 1, y].HasTile || !Main.tile[x - 1, y + 1].HasTile ||
                            !Main.tile[x, y + 1].HasTile || !Main.tile[x + 1, y + 1].HasTile))
                            Main.tile[x, y].TileType = TileID.AshGrass;
                    }
                }

                for (int x = 25; x < Main.maxTilesX - 25; x++)
                {
                    for (int y = Main.maxTilesY - 200; y < Main.maxTilesY - 50; y++)
                    {
                        if (Main.tile[x, y].TileType == TileID.AshGrass && Main.tile[x, y].HasTile && !Main.tile[x, y - 1].HasTile && WorldGen.genRand.NextBool(3))
                            WorldGen.TryGrowingTreeByType(TileID.TreeAsh, x, y);
                    }
                }
            }
        }

        public static void PlaceGeyserTraps()
        {
            // Place Geyser Traps on ash islands
            // These are only generated in the no-man's land central islands
            if (!WorldGen.remixWorldGen)
            {
                // Large = 8, Medium = 6, Small = 4
                // No-man's land islands: Large = 4, Medium = 2, Small = 2
                int numIslands = (int)(Main.maxTilesX / 4200f * 4f);
                bool mediumWorld = numIslands == MediumWorldIslands;

                // Start generating traps at this point
                int ashIslandX = (int)((double)Main.maxTilesX * (mediumWorld ? 0.453 : 0.43));

                // Stop generating traps at this point
                int ashIslandX2 = (int)((double)Main.maxTilesX * (mediumWorld ? 0.546 : 0.57));

                // Small = 12.6 Medium = 19.2 Large = 25.2
                double trapFrequency = (double)Main.maxTilesX * 0.003;
                if (WorldGen.noTrapsWorldGen)
                    trapFrequency = ((!WorldGen.tenthAnniversaryWorldGen && !WorldGen.notTheBees) ? (trapFrequency * 100D) : (trapFrequency * 5D));
                else if (WorldGen.getGoodWorldGen)
                    trapFrequency *= 1.5;

                if (Main.starGame)
                    trapFrequency *= Main.starGameMath(0.2);

                int maxTrapPlacementAttempts = 1150;
                for (int trapIndex = 0; (double)trapIndex < trapFrequency; trapIndex++)
                {
                    for (int trapIndex2 = 0; trapIndex2 < maxTrapPlacementAttempts; trapIndex2++)
                    {
                        int trapPlacementX = WorldGen.genRand.Next(ashIslandX, ashIslandX2);
                        int trapPlacementY = WorldGen.genRand.Next(Main.maxTilesY - AshIslandHeightMaxAboveTheUnderworldFloor, Main.maxTilesY - 100);

                        if (GeyserTraps(trapPlacementX, trapPlacementY))
                            break;
                    }
                }
            }
        }

        private static bool GeyserTraps(int geyserX, int geyserY)
        {
            int geyserPlacementY = geyserY;

            while (!WorldGen.SolidTile(geyserX, geyserPlacementY))
            {
                geyserPlacementY++;
                if (geyserPlacementY > Main.maxTilesY - 10)
                    return false;
            }

            geyserPlacementY--;

            if (!WorldGen.InWorld(geyserX, geyserPlacementY, 3))
                return false;

            if (Main.tile[geyserX, geyserPlacementY].HasUnactuatedTile ||
                Main.tile[geyserX - 1, geyserPlacementY].HasUnactuatedTile ||
                Main.tile[geyserX + 1, geyserPlacementY].HasUnactuatedTile ||
                Main.tile[geyserX, geyserPlacementY - 1].HasUnactuatedTile ||
                Main.tile[geyserX - 1, geyserPlacementY - 1].HasUnactuatedTile ||
                Main.tile[geyserX + 1, geyserPlacementY - 1].HasUnactuatedTile ||
                Main.tile[geyserX, geyserPlacementY - 2].HasUnactuatedTile ||
                Main.tile[geyserX - 1, geyserPlacementY - 2].HasUnactuatedTile ||
                Main.tile[geyserX + 1, geyserPlacementY - 2].HasUnactuatedTile)
                return false;

            if (Main.tile[geyserX + 1, geyserPlacementY].HasTile)
                return false;

            // Only place on ash grass to avoid placing in the schematics
            if (Main.tile[geyserX, geyserPlacementY + 1].TileType != TileID.AshGrass ||
                Main.tile[geyserX + 1, geyserPlacementY + 1].TileType != TileID.AshGrass)
                return false;

            for (int k = geyserX; k <= geyserX + 1; k++)
            {
                int j2 = geyserPlacementY + 1;
                if (!WorldGen.SolidTile(k, j2))
                    return false;
            }

            int geyserPlacementY2 = WorldGen.genRand.Next(2);
            for (int l = 0; l < 2; l++)
            {
                Main.tile[geyserX + l, geyserPlacementY].Get<TileWallWireStateData>().HasTile = true;
                Main.tile[geyserX + l, geyserPlacementY].TileType = TileID.GeyserTrap;
                Main.tile[geyserX + l, geyserPlacementY].TileFrameX = (short)(18 * l + 36 * geyserPlacementY2);
                Main.tile[geyserX + l, geyserPlacementY].TileFrameY = 0;
            }

            return true;
        }

        private static void AddHellHouses()
        {
            // Original tower gen area was the outer quarters of the underworld
            // New tower gen area is the outer thirds of the underworld
            int towerGenArea = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? 0.25 : 0.33));

            // Generate towers
            for (int i = 100; i < Main.maxTilesX - 100; i++)
            {
                if (i > towerGenArea && i < Main.maxTilesX - towerGenArea)
                    continue;

                int hellFortGenY = Main.maxTilesY - 40;
                while (Main.tile[i, hellFortGenY].HasTile || Main.tile[i, hellFortGenY].LiquidAmount > 0)
                    hellFortGenY--;

                if (Main.tile[i, hellFortGenY + 1].HasTile)
                {
                    ushort hellFortTileType = (ushort)WorldGen.genRand.Next(TileID.ObsidianBrick, TileID.HellstoneBrick + 1);
                    ushort wallType = WallID.HellstoneBrickUnsafe;
                    if (WorldGen.genRand.Next(5) > 0)
                        hellFortTileType = TileID.ObsidianBrick;

                    if (hellFortTileType == TileID.ObsidianBrick)
                        wallType = WallID.ObsidianBrickUnsafe;

                    if (WorldGen.getGoodWorldGen)
                        hellFortTileType = TileID.HellstoneBrick;

                    // Place tower
                    WorldGen.HellFort(i, hellFortGenY, hellFortTileType, (byte)wallType);

                    // Move index further along to keep towers spread apart
                    // Original min and max values were, respectively, 30 and 130
                    // New min and max values are, respectively, 30 and 60
                    i += WorldGen.genRand.Next(30, WorldGen.remixWorldGen ? 130 : 60);

                    // Randomly add more distance between towers
                    // Original max value was 200
                    // New max value is 50
                    if (WorldGen.genRand.NextBool(10))
                        i += WorldGen.genRand.Next(WorldGen.remixWorldGen ? 200 : 50);
                }
            }

            // Placing torches in towers
            float torchAmountMult = Main.maxTilesX / 4200f;
            for (int torchIndex = 0; (float)torchIndex < 200f * torchAmountMult; torchIndex++)
            {
                int attempts = 0;
                bool placedTorch = false;
                while (!placedTorch)
                {
                    attempts++;
                    int x = WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.2), (int)((double)Main.maxTilesX * 0.8));
                    int y = WorldGen.genRand.Next(Main.maxTilesY - 300, Main.maxTilesY - 20);
                    if (Main.tile[x, y].HasTile && (Main.tile[x, y].TileType == TileID.ObsidianBrick || Main.tile[x, y].TileType == TileID.HellstoneBrick))
                    {
                        int xOffset = 0;
                        if (Main.tile[x - 1, y].WallType > 0)
                            xOffset = -1;
                        else if (Main.tile[x + 1, y].WallType > 0)
                            xOffset = 1;

                        if (!Main.tile[x + xOffset, y].HasTile && !Main.tile[x + xOffset, y + 1].HasTile)
                        {
                            // Check if a torch has already been placed in this location
                            bool torchAlreadyPlaced = false;
                            for (int k = x - 8; k < x + 8; k++)
                            {
                                for (int l = y - 8; l < y + 8; l++)
                                {
                                    if (Main.tile[k, l].HasTile && TileID.Sets.Torch[Main.tile[k, l].TileType])
                                    {
                                        torchAlreadyPlaced = true;
                                        break;
                                    }
                                }
                            }

                            // Place the torch
                            if (!torchAlreadyPlaced)
                            {
                                WorldGen.PlaceTile(x + xOffset, y, TileID.Torches, mute: true, forced: true, -1, 7);
                                placedTorch = true;
                            }
                        }
                    }

                    if (attempts > 1000)
                        placedTorch = true;
                }
            }

            // Place furniture in the towers
            double furnitureAmount = 4200000D / (double)Main.maxTilesX;
            for (int furnitureIndex = 0; (double)furnitureIndex < furnitureAmount; furnitureIndex++)
            {
                int attempts = 0;
                int furniturePlacementX = WorldGen.genRand.Next(towerGenArea, Main.maxTilesX - towerGenArea);
                int y = WorldGen.genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 20);
                while ((Main.tile[furniturePlacementX, y].WallType != WallID.HellstoneBrickUnsafe && Main.tile[furniturePlacementX, y].WallType != WallID.ObsidianBrickUnsafe) || Main.tile[furniturePlacementX, y].HasTile)
                {
                    furniturePlacementX = WorldGen.genRand.NextBool(2) ? WorldGen.genRand.Next(Main.maxTilesX - towerGenArea, Main.maxTilesX - 50) : WorldGen.genRand.Next(50, towerGenArea);
                    y = WorldGen.genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 20);

                    attempts++;
                    if (attempts > 100000)
                        break;
                }

                if (attempts > 100000 || (Main.tile[furniturePlacementX, y].WallType != WallID.HellstoneBrickUnsafe && Main.tile[furniturePlacementX, y].WallType != WallID.ObsidianBrickUnsafe) || Main.tile[furniturePlacementX, y].HasTile)
                    continue;

                for (; !WorldGen.SolidTile(furniturePlacementX, y) && y < Main.maxTilesY - 20; y++)
                {
                }

                y--;
                int placementAdjustment = furniturePlacementX;
                int placementAdjustment2 = furniturePlacementX;
                while (!Main.tile[placementAdjustment, y].HasTile && WorldGen.SolidTile(placementAdjustment, y + 1))
                    placementAdjustment--;

                placementAdjustment++;
                for (; !Main.tile[placementAdjustment2, y].HasTile && WorldGen.SolidTile(placementAdjustment2, y + 1); placementAdjustment2++)
                {
                }

                placementAdjustment2--;
                int placementSpaceLimit = placementAdjustment2 - placementAdjustment;
                int x = (placementAdjustment2 + placementAdjustment) / 2;
                if (Main.tile[x, y].HasTile || (Main.tile[x, y].WallType != WallID.HellstoneBrickUnsafe && Main.tile[x, y].WallType != WallID.ObsidianBrickUnsafe) || !WorldGen.SolidTile(x, y + 1))
                    continue;

                // Different styles of houses
                int style = 16;
                int style2 = 13;
                int style3 = 14;
                int style4 = 49;
                int style5 = 4;
                int style6 = 8;
                int style7 = 15;
                int style8 = 9;
                int style9 = 10;
                int style10 = 17;
                int style11 = 25;
                int style12 = 25;
                int style13 = 23;
                int style14 = 25;
                int styleChoice = WorldGen.genRand.Next(13);
                int sizeX = 0;
                int sizeY = 0;
                if (styleChoice == 0)
                {
                    sizeX = 5;
                    sizeY = 4;
                }

                if (styleChoice == 1)
                {
                    sizeX = 4;
                    sizeY = 3;
                }

                if (styleChoice == 2)
                {
                    sizeX = 3;
                    sizeY = 5;
                }

                if (styleChoice == 3)
                {
                    sizeX = 4;
                    sizeY = 6;
                }

                if (styleChoice == 4)
                {
                    sizeX = 3;
                    sizeY = 3;
                }

                if (styleChoice == 5)
                {
                    sizeX = 5;
                    sizeY = 3;
                }

                if (styleChoice == 6)
                {
                    sizeX = 5;
                    sizeY = 4;
                }

                if (styleChoice == 7)
                {
                    sizeX = 5;
                    sizeY = 4;
                }

                if (styleChoice == 8)
                {
                    sizeX = 5;
                    sizeY = 4;
                }

                if (styleChoice == 9)
                {
                    sizeX = 3;
                    sizeY = 5;
                }

                if (styleChoice == 10)
                {
                    sizeX = 5;
                    sizeY = 3;
                }

                if (styleChoice == 11)
                {
                    sizeX = 2;
                    sizeY = 4;
                }

                if (styleChoice == 12)
                {
                    sizeX = 3;
                    sizeY = 3;
                }

                // Check if there are any tiles in the way of furniture placement
                for (int placementIndexX = x - sizeX; placementIndexX <= x + sizeX; placementIndexX++)
                {
                    for (int placementIndexY = y - sizeY; placementIndexY <= y; placementIndexY++)
                    {
                        if (Main.tile[placementIndexX, placementIndexY].HasTile)
                        {
                            styleChoice = -1;
                            break;
                        }
                    }
                }

                // Check if there is enough space to place furniture
                if ((double)placementSpaceLimit < (double)sizeX * 1.75)
                    styleChoice = -1;

                switch (styleChoice)
                {
                    case 0:
                        WorldGen.PlaceTile(x, y, TileID.Tables, mute: true, forced: false, -1, style2);
                        int candlePlacementX = WorldGen.genRand.Next(6);
                        if (candlePlacementX < 3)
                            WorldGen.PlaceTile(x + candlePlacementX, y - 2, TileID.Candles, mute: true, forced: false, -1, style12);

                        if (!Main.tile[x, y].HasTile)
                            break;

                        if (!Main.tile[x - 2, y].HasTile)
                        {
                            WorldGen.PlaceTile(x - 2, y, TileID.Chairs, mute: true, forced: false, -1, style);
                            if (Main.tile[x - 2, y].HasTile)
                            {
                                Main.tile[x - 2, y].TileFrameX += 18;
                                Main.tile[x - 2, y - 1].TileFrameX += 18;
                            }
                        }

                        if (!Main.tile[x + 2, y].HasTile)
                            WorldGen.PlaceTile(x + 2, y, TileID.Chairs, mute: true, forced: false, -1, style);

                        break;

                    case 1:
                        WorldGen.PlaceTile(x, y, TileID.WorkBenches, mute: true, forced: false, -1, style3);
                        int candlePlacementX2 = WorldGen.genRand.Next(4);
                        if (candlePlacementX2 < 2)
                            WorldGen.PlaceTile(x + candlePlacementX2, y - 1, TileID.Candles, mute: true, forced: false, -1, style12);

                        if (!Main.tile[x, y].HasTile)
                            break;

                        if (WorldGen.genRand.NextBool())
                        {
                            if (!Main.tile[x - 1, y].HasTile)
                            {
                                WorldGen.PlaceTile(x - 1, y, TileID.Chairs, mute: true, forced: false, -1, style);
                                if (Main.tile[x - 1, y].HasTile)
                                {
                                    Main.tile[x - 1, y].TileFrameX += 18;
                                    Main.tile[x - 1, y - 1].TileFrameX += 18;
                                }
                            }
                        }
                        else if (!Main.tile[x + 2, y].HasTile)
                            WorldGen.PlaceTile(x + 2, y, TileID.Chairs, mute: true, forced: false, -1, style);

                        break;

                    case 2:
                        WorldGen.PlaceTile(x, y, TileID.Statues, mute: true, forced: false, -1, style4);
                        break;

                    case 3:
                        WorldGen.PlaceTile(x, y, TileID.Bookcases, mute: true, forced: false, -1, style5);
                        break;

                    case 4:

                        if (WorldGen.genRand.NextBool())
                        {
                            WorldGen.PlaceTile(x, y, TileID.Chairs, mute: true, forced: false, -1, style);
                            Main.tile[x, y].TileFrameX += 18;
                            Main.tile[x, y - 1].TileFrameX += 18;
                        }
                        else
                            WorldGen.PlaceTile(x, y, TileID.Chairs, mute: true, forced: false, -1, style);

                        break;

                    case 5:
                        if (WorldGen.genRand.NextBool())
                            WorldGen.Place4x2(x, y, TileID.Beds, 1, style6);
                        else
                            WorldGen.Place4x2(x, y, TileID.Beds, -1, style6);
                        break;

                    case 6:
                        WorldGen.PlaceTile(x, y, TileID.Pianos, mute: true, forced: false, -1, style7);
                        break;

                    case 7:
                        WorldGen.PlaceTile(x, y, TileID.Dressers, mute: true, forced: false, -1, style8);
                        break;

                    case 8:
                        WorldGen.PlaceTile(x, y, TileID.Benches, mute: true, forced: false, -1, style9);
                        break;

                    case 9:
                        WorldGen.PlaceTile(x, y, TileID.GrandfatherClocks, mute: true, forced: false, -1, style10);
                        break;

                    case 10:
                        if (WorldGen.genRand.NextBool())
                            WorldGen.Place4x2(x, y, TileID.Bathtubs, 1, style14);
                        else
                            WorldGen.Place4x2(x, y, TileID.Bathtubs, -1, style14);
                        break;

                    case 11:
                        WorldGen.PlaceTile(x, y, TileID.Lamps, mute: true, forced: false, -1, style13);
                        break;

                    case 12:
                        WorldGen.PlaceTile(x, y, TileID.Candelabras, mute: true, forced: false, -1, style11);
                        break;
                }
            }

            // Place paintings in the towers
            furnitureAmount = 420000D / (double)Main.maxTilesX;
            for (int paintingIndex = 0; (double)paintingIndex < furnitureAmount; paintingIndex++)
            {
                int attempts = 0;
                int x = WorldGen.genRand.Next(towerGenArea, Main.maxTilesX - towerGenArea);
                int y = WorldGen.genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 20);
                while ((Main.tile[x, y].WallType != WallID.HellstoneBrickUnsafe && Main.tile[x, y].WallType != WallID.ObsidianBrickUnsafe) || Main.tile[x, y].HasTile)
                {
                    x = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(Main.maxTilesX - towerGenArea, Main.maxTilesX - 50) : WorldGen.genRand.Next(50, towerGenArea);
                    y = WorldGen.genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 20);

                    attempts++;
                    if (attempts > 100000)
                        break;
                }

                if (attempts > 100000)
                    continue;

                int paintingPlacementX = x;
                int paintingPlacementX2 = x;
                int paintingPlacementY = y;
                int paintingPlacementY2 = y;
                int sizeY = 0;
                for (int num32 = 0; num32 < 2; num32++)
                {
                    paintingPlacementX = x;
                    paintingPlacementX2 = x;
                    while (!Main.tile[paintingPlacementX, y].HasTile && (Main.tile[paintingPlacementX, y].WallType == WallID.HellstoneBrickUnsafe || Main.tile[paintingPlacementX, y].WallType == WallID.ObsidianBrickUnsafe))
                        paintingPlacementX--;

                    paintingPlacementX++;
                    for (; !Main.tile[paintingPlacementX2, y].HasTile && (Main.tile[paintingPlacementX2, y].WallType == WallID.HellstoneBrickUnsafe || Main.tile[paintingPlacementX2, y].WallType == WallID.ObsidianBrickUnsafe); paintingPlacementX2++)
                    {
                    }

                    paintingPlacementX2--;
                    x = (paintingPlacementX + paintingPlacementX2) / 2;
                    paintingPlacementY = y;
                    paintingPlacementY2 = y;
                    while (!Main.tile[x, paintingPlacementY].HasTile && (Main.tile[x, paintingPlacementY].WallType == WallID.HellstoneBrickUnsafe || Main.tile[x, paintingPlacementY].WallType == WallID.ObsidianBrickUnsafe))
                        paintingPlacementY--;

                    paintingPlacementY++;
                    for (; !Main.tile[x, paintingPlacementY2].HasTile && (Main.tile[x, paintingPlacementY2].WallType == WallID.HellstoneBrickUnsafe || Main.tile[x, paintingPlacementY2].WallType == WallID.ObsidianBrickUnsafe); paintingPlacementY2++)
                    {
                    }

                    paintingPlacementY2--;
                    y = (paintingPlacementY + paintingPlacementY2) / 2;
                }

                // Clamp this due to out of bounds errors
                y = (int)MathHelper.Clamp(y, 1, Main.maxTilesY - 1);

                paintingPlacementX = x;
                paintingPlacementX2 = x;
                while (!Main.tile[paintingPlacementX, y].HasTile && !Main.tile[paintingPlacementX, y - 1].HasTile && !Main.tile[paintingPlacementX, y + 1].HasTile)
                {
                    paintingPlacementX--;

                    // Ensure we don't get out of bounds errors
                    if (paintingPlacementX < 0)
                    {
                        paintingPlacementX = 0;
                        break;
                    }
                }

                paintingPlacementX++;
                for (; !Main.tile[paintingPlacementX2, y].HasTile && !Main.tile[paintingPlacementX2, y - 1].HasTile && !Main.tile[paintingPlacementX2, y + 1].HasTile; paintingPlacementX2++)
                {
                }

                paintingPlacementX2--;
                paintingPlacementY = y;
                paintingPlacementY2 = y;
                while (!Main.tile[x, paintingPlacementY].HasTile && !Main.tile[x - 1, paintingPlacementY].HasTile && !Main.tile[x + 1, paintingPlacementY].HasTile)
                    paintingPlacementY--;

                paintingPlacementY++;
                for (; !Main.tile[x, paintingPlacementY2].HasTile && !Main.tile[x - 1, paintingPlacementY2].HasTile && !Main.tile[x + 1, paintingPlacementY2].HasTile; paintingPlacementY2++)
                {
                }

                paintingPlacementY2--;
                x = (paintingPlacementX + paintingPlacementX2) / 2;
                y = (paintingPlacementY + paintingPlacementY2) / 2;
                int sizeX = paintingPlacementX2 - paintingPlacementX;
                sizeY = paintingPlacementY2 - paintingPlacementY;
                if (sizeX <= 7 || sizeY <= 5)
                    continue;

                if (!WorldGen.nearPicture2(x, y))
                {
                    PaintingEntry paintingEntry = WorldGen.RandHellPicture();
                    if (!WorldGen.nearPicture(x, y))
                        WorldGen.PlaceTile(x, y, paintingEntry.tileType, mute: true, forced: false, -1, paintingEntry.style);
                }
            }

            // An array used for banner styles
            int[] bannerArray = new int[3]
            {
                WorldGen.genRand.Next(16, 22),
                WorldGen.genRand.Next(16, 22),
                WorldGen.genRand.Next(16, 22)
            };

            while (bannerArray[1] == bannerArray[0])
                bannerArray[1] = WorldGen.genRand.Next(16, 22);

            while (bannerArray[2] == bannerArray[0] || bannerArray[2] == bannerArray[1])
                bannerArray[2] = WorldGen.genRand.Next(16, 22);

            // Place hanging tiles
            furnitureAmount = 420000D / (double)Main.maxTilesX;
            for (int hangingTileIndex = 0; (double)hangingTileIndex < furnitureAmount; hangingTileIndex++)
            {
                int attempts = 0;
                int x;
                int y;
                do
                {
                    x = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(Main.maxTilesX - towerGenArea, Main.maxTilesX - 50) : WorldGen.genRand.Next(50, towerGenArea);
                    y = WorldGen.genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 20);
                    attempts++;
                }
                while (attempts <= 100000 && ((Main.tile[x, y].WallType != WallID.HellstoneBrickUnsafe && Main.tile[x, y].WallType != WallID.ObsidianBrickUnsafe) || Main.tile[x, y].HasTile));

                if (attempts > 100000)
                    continue;

                while (!WorldGen.SolidTile(x, y) && y > 10)
                    y--;

                y++;
                if (Main.tile[x, y].WallType != WallID.HellstoneBrickUnsafe && Main.tile[x, y].WallType != WallID.ObsidianBrickUnsafe)
                    continue;

                int hangingTileType = WorldGen.genRand.Next(3);
                int chandelierStyle = 32;
                int lanternStyle = 32;
                int hangingTileSizeX;
                int hangingTileSizeY;
                switch (hangingTileType)
                {
                    default:
                        hangingTileSizeX = 1;
                        hangingTileSizeY = 3;
                        break;
                    case 1:
                        hangingTileSizeX = 3;
                        hangingTileSizeY = 3;
                        break;
                    case 2:
                        hangingTileSizeX = 1;
                        hangingTileSizeY = 2;
                        break;
                }

                for (int placementIndexX = x - 1; placementIndexX <= x + hangingTileSizeX; placementIndexX++)
                {
                    for (int placementIndexY = y; placementIndexY <= y + hangingTileSizeY; placementIndexY++)
                    {
                        Tile tile = Main.tile[x, y];
                        if (placementIndexX < x || placementIndexX == x + hangingTileSizeX)
                        {
                            if (tile.HasTile)
                            {
                                switch (tile.TileType)
                                {
                                    case TileID.ClosedDoor:
                                    case TileID.OpenDoor:
                                    case TileID.Chandeliers:
                                    case TileID.HangingLanterns:
                                    case TileID.Banners:
                                        hangingTileType = -1;
                                        break;
                                }
                            }
                        }
                        else if (tile.HasTile)
                            hangingTileType = -1;
                    }
                }

                switch (hangingTileType)
                {
                    case 0:
                        WorldGen.PlaceTile(x, y, TileID.Banners, mute: true, forced: false, -1, bannerArray[WorldGen.genRand.Next(3)]);
                        break;
                    case 1:
                        WorldGen.PlaceTile(x, y, TileID.Chandeliers, mute: true, forced: false, -1, chandelierStyle);
                        break;
                    case 2:
                        WorldGen.PlaceTile(x, y, TileID.HangingLanterns, mute: true, forced: false, -1, lanternStyle);
                        break;
                }
            }
        }

        private static void FillAtriumChests(Chest chest, int Type, bool place)
        {
            int shadowChestItems = Utils.SelectRandom(WorldGen.genRand, ItemID.Sunfury, ItemID.FlowerofFire, ItemID.Flamelash, ItemID.DarkLance, ItemID.HellwingBow);
            int shadowChestItems2 = Utils.SelectRandom(WorldGen.genRand, ItemID.HellMinecart, ItemID.OrnateShadowKey, ItemID.HellCake);
            List<ChestItem> contents = new List<ChestItem>()
            {
                new ChestItem(shadowChestItems, 1),
                new ChestItem(shadowChestItems2, 1),
                new ChestItem(ItemID.ObsidianSkinPotion, WorldGen.genRand.Next(2, 5)),
                new ChestItem(ItemID.LifeforcePotion, WorldGen.genRand.Next(2, 5)),
                new ChestItem(ItemID.TeleportationPotion, WorldGen.genRand.Next(2, 5)),
                new ChestItem(ItemID.GoldCoin, WorldGen.genRand.Next(3, 6)),
            };

            for (int i = 0; i < contents.Count; i++)
            {
                chest.item[i].SetDefaults(contents[i].Type);
                chest.item[i].Prefix(-1);
                chest.item[i].stack = contents[i].Stack;
            }
        }

        private static void FillCacheChests(Chest chest, int Type, bool place)
        {
            int shadowChestItems = Utils.SelectRandom(WorldGen.genRand, ItemID.Sunfury, ItemID.FlowerofFire, ItemID.Flamelash, ItemID.DarkLance, ItemID.HellwingBow);
            int barTypes = Utils.SelectRandom(WorldGen.genRand, ItemID.MeteoriteBar, GenVars.goldBar == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar);
            int ammoTypes = Utils.SelectRandom(WorldGen.genRand, ItemID.HellfireArrow, GenVars.silverBar == TileID.Silver ? ItemID.SilverBullet : ItemID.TungstenBullet);
            int potionTypes = Utils.SelectRandom(WorldGen.genRand, ItemID.SpelunkerPotion, ItemID.FeatherfallPotion, ItemID.ManaRegenerationPotion, ItemID.MagicPowerPotion, ItemID.InvisibilityPotion, ItemID.HunterPotion, ItemID.HeartreachPotion);
            int potionTypes2 = Utils.SelectRandom(WorldGen.genRand, ItemID.GravitationPotion, ItemID.ThornsPotion, ItemID.WaterWalkingPotion, ItemID.BattlePotion, ItemID.InfernoPotion);
            int potionTypes3 = Utils.SelectRandom(WorldGen.genRand, ItemID.RecallPotion, ItemID.PotionOfReturn);
            int lightSourceTypes = Utils.SelectRandom(WorldGen.genRand, ItemID.Torch, ItemID.Glowstick);
            List<ChestItem> contents = new List<ChestItem>()
            {
                new ChestItem(shadowChestItems, 1),
                new ChestItem(ItemID.Dynamite, WorldGen.genRand.Next(1, 3)),
                new ChestItem(barTypes, WorldGen.genRand.Next(15, 31)),
                new ChestItem(ammoTypes, WorldGen.genRand.Next(50, 76)),
                new ChestItem(ItemID.RestorationPotion, WorldGen.genRand.Next(15, 21)),
                new ChestItem(potionTypes, WorldGen.genRand.Next(1, 3)),
                new ChestItem(potionTypes2, WorldGen.genRand.Next(1, 3)),
                new ChestItem(potionTypes3, WorldGen.genRand.Next(1, 3)),
                new ChestItem(lightSourceTypes, WorldGen.genRand.Next(15, 31)),
                new ChestItem(ItemID.GoldCoin, WorldGen.genRand.Next(2, 5)),
            };

            for (int i = 0; i < contents.Count; i++)
            {
                chest.item[i].SetDefaults(contents[i].Type);
                chest.item[i].Prefix(-1);
                chest.item[i].stack = contents[i].Stack;
            }
        }
    }
}
