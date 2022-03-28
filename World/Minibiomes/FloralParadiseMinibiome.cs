using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Tiles.FloralParadise;
using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.World.Minibiomes
{
    public class FloralParadiseMinibiome
    {
        // Constants pertaining to the more important details of the world-gen, to allow for ease-of-change.
        // Certain numbers are hardcoded and not present here, but they tend to be in regards to things that either really shouldn't need changing or 
        // are extremely subtle details.

        public const int MaxHorizontalDistanceFromWorldCenter = 1250;

        public const int MinSurfaceDepth = 250;

        public const int MaxSurfaceDepth = 420;

        public const int BlacklistedTileCheckArea = 50;

        public const float CaveOpennessFactor = 0.08f;

        public const float WallOpennessFactor = 0.135f;

        public const int TotalSecondaryDirtCreationPasses = 1;

        public const float SecondaryPassDirtCreationChance = 0.5f;

        public const int MinVigorousVineLength = 4;

        public const int MaxVigorousVineLength = 12;

        public const int MinSmallVineLength = 9;

        public const int MaxSmallVineLength = 17;

        public const int MinVigorousVineCount = 20;

        public const int MaxVigorousVineCount = 30;

        public const int MinSmallVineCount = 35;

        public const int MaxSmallVineCount = 50;

        public const int MinPondWaterTileCount = 180;

        public const int MaxPondWaterTileCount = 210;

        public static int[] BlacklistedNearbyTiles => new int[]
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

        public static void GenerateInstances()
        {
            float worldSize = Main.maxTilesX / 4200f;
            int totalCaves = (int)(worldSize * 5f);
            int infiniteLoopPreventerIncrement = 0;
            List<Rectangle> existingCaveAreas = new List<Rectangle>();
            for (int i = 0; i < totalCaves; i++)
            {
                // Use an emergency infinite loop increment.
                // The placement loop is left if it exceeds an extremely large quantity.
                // This exists primarily due to potential issues in small worlds.
                // This will likely never be hit in typical circumstances, but it's better to be safe than sorry.
                infiniteLoopPreventerIncrement++;
                if (infiniteLoopPreventerIncrement >= 1000)
                    break;

                int x = Main.maxTilesX / 2 + WorldGen.genRand.Next(-MaxHorizontalDistanceFromWorldCenter, MaxHorizontalDistanceFromWorldCenter);
                int y = (int)WorldGen.worldSurface + WorldGen.genRand.Next(MinSurfaceDepth, MaxSurfaceDepth);
                int width = (int)(WorldGen.genRand.Next(80, 100) * worldSize);
                int height = (int)(WorldGen.genRand.Next(70, 95) * worldSize);
                Rectangle caveArea = Utils.CenteredRectangle(new Vector2(x, y), new Vector2(width, height));

                // Try again if this selected location is near blacklisted tiles.
                // This is done to prevent logical consistencies/intrusion on unrelated biomes.
                bool needsToTryAgain = false;
                for (int dx = -BlacklistedTileCheckArea; dx < BlacklistedTileCheckArea; dx++)
                {
                    for (int dy = -BlacklistedTileCheckArea; dy < BlacklistedTileCheckArea; dy++)
                    {
                        if (!needsToTryAgain)
                        {
                            Tile tile = CalamityUtils.ParanoidTileRetrieval(x + dx, y + dy);
                            needsToTryAgain = tile.active() && BlacklistedNearbyTiles.Contains(tile.type);
                        }
                        else
                            break;
                    }
                }

                // If the blacklisted tile check was passed, check to see if the potential spot is near.
                if (!needsToTryAgain)
                    needsToTryAgain = existingCaveAreas.Any(c => caveArea.Intersects(c));

                if (needsToTryAgain)
                {
                    i--;
                    continue;
                }

                Place(caveArea);
                existingCaveAreas.Add(caveArea);
            }
        }

        public static void Place(Rectangle placementArea)
        {
            int seed = WorldGen.genRand.Next();
            int pondCount = WorldGen.genRand.Next(3);
            int vigorousVineCount = WorldGen.genRand.Next(MinVigorousVineCount, MaxVigorousVineCount);
            int smallVineCount = WorldGen.genRand.Next(MinSmallVineCount, MaxSmallVineCount);
            int waterTileCount = WorldGen.genRand.Next(MinPondWaterTileCount, MaxPondWaterTileCount);
            int smallFlowerCount = WorldGen.genRand.Next(150, 200);

            CutOutCave(placementArea, seed, out bool[,] grassDirtMap);
            GenerateDirtBehindGrass(placementArea, grassDirtMap);
            GenerateWalls(placementArea, unchecked(seed - 8));
            GenerateVines(placementArea, vigorousVineCount, smallVineCount);
            GeneratePonds(placementArea, waterTileCount, pondCount);
            CreateScenicPlants(placementArea, smallFlowerCount);

            // Place a blood orange. THIS IS FOR TESTING PURPOSES.
            for (int i = 0; i < 1; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(24, placementArea.Width - 24);
                int y = placementArea.Y + WorldGen.genRand.Next(18, placementArea.Height - 18);
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

        public static void CutOutCave(Rectangle placementArea, int seed, out bool[,] grassDirtMap)
        {
            // Calculate the initial cell states with perlin noise.
            // To be specific, it uses values near the midpoint of 0.5 as open caves.
            // This helps solve the problem of having disconnected areas that using min/max threshold values causes.
            bool[,] caveState = new bool[placementArea.Width, placementArea.Height];
            for (int i = 0; i < placementArea.Width; i++)
            {
                for (int j = 0; j < placementArea.Height; j++)
                {
                    float noise = CalamityUtils.PerlinNoise2D(i / 180f, j / 180f, 3, seed) * 0.5f + 0.5f;
                    caveState[i, j] = MathHelper.Distance(noise, 0.56f) < CaveOpennessFactor;
                    if (WorldGen.genRand.NextBool(3))
                        caveState[i, j] = WorldGen.genRand.NextBool();
                }
            }

            // Repeatedly smoothen out the cave state with celluar automata.
            for (int i = 0; i < 8; i++)
                caveState = CalamityUtils.SimulateCelluarAutomata(caveState);

            // Reset tile states based on the above results.
            ushort grassID = (ushort)ModContent.TileType<FloralGrass>();
            ushort stoneID = (ushort)ModContent.TileType<FloralStone>();
            grassDirtMap = new bool[placementArea.Width, placementArea.Height];
            for (int x = placementArea.X; x < placementArea.X + placementArea.Width; x++)
            {
                for (int y = placementArea.Y; y < placementArea.Y + placementArea.Height; y++)
                {
                    // Don't attempt to change tile data if a tile is outside of the world for some reason.
                    if (!WorldGen.InWorld(x, y, 1))
                        continue;

                    int arrayRelativeX = x - placementArea.X;
                    int arrayRelativeY = y - placementArea.Y;
                    Vector2 normalizedPosition = new Vector2(arrayRelativeX / (float)placementArea.Width, arrayRelativeY / (float)placementArea.Height);
                    float distanceFromCenter = Vector2.Distance(normalizedPosition, Vector2.One * 0.5f) * 1.75f;
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
                            bool useGrass = CalamityUtils.CountCellsAtPosition(caveState, arrayRelativeX, arrayRelativeY, false) >= 1 && distanceFromCenter < 0.72f;
                            grassDirtMap[arrayRelativeX, arrayRelativeY] = useGrass;
                            Main.tile[x, y] = new Tile();
                            Main.tile[x, y].ResetToType(useGrass ? grassID : stoneID);
                            WorldGen.SquareTileFrame(x, y);
                        }
                    }
                }
            }
        }

        public static void GenerateDirtBehindGrass(Rectangle placementArea, bool[,] grassDirtMap)
        {
            ushort dirtID = (ushort)ModContent.TileType<PeteMoss>();
            ushort stoneID = (ushort)ModContent.TileType<FloralStone>();
            ushort grassID = (ushort)ModContent.TileType<FloralGrass>();

            // Perform secondary passes that overlay dirt behind grass.
            for (int i = 0; i < TotalSecondaryDirtCreationPasses + 1; i++)
            {
                for (int x = placementArea.X; x < placementArea.X + placementArea.Width; x++)
                {
                    for (int y = placementArea.Y; y < placementArea.Y + placementArea.Height; y++)
                    {
                        int arrayRelativeX = x - placementArea.X;
                        int arrayRelativeY = y - placementArea.Y;
                        bool nearbyGrassOrDirt = false;
                        if (arrayRelativeX - 1 >= 0 && grassDirtMap[arrayRelativeX - 1, arrayRelativeY])
                            nearbyGrassOrDirt = true;
                        else if (arrayRelativeX + 1 < placementArea.Width && grassDirtMap[arrayRelativeX + 1, arrayRelativeY])
                            nearbyGrassOrDirt = true;
                        else if (arrayRelativeY - 1 >= 0 && grassDirtMap[arrayRelativeX, arrayRelativeY - 1])
                            nearbyGrassOrDirt = true;
                        else if (arrayRelativeY + 1 < placementArea.Height && grassDirtMap[arrayRelativeX, arrayRelativeY + 1])
                            nearbyGrassOrDirt = true;

                        // The first pass always generates dirt behind grass.
                        // However, successive passes only generate dirt with a specific probability.
                        // This makes the dirt feel more natural and connected to the underlying rock.
                        bool canPerformPlacement = i == 0 || WorldGen.genRand.NextFloat() < SecondaryPassDirtCreationChance;
                        Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                        if (canPerformPlacement && tile.active() && tile.type == stoneID && nearbyGrassOrDirt)
                            Main.tile[x, y].type = dirtID;
                    }
                }

                // Reset the dirt/grass map for the next pass.
                // This is skipped on the last pass for performance reasons, since the map won't be used after that point.
                if (i >= TotalSecondaryDirtCreationPasses)
                    continue;

                for (int x = placementArea.X; x < placementArea.X + placementArea.Width; x++)
                {
                    for (int y = placementArea.Y; y < placementArea.Y + placementArea.Height; y++)
                    {
                        Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                        int arrayRelativeX = x - placementArea.X;
                        int arrayRelativeY = y - placementArea.Y;
                        grassDirtMap[arrayRelativeX, arrayRelativeY] = tile.type == grassID || tile.type == dirtID;
                    }
                }
            }
        }

        public static void GenerateWalls(Rectangle placementArea, int seed)
        {
            for (int x = placementArea.X; x < placementArea.X + placementArea.Width; x++)
            {
                for (int y = placementArea.Y; y < placementArea.Y + placementArea.Height; y++)
                {
                    float noise = CalamityUtils.PerlinNoise2D(x / 160f, y / 160f, 3, seed) * 0.5f + 0.5f;
                    if (MathHelper.Distance(noise, 0.555f) < WallOpennessFactor)
                        Main.tile[x, y].wall = WorldGen.genRand.NextBool(4) ? WallID.GrassUnsafe : WallID.FlowerUnsafe;
                }
            }
        }

        public static void GenerateVines(Rectangle placementArea, int vigorousVineCount, int smallVineCount)
        {
            // Place big vines.
            int infiniteLoopPreventerIncrement = 0;
            for (int i = 0; i < vigorousVineCount; i++)
            {
                infiniteLoopPreventerIncrement++;
                if (infiniteLoopPreventerIncrement >= 500)
                    break;

                int x = placementArea.X + WorldGen.genRand.Next(24, placementArea.Width - 24);
                int y = placementArea.Y + WorldGen.genRand.Next(18, placementArea.Height - 18);
                if (!GenerateVigorousVine(x, y, WorldGen.genRand.Next(MinVigorousVineLength, MaxVigorousVineLength + 1)))
                {
                    i--;
                    continue;
                }
                infiniteLoopPreventerIncrement = 0;
            }

            // Place small vines.
            for (int i = 0; i < smallVineCount; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(24, placementArea.Width - 24);
                int y = placementArea.Y + WorldGen.genRand.Next(18, placementArea.Height - 18);

                // Ensure that the initial tile is empty and that there's a solid tile above for the vine to hang from.
                // If these conditions are not met, try again.
                if (CalamityUtils.ParanoidTileRetrieval(x, y).active() || !WorldGen.SolidTile(x, y - 1))
                {
                    i--;
                    continue;
                }

                // Search to ensure that nothing is in the way of the vine's potential positions.
                // If something is, try again.
                int vineLength = WorldGen.genRand.Next(MinSmallVineLength, MaxSmallVineLength + 1);
                bool neeedsToTryAgain = false;
                for (int dy = 0; dy < vineLength; dy++)
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

                // Perform placement.
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
        }

        public static void GeneratePonds(Rectangle placementArea, int waterTileCount, int pondCount)
        {
            for (int i = 0; i < pondCount; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(24, placementArea.Width - 24);
                int y = placementArea.Y + WorldGen.genRand.Next(24, placementArea.Height - 24);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                if (tile.active() || tile.liquid > 0)
                {
                    i--;
                    continue;
                }

                // Go upward once an open area is found. This is where water will be created.
                Point waterSpawnPoint = new Point(x, y);
                for (int dy = 0; dy < 100; dy++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval(x, y - dy).active())
                    {
                        waterSpawnPoint.Y -= dy - 1;
                        break;
                    }
                }

                // Create a predetermined quantity of water via recursion.
                // This starts at the initial water spawn point and the expands outward in all four cardinal directions repeatedly until either no more locations
                // are valid or the water limit has been reached.
                // This water will be settled to create ponds via extraneous code later in the world generation pipeline.
                int increment = 0;
                RecursivelyFillAreaWithWater(x, y, waterTileCount, ref increment);
            }
        }

        public static void CreateScenicPlants(Rectangle placementArea, int smallFlowerCount)
        {
            for (int i = 0; i < smallFlowerCount; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(16, placementArea.Width - 16);
                int y = placementArea.Y + WorldGen.genRand.Next(12, placementArea.Height - 12);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                if (tile.active() || tile.liquid > 0 || !WorldGen.SolidTile(x, y + 1))
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
        }

        #region Biome-Specific Utilities

        public static bool GenerateVigorousVine(int x, int y, int length)
        {
            // Don't bother placing vines if there's no solid ground above for them to hang from.
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

        #endregion Biome-Specific Utilities
    }
}
