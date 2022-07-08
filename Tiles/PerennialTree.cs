using System;
using CalamityMod.DataStructures;
using CalamityMod.ILEditing;
using CalamityMod.Tiles.BaseTiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles
{
    public class PerennialTree : DynamicallyGrownTree
    {
        // The max amount that branches can travel. Once the distance of the branches reaches or exceeds this threshold, the tree is done growing.
        public override float MaxDistanceBeforeCutoff => 2100f;

        public override float DistanceUsedForTrunk => 140f;

        public override float BranchMaxBendFactor => 0.7f;

        public override float BranchTurnAngleVariance => 0.4f;

        public override float MinBranchLength => 30f;

        public override float TrunkWidth => 16f;

        public override float ChanceToCreateNewBranches => 0.8f;

        public override float VerticalStretchFactor => 12f;

        public override float DownwardBiasFactor => 0.4f;

        public override float BranchGrowthWidthDecay => 0.61f;

        public override int MaxCutoffBranchesPerBranch => 5;

        public override void SetStaticDefaults()
        {
            Main.tileAxe[Type] = true;
            Main.tileFrameImportant[Type] = true;
            DustType = 7;
            ItemDrop = ItemID.Wood;
            UseDefaultSize();
            TileObjectData.addTile(Type);

            AddMapEntry(Color.SaddleBrown);
        }

        public void DrawVine(Vector2 vineTop, Vector2 downwardBottom, int totalVinesToDraw, float swayFactor)
        {
            Texture2D vineTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/PerennialTreeVines").Value;

            // Determine the initial points for the vines.
            Vector2[] controlPoints = new Vector2[8];
            Vector2 lightOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(-Main.offScreenRange);
            for (int i = 0; i < controlPoints.Length; i++)
            {
                float swayOffset = swayFactor;
                controlPoints[i] = Vector2.Lerp(vineTop, downwardBottom, i / (float)(controlPoints.Length - 1f));

                Point p = (controlPoints[i] + lightOffset + PreviousPoint.ToWorldCoordinates()).ToTileCoordinates();
                ILChanges.Windgrid.GetWindTime(p.X, p.Y, 40, out int windTimeLeft, out int direction);
                float windInterpolant = windTimeLeft / 40f;
                swayOffset += Utils.GetLerpValue(0f, 0.45f, windInterpolant, true) * Utils.GetLerpValue(1f, 0.55f, windInterpolant, true) * direction * 28f;
                controlPoints[i].X += swayOffset * Utils.GetLerpValue(0f, controlPoints.Length - 1f, i, true);
                controlPoints[i].Y -= 4f;
            }
            BezierCurve vineCurve = new(controlPoints);

            int drawCount = (int)(totalVinesToDraw * 2.4f);
            for (int i = 0; i < drawCount - 1; i++)
            {
                Vector2 vineCenter = vineCurve.Evaluate(i / (float)(drawCount - 1f));
                Vector2 ahead = vineCurve.Evaluate((i + 1f) / (float)(drawCount - 1f));
                float rotation = (ahead - vineCenter).ToRotation() - MathHelper.PiOver2;
                vineCenter += PreviousPoint.ToWorldCoordinates() - Main.screenPosition;

                // Calculate light values.
                Vector2 lightPosition = vineCenter + lightOffset + Main.screenPosition;

                int frameX = 0;
                int frameY = 0;
                if (i > 0)
                    frameY = RNG.Next(1, 3);
                if (i >= drawCount - 3)
                {
                    frameY = 3;
                    frameX = RNG.Next(3);
                }
                if (i == drawCount - 2)
                    frameY = 4;

                Rectangle frame = vineTexture.Frame(3, 5, frameX, frameY, -2, -2);
                Color color = Lighting.GetColor(lightPosition.ToTileCoordinates());

                Main.spriteBatch.Draw(vineTexture, vineCenter, frame, color, rotation, frame.Size() * 0.5f, 1f, 0f, 0);
            }
        }

        public override void DrawThingAtEndOfBranch(Branch branch)
        {
            int totalVinesToDraw = RNG.Next(4, 7);
            float swayFactor = Main.windSpeedCurrent + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.87f + RNG.NextFloat(0.94f)) * 0.4f;
            Vector2 top = branch.EndOfCurve + Vector2.UnitY * 12f;
            Vector2 bottom = top + Vector2.UnitY * totalVinesToDraw * 30f;
            DrawVine(top, bottom, totalVinesToDraw, swayFactor * 35f);
        }
    }
}
