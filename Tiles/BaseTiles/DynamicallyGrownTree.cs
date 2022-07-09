using Terraria;
using Terraria.ModLoader;
using System.Linq;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;
using ReLogic.Content;
using Terraria.Utilities;

namespace CalamityMod.Tiles.BaseTiles
{
    public abstract class DynamicallyGrownTree : ModTile
    {
        public BranchDrawer BranchDrawer
        {
            get;
            internal set;
        } = null;

        public abstract float MaxDistanceBeforeCutoff { get; }

        public abstract float DistanceUsedForTrunk { get; }

        public abstract float BranchMaxBendFactor { get; }

        public abstract float BranchTurnAngleVariance { get; }

        public abstract float MinBranchLength { get; }

        public abstract float TrunkWidth { get; }

        public abstract float ChanceToCreateNewBranches { get; }

        public abstract float VerticalStretchFactor { get; }

        public abstract float DownwardBiasFactor { get; }

        public abstract float BranchGrowthWidthDecay { get; }

        public abstract int MaxCutoffBranchesPerBranch { get; }

        public const int ControlPointCountPerBranch = 8;

        public UnifiedRandom RNG => BranchDrawer.RNG;

        public override void Load()
        {
            BranchDrawer = new()
            {
                MaxDistanceBeforeCutoff = MaxDistanceBeforeCutoff,
                DistanceUsedForTrunk = DistanceUsedForTrunk,
                BranchMaxBendFactor = BranchMaxBendFactor,
                BranchTurnAngleVariance = BranchTurnAngleVariance,
                MinBranchLength = MinBranchLength,
                TrunkWidth = TrunkWidth,
                ChanceToCreateNewBranches = ChanceToCreateNewBranches,
                VerticalStretchFactor = VerticalStretchFactor,
                DownwardBiasFactor = DownwardBiasFactor,
                BranchGrowthWidthDecay = BranchGrowthWidthDecay,
                MaxCutoffBranchesPerBranch = MaxCutoffBranchesPerBranch
            };

            if (Main.netMode != NetmodeID.Server)
                BranchDrawer.BarkTexture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
        }

        public virtual void DrawThingAtEndOfBranch(BranchDrawer.Branch branch) { }

        public void Draw(Point p)
        {
            // This is necessary to ensure that the primitives properly render.
            TileID.Sets.DrawTileInSolidLayer[Type] = true;
            BranchDrawer.Draw(p);

            // Determine branch data.
            var branchData = BranchDrawer.GenerateBranches(p);
            var branches = branchData.Select(b => b.Key);
            var outwardmostBranches = branchData.Where(b => b.Value.Count <= 0f).Select(b => b.Key);

            // Draw things at the end of branches.
            foreach (BranchDrawer.Branch outwardmostBranch in outwardmostBranches)
                DrawThingAtEndOfBranch(outwardmostBranch);
        }

        public void UseDefaultSize()
        {
            TileObjectData.newTile.Width = (int)Math.Ceiling(TrunkWidth / 16);
            TileObjectData.newTile.Height = (int)Math.Ceiling(DistanceUsedForTrunk / 16);
            TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2, TileObjectData.newTile.Height - 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = 2;
        }

        public override bool Drop(int i, int j) => false;

        public override void KillMultiTile(int x, int y, int frameX, int frameY)
        {
            var branchData = BranchDrawer.GenerateBranches(new(x, y));
            var branches = branchData.Select(b => b.Key);
            foreach (var branch in branches)
            {
                int totalWood = (int)Math.Ceiling(branch.CurveLength / 56f);
                int totalDust = totalWood * 3;
                int stack = Main.rand.Next(1, 3);
                for (int i = 0; i < totalWood; i++)
                {
                    Vector2 woodPosition = branch.Curve.Evaluate(i / (float)(totalWood - 1f));
                    woodPosition += new Vector2(x, y).ToWorldCoordinates() + Main.rand.NextVector2Circular(10f, 10f);
                    woodPosition.Y += DistanceUsedForTrunk;
                    Item.NewItem(new EntitySource_TileBreak(x, y), woodPosition, ItemDrop);
                }

                // Create dust.
                for (int i = 0; i < totalDust; i++)
                {
                    Vector2 dustPosition = branch.Curve.Evaluate(i / (float)(totalDust - 1f));
                    dustPosition += new Vector2(x, y).ToWorldCoordinates() + Main.rand.NextVector2Circular(5f, 5f);
                    dustPosition.Y += DistanceUsedForTrunk;
                    Dust.NewDustDirect(dustPosition, 4, 4, DustType);
                }
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = CalamityUtils.ParanoidTileRetrieval(i, j);
            if (t.TileFrameX != 0 || t.TileFrameY != ((int)Math.Ceiling(DistanceUsedForTrunk / 16) - 1) * 18)
                return false;

            Vector2 screenOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Draw(new(i + (int)screenOffset.X / 16, j + (int)screenOffset.Y / 16));
            return false;
        }
    }
}
