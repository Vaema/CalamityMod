using CalamityMod.Tiles;
using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Tiles.FloralParadise;
using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.World.Minibiomes
{
    public class FloralParadiseMinibiome
    {
        // Constants pertaining to the more important details of the world-gen, to allow for ease-of-change.
        // Certain numbers are hardcoded and not present here, but they tend to be in regards to things that either really shouldn't need changing or 
        // are extremely subtle details.
        public const int TotalCavesInMediumWorld = 3;

        public const int MaxHorizontalDistanceFromWorldCenter = 1250;

        public const int MinSurfaceDepth = 250;

        public const int MaxSurfaceDepth = 420;

        public const int BlacklistedTileCheckArea = 50;

        public const float CaveOpennessFactor = 0.08f;

        public const float WallOpennessFactor = 0.135f;

        public const int TotalSecondaryDirtCreationPasses = 1;

        public const float SecondaryPassDirtCreationChance = 0.5f;

        public const int MinVigorousVineLength = 6;

        public const int MaxVigorousVineLength = 16;

        public const int MinSmallVineLength = 9;

        public const int MaxSmallVineLength = 17;

        public const int MinSmallVineCount = 45;

        public const int MaxSmallVineCount = 64;

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
            int totalCaves = (int)(worldSize * TotalCavesInMediumWorld);
            int infiniteLoopPreventerIncrement = 0;
            List<Rectangle> existingCaveAreas = new List<Rectangle>();
            for (int i = 0; i < totalCaves - 1; i++)
            {
                // Use an emergency infinite loop increment.
                // The placement loop is left if it exceeds an extremely large quantity.
                // This exists primarily due to potential issues in small worlds.
                // This will likely never be hit in typical circumstances, but it's better to be safe than sorry.
                infiniteLoopPreventerIncrement++;
                if (infiniteLoopPreventerIncrement >= 3000)
                    break;

                int x = Main.maxTilesX / 2 + WorldGen.genRand.Next(-MaxHorizontalDistanceFromWorldCenter, MaxHorizontalDistanceFromWorldCenter);
                int y = (int)WorldGen.worldSurface + WorldGen.genRand.Next(MinSurfaceDepth, MaxSurfaceDepth);
                int width = (int)(WorldGen.genRand.Next(95, 110) * worldSize);
                int height = (int)(WorldGen.genRand.Next(80, 104) * worldSize);
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
                            needsToTryAgain = tile.HasTile && BlacklistedNearbyTiles.Contains(tile.TileType);
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

            // Generate a single, special living tree on the surface with a floral paradise biome underneath.
            for (int i = 0; i < 2000; i++)
            {
                int x = Main.maxTilesX / 2 + WorldGen.genRand.Next(330, MaxHorizontalDistanceFromWorldCenter) * WorldGen.genRand.NextBool().ToDirectionInt();
                int y = (int)WorldGen.worldSurface - 120;
                if (!FloralParadiseTree.Create(new(x, y)))
                    continue;
                break;
            }
        }

        public static void Place(Rectangle placementArea)
        {
            int seed = WorldGen.genRand.Next();
            int pondCount = WorldGen.genRand.Next(3);
            int smallVineCount = WorldGen.genRand.Next(MinSmallVineCount, MaxSmallVineCount);
            int waterTileCount = WorldGen.genRand.Next(MinPondWaterTileCount, MaxPondWaterTileCount);
            int smallFlowerCount = WorldGen.genRand.Next(150, 200);
            int bigTreeCount = WorldGen.genRand.Next(2, 5);

            CutOutCave(placementArea, seed, out bool[,] grassDirtMap);
            GenerateDirtBehindGrass(placementArea, grassDirtMap);
            GenerateWalls(placementArea, unchecked(seed - 8));
            GenerateVines(placementArea, smallVineCount);
            GeneratePonds(placementArea, waterTileCount, pondCount);
            CreateScenicPlants(placementArea, smallFlowerCount);
            CreateTrees(placementArea, bigTreeCount);

            // Place a blood orange. THIS IS FOR TESTING PURPOSES.
            for (int i = 0; i < 1; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(24, placementArea.Width - 24);
                int y = placementArea.Y + WorldGen.genRand.Next(18, placementArea.Height - 18);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                if (!WorldGen.SolidTile(x, y + 1) || !WorldGen.SolidTile(x + 1, y + 1) || tile.HasTile || tile.LiquidAmount > 0)
                {
                    i--;
                    continue;
                }
                x--;

                for (int dx = 0; dx < 3; dx++)
                {
                    for (int dy = 0; dy < 3; dy++)
                    {
                        Main.tile[x + dx, y + dy].TileType = (ushort)ModContent.TileType<BloodOrangePlant>();
                        Main.tile[x + dx, y + dy].TileFrameX = (short)(dx * 18);
                        Main.tile[x + dx, y + dy].TileFrameY = (short)(dy * 18);
                        Main.tile[x + dx, y + dy].Get<TileWallWireStateData>().HasTile = true;
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
            ushort grassID = (ushort)ModContent.TileType<PeatMoss>();
            ushort stoneID = (ushort)ModContent.TileType<AlgalSlate>();
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

                    float ditherChance = Utils.GetLerpValue(0.97f, 0.72f, distanceFromCenter, true);

                    Main.tile[x, y].WallType = WallID.None;
                    if (WorldGen.genRand.NextFloat() < ditherChance)
                    {
                        if (!caveState[arrayRelativeX, arrayRelativeY] && distanceFromCenter < 0.7f)
                            Main.tile[x, y].ClearEverything();
                        else
                        {
                            bool useGrass = CalamityUtils.CountCellsAtPosition(caveState, arrayRelativeX, arrayRelativeY, false) >= 1 && distanceFromCenter < 0.72f;
                            grassDirtMap[arrayRelativeX, arrayRelativeY] = useGrass;
                            Main.tile[x, y].ClearEverything();
                            Main.tile[x, y].ResetToType(useGrass ? grassID : stoneID);
                            WorldGen.SquareTileFrame(x, y);
                        }
                    }
                }
            }
        }

        public static void GenerateDirtBehindGrass(Rectangle placementArea, bool[,] grassDirtMap)
        {
            ushort dirtID = (ushort)ModContent.TileType<Peat>();
            ushort stoneID = (ushort)ModContent.TileType<AlgalSlate>();
            ushort grassID = (ushort)ModContent.TileType<PeatMoss>();

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
                        if (canPerformPlacement && tile.HasTile && tile.TileType == stoneID && nearbyGrassOrDirt)
                            Main.tile[x, y].TileType = dirtID;
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
                        grassDirtMap[arrayRelativeX, arrayRelativeY] = tile.TileType == grassID || tile.TileType == dirtID;
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
                        Main.tile[x, y].WallType = WorldGen.genRand.NextBool(4) ? WallID.GrassUnsafe : WallID.FlowerUnsafe;
                }
            }
        }

        public static void GenerateVines(Rectangle placementArea, int smallVineCount)
        {
            ushort[] vineIDs = new ushort[]
            {
                (ushort)ModContent.TileType<SmallVines>(),
                (ushort)ModContent.TileType<LushVines>(),
            };

            // Place small vines.
            for (int i = 0; i < smallVineCount; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(24, placementArea.Width - 24);
                int y = placementArea.Y + WorldGen.genRand.Next(18, placementArea.Height - 18);

                // Ensure that the initial tile is empty and that there's a solid tile above for the vine to hang from.
                // If these conditions are not met, try again.
                if (CalamityUtils.ParanoidTileRetrieval(x, y).HasTile || !WorldGen.SolidTile(x, y - 1))
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
                    if (CalamityUtils.ParanoidTileRetrieval(x, y + dy).HasTile)
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
                ushort vineID = WorldGen.genRand.Next(vineIDs);
                for (int dy = 0; dy < vineLength; dy++)
                {
                    Main.tile[x, y + dy].TileType = vineID;
                    if (dy == vineLength - 1)
                    {
                        Main.tile[x, y + dy].TileFrameX = (short)(WorldGen.genRand.Next(3) * 18);
                        Main.tile[x, y + dy].TileFrameY = 54;
                    }
                    else
                    {
                        Main.tile[x, y + dy].TileFrameX = (short)(WorldGen.genRand.Next(3) * 18);
                        Main.tile[x, y + dy].TileFrameY = (short)(WorldGen.genRand.Next(3) * 18);
                    }
                    Main.tile[x, y + dy].Get<TileWallWireStateData>().HasTile = true;
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
                if (tile.HasTile || tile.LiquidAmount > 0)
                {
                    i--;
                    continue;
                }

                // Go upward once an open area is found. This is where water will be created.
                Point waterSpawnPoint = new Point(x, y);
                for (int dy = 0; dy < 100; dy++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval(x, y - dy).HasTile)
                    {
                        waterSpawnPoint.Y -= dy - 1;
                        break;
                    }
                }

                // Create a predetermined quantity of water via recursion.
                // This starts at the initial water spawn point and the expands outward in all four cardinal directions repeatedly until either no more locations
                // are valid or the water limit has been reached.
                int increment = 0;
                RecursivelyFillAreaWithWater(x, y, waterTileCount, ref increment);

                // Settle water and create waterfalls.
                Liquid.QuickWater(3);
                WorldGen.WaterCheck();
                Liquid.quickSettle = true;
                Liquid.UpdateLiquid();
                WorldGen.WaterCheck();
                Liquid.quickSettle = false;

                for (int dy = 0; dy < 50; dy++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval(x, y + dy).LiquidAmount > 0)
                    {
                        Main.tile[x, y + dy - 8].Get<TileWallWireStateData>().HasTile = true;
                        Main.tile[x, y + dy - 8].TileType = (ushort)ModContent.TileType<WaterfallCreator>();
                        break;
                    }
                }
            }
        }

        public static void CreateScenicPlants(Rectangle placementArea, int smallFlowerCount)
        {
            for (int i = 0; i < smallFlowerCount; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(16, placementArea.Width - 16);
                int y = placementArea.Y + WorldGen.genRand.Next(12, placementArea.Height - 12);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                if (tile.HasTile || tile.LiquidAmount > 0 || !WorldGen.SolidTile(x, y + 1))
                {
                    i--;
                    continue;
                }

                Main.tile[x, y].TileType = (ushort)ModContent.TileType<FloralPlants>();
                Main.tile[x, y].TileFrameX = (short)(Utils.SelectRandom(WorldGen.genRand, 7, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 27, 28, 29,
                    30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44) * 18);
                Main.tile[x, y].TileFrameY = 0;
                Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            }

            // TODO -- Move this to another method.
            for (int i = 0; i < smallFlowerCount / 2; i++)
            {
                int x = placementArea.X + WorldGen.genRand.Next(16, placementArea.Width - 16);
                int y = placementArea.Y + WorldGen.genRand.Next(12, placementArea.Height - 12);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                if (tile.HasTile || tile.LiquidAmount > 0 || !WorldGen.SolidTile(x, y + 1))
                {
                    i--;
                    continue;
                }

                WorldGen.PlaceTile(x, y, ModContent.TileType<LargeMossPile>());
            }
        }

        public static void CreateTrees(Rectangle placementArea, int treeCount)
        {
            int tries = 0;
            int treeID = ModContent.TileType<PerennialTree>();

            for (int i = 0; i < treeCount; i++)
            {
                tries++;

                int x = placementArea.X + WorldGen.genRand.Next(16, placementArea.Width - 16);
                int y = placementArea.Y + WorldGen.genRand.Next(12, placementArea.Height - 12);
                Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                Vector2 checkTopLeft = new Vector2(x, y).ToWorldCoordinates() - new Vector2(120f, 300f);
                Vector2 checkArea = new Vector2(240f, 200f);

                if (tries >= 2500)
                    break;

                if (tile.HasTile || tile.LiquidAmount > 0 || !WorldGen.SolidTile(x, y + 1) || 
                    !TileObject.CanPlace(x, y, treeID, 0, 0, out _, true, true) ||
                    Collision.SolidCollision(checkTopLeft, (int)checkArea.X, (int)checkArea.Y))
                {
                    i--;
                    continue;
                }

                WorldGen.PlaceTile(x, y, treeID);
            }
        }

        #region Biome-Specific Utilities

        public static void RecursivelyFillAreaWithWater(int x, int y, int limit, ref int increment)
        {
            Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
            if (tile.HasTile || tile.LiquidAmount > 127)
                return;

            increment++;
            if (increment >= limit)
                return;

            Main.tile[x, y].LiquidAmount = 255;
            RecursivelyFillAreaWithWater(x + 1, y, limit, ref increment);
            RecursivelyFillAreaWithWater(x - 1, y, limit, ref increment);
            RecursivelyFillAreaWithWater(x, y + 1, limit, ref increment);
            RecursivelyFillAreaWithWater(x, y - 1, limit, ref increment);
        }

        #endregion Biome-Specific Utilities
    }
}
