using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Tiles.FloralParadise;
using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace CalamityMod.World.Minibiomes
{
    public class FloralParadiseMinibiome
    {
        public const int MaxHorizontalDistanceFromWorldCenter = 1250;

        public const int MinSurfaceDepth = 250;

        public const int MaxSurfaceDepth = 550;

        public static void GenerateInstances()
        {
            float worldSize = Main.maxTilesX / 4200f;
            int totalCaves = (int)(worldSize * 5f);
            int[] blacklistedTiles = new int[]
            {
                TileID.Sand,
                TileID.Sandstone,
                TileID.HardenedSand,
                TileID.DesertFossil,
                TileID.SnowBlock,
                TileID.IceBlock,
                ModContent.TileType<LaboratoryPanels>(),
                ModContent.TileType<LaboratoryPipePlating>(),
                ModContent.TileType<LaboratoryPlating>(),
                ModContent.TileType<HazardChevronPanels>(),
                ModContent.TileType<RustedPlating>(),
                ModContent.TileType<RustedPipes>(),
                ModContent.TileType<Navystone>(),
                ModContent.TileType<EutrophicSand>(),
            };

            List<Rectangle> existingCaves = new List<Rectangle>();
            for (int i = 0; i < totalCaves; i++)
            {
                int x = Main.maxTilesX / 2 + WorldGen.genRand.Next(-MaxHorizontalDistanceFromWorldCenter, MaxHorizontalDistanceFromWorldCenter);
                int y = (int)WorldGen.worldSurface + WorldGen.genRand.Next(MinSurfaceDepth, MaxSurfaceDepth);
                int width = (int)(WorldGen.genRand.Next(80, 100) * worldSize);
                int height = (int)(WorldGen.genRand.Next(70, 95) * worldSize);
                Rectangle caveArea = Utils.CenteredRectangle(new Vector2(x, y), new Vector2(width, height));

                // Try again if this selected location is near blacklisted tiles.
                // This is done to prevent logical consistencies/intrusion on unrelated biomes.
                bool needsToTryAgain = false;
                for (int dx = -40; dx < 40; dx++)
                {
                    for (int dy = -40; dy < 40; dy++)
                    {
                        if (!needsToTryAgain)
                        {
                            Tile tile = CalamityUtils.ParanoidTileRetrieval(x + dx, y + dy);
                            needsToTryAgain = tile.active() && blacklistedTiles.Contains(tile.type);
                        }
                        else
                            break;
                    }
                }

                if (!needsToTryAgain)
                    needsToTryAgain = existingCaves.Any(c => caveArea.Intersects(c));

                if (needsToTryAgain)
                {
                    i--;
                    continue;
                }

                Place(new Vector2(x, y), width, height);
                existingCaves.Add(caveArea);
            }
        }

        public static void Place(Vector2 placementCenter, int width, int height)
        {
            int seed = WorldGen.genRand.Next();
            bool[,] caveState = new bool[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float noise = CalamityUtils.PerlinNoise2D(i / 180f, j / 180f, 3, seed) * 0.5f + 0.5f;
                    caveState[i, j] = noise >= 0.48f && noise < 0.64f;
                    if (WorldGen.genRand.NextBool(3))
                        caveState[i, j] = WorldGen.genRand.NextBool();
                }
            }

            // Repeatedly smoothen out caves with celluar automata.
            for (int i = 0; i < 12; i++)
                caveState = SimulateCelluarAutomata(caveState);

            // Reset tile states based on the above results.
            ushort grassID = (ushort)ModContent.TileType<FloralGrass>();
            ushort dirtID = (ushort)ModContent.TileType<RichDirt>();
            ushort stoneID = (ushort)ModContent.TileType<FloralStone>();
            bool[,] grassDirtMap = new bool[width, height];
            for (int x = (int)placementCenter.X; x < (int)placementCenter.X + width; x++)
            {
                for (int y = (int)placementCenter.Y; y < (int)placementCenter.Y + height; y++)
                {
                    // Don't attempt to change tile data if a tile is outside of the world for some reason.
                    if (!WorldGen.InWorld(x, y, 1))
                        continue;

                    int arrayRelativeX = x - (int)placementCenter.X;
                    int arrayRelativeY = y - (int)placementCenter.Y;
                    float distanceFromCenter = Vector2.Distance(new Vector2(arrayRelativeX / (float)width, arrayRelativeY / (float)height), Vector2.One * 0.5f) * 1.75f;
                    if (distanceFromCenter > 1f)
                        distanceFromCenter = 1f;

                    float ditherChance = Utils.InverseLerp(0.97f, 0.72f, distanceFromCenter, true);

                    Main.tile[x, y].wall = WallID.None;
                    if (WorldGen.genRand.NextFloat() < ditherChance)
                    {
                        if (!caveState[arrayRelativeX, arrayRelativeY] && distanceFromCenter < 0.7f)
                            Main.tile[x, y] = new Tile();
                        else
                        {
                            bool useGrass = CountCellsAtPosition(caveState, arrayRelativeX, arrayRelativeY, false) >= 1;
                            grassDirtMap[arrayRelativeX, arrayRelativeY] = useGrass;
                            Main.tile[x, y] = new Tile();
                            Main.tile[x, y].ResetToType(useGrass ? grassID : stoneID);
                            WorldGen.SquareTileFrame(x, y);
                        }
                    }
                }
            }

            // Perform secondary passes that overlay dirt behind grass.
            for (int i = 0; i < 2; i++)
            {
                for (int x = (int)placementCenter.X; x < (int)placementCenter.X + width; x++)
                {
                    for (int y = (int)placementCenter.Y; y < (int)placementCenter.Y + height; y++)
                    {
                        int arrayRelativeX = x - (int)placementCenter.X;
                        int arrayRelativeY = y - (int)placementCenter.Y;
                        bool nearbyGrassOrDirt = false;
                        if (arrayRelativeX - 1 >= 0 && grassDirtMap[arrayRelativeX - 1, arrayRelativeY])
                            nearbyGrassOrDirt = true;
                        else if (arrayRelativeX + 1 < width && grassDirtMap[arrayRelativeX + 1, arrayRelativeY])
                            nearbyGrassOrDirt = true;
                        else if (arrayRelativeY - 1 >= 0 && grassDirtMap[arrayRelativeX, arrayRelativeY - 1])
                            nearbyGrassOrDirt = true;
                        else if (arrayRelativeY + 1 < height && grassDirtMap[arrayRelativeX, arrayRelativeY + 1])
                            nearbyGrassOrDirt = true;

                        bool canPerformPlacement = i == 0 || WorldGen.genRand.NextBool();
                        Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                        if (canPerformPlacement && tile.active() && tile.type == stoneID && nearbyGrassOrDirt)
                            Main.tile[x, y].type = dirtID;
                    }
                }

                // Reset the dirt/grass map for successive passes.
                for (int x = (int)placementCenter.X; x < (int)placementCenter.X + width; x++)
                {
                    for (int y = (int)placementCenter.Y; y < (int)placementCenter.Y + height; y++)
                    {
                        Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                        int arrayRelativeX = x - (int)placementCenter.X;
                        int arrayRelativeY = y - (int)placementCenter.Y;
                        grassDirtMap[arrayRelativeX, arrayRelativeY] = tile.type == grassID || tile.type == dirtID;
                    }
                }
            }

            // Create walls.
            for (int x = (int)placementCenter.X; x < (int)placementCenter.X + width; x++)
            {
                for (int y = (int)placementCenter.Y; y < (int)placementCenter.Y + height; y++)
                {
                    float noise = CalamityUtils.PerlinNoise2D(x / 160f, y / 160f, 3, unchecked(seed - 13)) * 0.5f + 0.5f;
                    if (noise >= 0.42f && noise < 0.69f)
                        Main.tile[x, y].wall = WorldGen.genRand.NextBool(4) ? WallID.GrassUnsafe : WallID.FlowerUnsafe;
                }
            }

            // Place big vines.
            int bigVineCount = WorldGen.genRand.Next(20, 30);
            for (int i = 0; i < bigVineCount; i++)
            {
                int x = (int)placementCenter.X + WorldGen.genRand.Next(24, width - 24);
                int y = (int)placementCenter.Y + WorldGen.genRand.Next(18, height - 18);
                if (!GenerateVigorousVines(x, y, WorldGen.genRand.Next(3, 12)))
                {
                    i--;
                    continue;
                }
            }

            // Place small vines.
            int smallVineCount = WorldGen.genRand.Next(35, 50);
            for (int i = 0; i < smallVineCount; i++)
            {
                int x = (int)placementCenter.X + WorldGen.genRand.Next(24, width - 24);
                int y = (int)placementCenter.Y + WorldGen.genRand.Next(18, height - 18);
                if (CalamityUtils.ParanoidTileRetrieval(x, y).active() || !WorldGen.SolidTile(x, y - 1))
                {
                    i--;
                    continue;
                }

                int vineLength = WorldGen.genRand.Next(5, 12);
                bool neeedsToTryAgain = false;
                for (int dy = 0; dy < vineLength + 4; dy++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval(x, y + dy).active())
                    {
                        neeedsToTryAgain = true;
                        break;
                    }
                }

                if (neeedsToTryAgain)
                {
                    i--;
                    continue;
                }

                for (int dy = 0; dy < vineLength; dy++)
                {
                    Main.tile[x, y + dy].type = (ushort)ModContent.TileType<SmallVines>();
                    if (dy == vineLength - 1)
                    {
                        Main.tile[x, y + dy].frameX = (short)(WorldGen.genRand.Next(8) * 18);
                        Main.tile[x, y + dy].frameY = 72;
                    }
                    else
                    {
                        Main.tile[x, y + dy].frameX = (short)(WorldGen.genRand.Next(12) * 18);
                        Main.tile[x, y + dy].frameY = (short)(WorldGen.genRand.Next(4) * 18);
                    }
                    Main.tile[x, y + dy].active(true);
                    WorldGen.SquareTileFrame(x, y, true);
                }
            }

            // Place small flowers everywhere.
            int smallFlowerCount = WorldGen.genRand.Next(150, 200);
            for (int i = 0; i < smallFlowerCount; i++)
            {
                int x = (int)placementCenter.X + WorldGen.genRand.Next(16, width - 16);
                int y = (int)placementCenter.Y + WorldGen.genRand.Next(12, height - 12);
                if (CalamityUtils.ParanoidTileRetrieval(x, y).active() || !WorldGen.SolidTile(x, y + 1))
                {
                    i--;
                    continue;
                }

                Main.tile[x, y].type = (ushort)ModContent.TileType<FloralPlants>();
                Main.tile[x, y].frameX = (short)(Utils.SelectRandom(WorldGen.genRand, 7, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 27, 28, 29,
                    30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44) * 18);
                Main.tile[x, y].frameY = 0;
                Main.tile[x, y].active(true);
            }

            // Place ponds.
            int pondCount = WorldGen.genRand.Next(3);
            for (int i = 0; i < pondCount; i++)
            {
                int x = (int)placementCenter.X + WorldGen.genRand.Next(24, width - 24);
                int y = (int)placementCenter.Y + WorldGen.genRand.Next(24, height - 24);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                if (tile.active() || tile.liquid > 0)
                {
                    i--;
                    continue;
                }

                // Go upward once an open area is found. This is where water will be created and then settled.
                Point waterSpawnPoint = new Point(x, y);
                for (int dy = 0; dy < 100; dy++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval(x, y - dy).active())
                    {
                        waterSpawnPoint.Y -= dy - 1;
                        break;
                    }
                }

                int increment = 0;
                int waterCount = WorldGen.genRand.Next(180, 200);
                RecursivelyFillAreaWithWater(x, y, waterCount, ref increment);
            }

            // Place a blood orange. THIS IS FOR TESTING PURPOSES.
            for (int i = 0; i < 1; i++)
            {
                int x = (int)placementCenter.X + WorldGen.genRand.Next(24, width - 24);
                int y = (int)placementCenter.Y + WorldGen.genRand.Next(18, height - 18);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                if (!WorldGen.SolidTile(x, y + 1) || !WorldGen.SolidTile(x + 1, y + 1) || tile.active() || tile.liquid > 0)
                {
                    i--;
                    continue;
                }

                for (int dx = 0; dx < 3; dx++)
                {
                    for (int dy = 0; dy < 3; dy++)
                    {
                        Main.tile[x + dx, y + dy].type = (ushort)ModContent.TileType<BloodOrangePlant>();
                        Main.tile[x + dx, y + dy].frameX = (short)(dx * 18);
                        Main.tile[x + dx, y + dy].frameY = (short)(dy * 18);
                        Main.tile[x + dx, y + dy].active(true);
                    }
                }
            }
        }

        public static int CountCellsAtPosition(bool[,] originalMap, int x, int y, bool checkForActiveCells)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Ignore the center position.
                    if (dx == 0 && dy == 0)
                        continue;

                    // Ignore cells outside of the range of the map.
                    if (x + dx < 0 || x + dx >= originalMap.GetLength(0) || y + dy < 0 || y + dy >= originalMap.GetLength(1))
                        continue;

                    if ((originalMap[x + dx, y + dy] && checkForActiveCells) || (!originalMap[x + dx, y + dy] && !checkForActiveCells))
                        count++;
                }
            }
            return count;
        }

        public static bool[,] SimulateCelluarAutomata(bool[,] originalMap)
        {
            bool[,] newMap = (bool[,])originalMap.Clone();
            for (int x = 0; x < originalMap.GetLength(0); x++)
            {
                for (int y = 0; y < originalMap.GetLength(1); y++)
                {
                    if (originalMap[x, y] && CountCellsAtPosition(originalMap, x, y, true) < 4)
                        newMap[x, y] = false;
                    else if (!originalMap[x, y] && CountCellsAtPosition(originalMap, x, y, true) >= 5)
                        newMap[x, y] = true;
                }
            }
            return newMap;
        }

        public static bool GenerateVigorousVines(int x, int y, int length)
        {
            // Don't bother placing  vines if there's no solid ground above for them to hang from.
            if (!WorldGen.SolidTile(x, y - 1) || !WorldGen.SolidTile(x + 1, y - 1))
                return false;

            // Don't bother placing vines if there's anything obstructing their placement.
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < length * 2; dy++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval(x + dx, y + dy).active())
                        return false;
                }
            }

            int xFrame = 0;
            ushort vineID = (ushort)ModContent.TileType<VigorousVines>();
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < length * 2; dy++)
                {
                    int yFrame = dy >= length * 2 - 2 ? 36 : 0;
                    if (dx == 0 && dy % 2 == 0)
                        xFrame = WorldGen.genRand.NextBool(yFrame > 0 ? 2 : 5) ? 36 : 0;

                    Main.tile[x + dx, y + dy].active(true);
                    Main.tile[x + dx, y + dy].frameX = (short)(xFrame + dx * 18);
                    Main.tile[x + dx, y + dy].frameY = (short)(yFrame + dy % 2 * 18);
                    Main.tile[x + dx, y + dy].type = vineID;
                }
            }
            return true;
        }

        public static void RecursivelyFillAreaWithWater(int x, int y, int limit, ref int increment)
        {
            Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
            if (tile.active() || tile.liquid > 127)
                return;

            increment++;
            if (increment >= limit)
                return;

            Main.tile[x, y].liquid = 255;
            RecursivelyFillAreaWithWater(x + 1, y, limit, ref increment);
            RecursivelyFillAreaWithWater(x - 1, y, limit, ref increment);
            RecursivelyFillAreaWithWater(x, y + 1, limit, ref increment);
            RecursivelyFillAreaWithWater(x, y - 1, limit, ref increment);
        }
    }
}
