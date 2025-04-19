using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;
using static tModPorter.ProgressUpdate;

namespace CalamityMod.World
{
    public class CustomUnderworld
    {
        private const int MaxIslands = 4;

        public static void NewUnderworld()
        {
            // Generate lower Underworld ash
            int ashDepth = Main.maxTilesY - WorldGen.genRand.Next(150, 190);
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                ashDepth += WorldGen.genRand.Next(-3, 4);
                if (ashDepth < Main.maxTilesY - 190)
                    ashDepth = Main.maxTilesY - 190;

                if (ashDepth > Main.maxTilesY - 160)
                    ashDepth = Main.maxTilesY - 160;

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
            int lavaDepth = Main.maxTilesY - WorldGen.genRand.Next(40, 70);
            for (int x = 10; x < Main.maxTilesX - 10; x++)
            {
                lavaDepth += WorldGen.genRand.Next(-10, 11);
                if (lavaDepth > Main.maxTilesY - 60)
                    lavaDepth = Main.maxTilesY - 60;

                if (lavaDepth < Main.maxTilesY - 110)
                    lavaDepth = Main.maxTilesY - 110;

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
            int ashIslandX = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? 0.38 : 0.37));

            // Stop generating islands at this point
            int ashIslandX2 = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? 0.62 : 0.63));

            // Ash island gen limits
            int ashIslandDepthLimit = Main.maxTilesY - 135;
            int ashIslandHeightLimit = Main.maxTilesY - 160;

            // Multiple islands in non-remix
            if (!WorldGen.remixWorldGen)
            {
                // Large = 4, Medium = 3, Small = 2
                int numIslands = (int)(Main.maxTilesX / 4200f * 2f);

                // Total extra distance between islands for lava lakes
                int totalExtraDistanceBetweenAshIslands = (int)((double)Main.maxTilesX * 0.02);

                // Extra distance per island
                // Due to this being done on both sides of each island, it is divided by 2
                int extraDistanceBetweenAshIslands = totalExtraDistanceBetweenAshIslands / numIslands / 2;

                // Calculate distance between islands
                int distanceBetweenIslands = (ashIslandX2 - ashIslandX) / numIslands;

                // Used for island height randomization
                int[] randomHeightAdjustmentLimits = new int[MaxIslands]
                {
                    WorldGen.genRand.Next(3) + 10,
                    WorldGen.genRand.Next(3) + 5,
                    WorldGen.genRand.Next(3),
                    WorldGen.genRand.Next(3) - 5
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

                    int randomizedIslandHeightAdjustment = randomHeightAdjustmentLimits[chosenIslandSize];
                    int randomizedAshIslandDepthLimit = ashIslandDepthLimit + randomizedIslandHeightAdjustment;
                    int randomizedAshIslandHeightLimit = ashIslandHeightLimit + randomizedIslandHeightAdjustment;

                    int ashIslandXAdjustment = distanceBetweenIslands * i;
                    int ashIslandX2Adjustment = distanceBetweenIslands * (numIslands - i - 1);
                    int ashIslandTilePlacementX = ashIslandX + ashIslandXAdjustment + extraDistanceBetweenAshIslands;
                    int ashIslandTilePlacementX2 = ashIslandX2 - ashIslandX2Adjustment - extraDistanceBetweenAshIslands;
                    int ashIslandGenLimiter = Main.maxTilesY - 1;
                    bool ashIslandGenLimitHit = false;
                    Liquid.QuickWater(-2);
                    for (; ashIslandGenLimiter < Main.maxTilesY - 1 || ashIslandTilePlacementX < ashIslandTilePlacementX2; ashIslandTilePlacementX++)
                    {
                        // Less random ash island terrain to make unmodified traversal less annoying
                        if (!ashIslandGenLimitHit)
                        {
                            // Steeper ash island edges
                            ashIslandGenLimiter -= WorldGen.genRand.Next(1, 8);
                            if (ashIslandGenLimiter < randomizedAshIslandDepthLimit)
                                ashIslandGenLimitHit = true;
                        }
                        else if (ashIslandTilePlacementX >= ashIslandTilePlacementX2)
                        {
                            // Steeper ash island edges
                            ashIslandGenLimiter += WorldGen.genRand.Next(1, 8);
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
                                else if (WorldGen.genRand.NextBool(9))
                                    ashIslandGenLimiter += WorldGen.genRand.Next(-3, 4);
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
                    int startX = ashIslandX + ashIslandXAdjustment + extraDistanceBetweenAshIslands;

                    // Lava holes in ash islands
                    double holeFrequency = 0.00008 / numIslands;
                    for (int j = 0; j < (int)((double)(Main.maxTilesX * Main.maxTilesY) * holeFrequency); j++)
                        WorldGen.TileRunner(WorldGen.genRand.Next(startX, ashIslandTilePlacementX2), WorldGen.genRand.Next(randomizedAshIslandHeightLimit + 15, Main.maxTilesY), WorldGen.genRand.Next(4, 7), WorldGen.genRand.Next(4, 7), -2);

                    // Place smaller hellstone splotches in the ash island
                    // I don't want there to be too many here because I don't want to encourage players to destroy the environmental Wall of Flesh arena
                    double hellstoneFrequency = 0.0002 / numIslands;
                    for (int j = 0; j < (int)((double)(Main.maxTilesX * Main.maxTilesY) * hellstoneFrequency); j++)
                        WorldGen.TileRunner(WorldGen.genRand.Next(startX, ashIslandTilePlacementX2), WorldGen.genRand.Next(randomizedAshIslandHeightLimit, Main.maxTilesY), WorldGen.genRand.Next(1, 5), WorldGen.genRand.Next(2, 5), TileID.Hellstone);
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

            // More cursed magic water function
            Liquid.QuickWater(-2);

            // Create grass on ash
            for (int x = ashIslandX; x < ashIslandX2 + 15; x++)
            {
                for (int y = Main.maxTilesY - 300; y < ashIslandDepthLimit + 20; y++)
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

            // Place ash trees
            for (int x = ashIslandX; x < ashIslandX2 + 15; x++)
            {
                for (int y = Main.maxTilesY - 200; y < ashIslandDepthLimit + 20; y++)
                {
                    if (Main.tile[x, y].TileType == TileID.AshGrass && Main.tile[x, y].HasTile && !Main.tile[x, y - 1].HasTile && WorldGen.genRand.NextBool(3))
                        WorldGen.TryGrowingTreeByType(TileID.TreeAsh, x, y);
                }
            }

            // Obsidian and hellstone towers...
            AddHellHouses();

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

            if (!WorldGen.remixWorldGen)
            {
                // Generate some small houses on the ash island
                int ashIslandX = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? 0.38 : 0.37));
                int ashIslandX2 = (int)((double)Main.maxTilesX * (WorldGen.remixWorldGen ? 0.62 : 0.63));
                int ashIslandDistance = ashIslandX2 - ashIslandX;
                int flatDistanceBetweenHellHouses = ashIslandDistance / 3;

                // Keep track of world size to adjust house distances
                float houseDistanceMult = Main.maxTilesX / 4200f;
                int maxRandomDistanceBetweenHouses = (int)(75 * houseDistanceMult);

                // ashIslandX + 100 places the first hell house at the perfect position on the left shore of the ash island
                // Add some random variance so that it doesn't feel so artificial
                int firstHouseLocation = ashIslandX + 100 + WorldGen.genRand.Next(maxRandomDistanceBetweenHouses);

                // Max amount of houses generated on ash island
                int maxHouses = 3;
                int placedHouses = 0;
                for (int i = firstHouseLocation; i < ashIslandX2 - 100; i++)
                {
                    // Start searching at Main.maxTilesY - 130 because ashIsland's max depth is Main.maxTilesY - 135
                    int hellHouseGenY = Main.maxTilesY - 130;
                    while (Main.tile[i, hellHouseGenY].HasTile || Main.tile[i, hellHouseGenY].LiquidAmount > 0)
                        hellHouseGenY--;

                    if (Main.tile[i, hellHouseGenY + 1].HasTile)
                    {
                        // Place house
                        // TODO -- Stip's houses will be generated here eventually
                        //WorldGen.HellHouse(i, hellHouseGenY);

                        // Move index further along to keep houses spread apart
                        i += flatDistanceBetweenHellHouses + WorldGen.genRand.Next(maxRandomDistanceBetweenHouses);

                        // Increment placed houses index and break loop once enough are placed
                        placedHouses++;
                        if (placedHouses >= maxHouses)
                            break;
                    }
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

                paintingPlacementX = x;
                paintingPlacementX2 = x;
                while (!Main.tile[paintingPlacementX, y].HasTile && !Main.tile[paintingPlacementX, y - 1].HasTile && !Main.tile[paintingPlacementX, y + 1].HasTile)
                    paintingPlacementX--;

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
    }
}
