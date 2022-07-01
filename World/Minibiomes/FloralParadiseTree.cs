using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace CalamityMod.World.Minibiomes
{
    public class FloralParadiseTree
    {
        public const int TrunkHeight = 220;

        public const int TrunkWidth = 10;

        public const int TrunkHollowSpace = 4;

        public const int RootCount = 5;

        public static bool Create(Point placementPosition)
        {
            float worldSize = Main.maxTilesX / 4200f;
            if (!WorldUtils.Find(new Point(placementPosition.X - 3, placementPosition.Y), Searches.Chain(new Searches.Down(400), new GenCondition[]
            {
                new Conditions.IsSolid().AreaAnd(6, 1)
            }), out Point ground))
            {
                return false;
            }

            ground.Y += (int)(TrunkHeight * 0.645f);

            // Get all tiles in a specific area.
            Dictionary<ushort, int> tileCounts = new Dictionary<ushort, int>();
            WorldUtils.Gen(new Point(ground.X - TrunkHeight / 2, ground.Y - TrunkHeight / 2), new Shapes.Rectangle(TrunkHeight, TrunkHeight), new Actions.TileScanner(new ushort[]
            {
                TileID.Dirt,
                TileID.Stone,
                TileID.Mud,
                TileID.JungleGrass,
                TileID.SnowBlock,
                TileID.Sand,
            }).Output(tileCounts));

            int overworldTileCount = tileCounts[TileID.Dirt] + tileCounts[TileID.Stone];
            int jungleTileCount = tileCounts[TileID.Mud] + tileCounts[TileID.JungleGrass];

            // Avoid jungles, deserts, and snow biomes.
            if (tileCounts[TileID.SnowBlock] > jungleTileCount || jungleTileCount > overworldTileCount || overworldTileCount < 50 || tileCounts[TileID.Sand] >= 50)
                return false;

            float trunkRotation = WorldGen.genRand.NextFloatDirection() * 0.116f;
            for (int i = 0; i < TrunkHeight; i += 2)
            {
                Point currentPoint = (ground.ToVector2() - Vector2.UnitY.RotatedBy(trunkRotation) * i).ToPoint();

                // Create the trunk.
                WorldUtils.Gen(currentPoint, new Shapes.Rectangle(TrunkWidth, TrunkWidth), Actions.Chain(new GenAction[]
                {
                    new Actions.ClearTile(),
                    new Actions.PlaceWall(WallID.LivingWood, true),
                    new Actions.PlaceTile(TileID.LivingMahogany)
                }));
            }

            // Generate roots.
            for (int i = 0; i < RootCount; i++)
            {
                int rootLength = (int)(TrunkHeight * WorldGen.genRand.NextFloat(0.425f, 0.7f));
                float rootAngle = MathHelper.Lerp(-0.89f, 0.89f, i / (float)(RootCount - 1f)) + WorldGen.genRand.NextFloatDirection() * 0.074f + trunkRotation + MathHelper.PiOver2;
                WorldUtils.Gen(ground, new ShapeRoot(rootAngle, rootLength, TrunkWidth - 1), Actions.Chain(new GenAction[]
                {
                    new Actions.SetTile(TileID.LivingMahogany),
                }));
            }

            List<Point> branchEnds = new();
            Point topOfTrunk = (ground.ToVector2() - Vector2.UnitY.RotatedBy(trunkRotation) * (TrunkHeight - 4f)).ToPoint();

            // Create side branches.
            for (int i = 0; i < RootCount - 1; i++)
            {
                int branchLength = WorldGen.genRand.Next(26, 49);
                float branchDirectionX = -WorldGen.genRand.NextFloat(1.25f, 4.5f) * WorldGen.genRand.NextBool().ToDirectionInt();
                float branchDirectionY = -2f;
                Vector2 branchDirection = new Vector2(branchDirectionX, branchDirectionY).SafeNormalize(Vector2.UnitX);
                Point branchStart = (ground.ToVector2() - Vector2.UnitY.RotatedBy(trunkRotation) * Main.rand.NextFloat(0.45f, 0.84f) * TrunkHeight).ToPoint();
                WorldUtils.Gen(branchStart, new ShapeBranch(branchDirection.ToRotation(), branchLength).OutputEndpoints(branchEnds), Actions.Chain(new GenAction[]
                {
                    new Actions.SetTile(TileID.LivingMahogany),
                }));
            }

            // Create branch leaves.
            using (List<Point>.Enumerator enumerator = branchEnds.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    WorldUtils.Gen(enumerator.Current, new Shapes.Circle(4), Actions.Chain(new GenAction[]
                    {
                        new Modifiers.Blotches(4, 2, 0.3),
                        new Actions.SetTile(TileID.LivingMahoganyLeaves),
                        new Actions.SetFrames(true)
                    }));
                }
            }

            // Create leaves at the top of the trunk.
            WorldUtils.Gen(topOfTrunk, new Shapes.Circle(TrunkWidth * 2), Actions.Chain(new GenAction[]
            {
                new Modifiers.Blotches(3, 2, 0.1),
                new Actions.SetTile(TileID.LivingMahoganyLeaves),
                new Actions.SetFrames(true)
            }));

            // Generate a cave in the roots of the biome.
            int width = (int)(WorldGen.genRand.Next(104, 120) * worldSize);
            int height = (int)(WorldGen.genRand.Next(88, 110) * worldSize);
            Vector2 cavePosition = ground.ToVector2() + Vector2.UnitY * height * 0.44f;
            Rectangle caveArea = Utils.CenteredRectangle(cavePosition, new Vector2(width, height));
            FloralParadiseMinibiome.Place(caveArea);

            // Clear out the inner part of the tiles to create an open space in the tree.
            for (int i = 0; i < TrunkHeight * 0.9f; i += 2)
            {
                Point currentPoint = (ground.ToVector2() - Vector2.UnitY.RotatedBy(trunkRotation) * i).ToPoint();
                Point center = (currentPoint.ToVector2() + Vector2.One * TrunkHollowSpace - Vector2.UnitX * 2f).ToPoint();
                WorldUtils.Gen(center, new Shapes.Rectangle(TrunkHollowSpace, TrunkHollowSpace), Actions.Chain(new GenAction[]
                {
                    new Actions.ClearTile(true),
                    new Actions.PlaceWall(WallID.LivingWood, true)
                }));
            }

            return true;
        }
    }
}
