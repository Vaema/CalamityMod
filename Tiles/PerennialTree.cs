using System;
using CalamityMod.DataStructures;
using CalamityMod.Tiles.BaseTiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public class PerennialTree : DynamicallyGrownTree
    {
        // The max amount that branches can travel. Once the distance of the branches reaches or exceeds this threshold, the tree is done growing.
        public override float MaxDistanceBeforeCutoff => 4200f;

        public override float DistanceUsedForTrunk => 225f;

        public override float BranchMaxBendFactor => 0.75f;

        public override float BranchTurnAngleVariance => 0.48f;

        public override float MinBranchLength => 30f;

        public override float TrunkWidth => 16f;

        public override float ChanceToCreateNewBranches => 0.8f;

        public override float VerticalStretchFactor => 10f;

        public override float DownwardBiasFactor => 0.75f;

        public override int MaxCutoffBranchesPerBranch => 6;

        public override void SetStaticDefaults()
        {
            DustType = 214;
            ItemDrop = ItemID.Wood;

            AddMapEntry(new Color(83, 91, 102));
        }

        public void DrawVine(Vector2 vineTop, Vector2 downwardBottom, int totalVinesToDraw, float swayFactor)
        {
            Texture2D vineTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/PerennialTreeVines").Value;

            // Determine the initial points for the vines.
            Vector2[] controlPoints = new Vector2[8];
            for (int i = 0; i < controlPoints.Length; i++)
            {
                controlPoints[i] = Vector2.Lerp(vineTop, downwardBottom, i / (float)(controlPoints.Length - 1f));
                controlPoints[i].X += swayFactor * Utils.GetLerpValue(0f, controlPoints.Length - 1f, i, true);
            }
            BezierCurve vineCurve = new(controlPoints);

            int drawCount = (int)(totalVinesToDraw * 1.25f);
            for (int i = 0; i < drawCount - 1; i++)
            {
                bool useFruitFrame = RNG.NextBool(25);
                Vector2 vineCenter = vineCurve.Evaluate(i / (float)(drawCount - 1f));
                Vector2 ahead = vineCurve.Evaluate((i + 1f) / (float)(drawCount - 1f));
                float rotation = (ahead - vineCenter).ToRotation() + MathHelper.PiOver2;
                vineCenter += PreviousPoint.ToWorldCoordinates() - Main.screenPosition;

                // Calculate light values.
                Vector2 lightOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(-Main.offScreenRange);
                Vector2 lightPosition = vineCenter + lightOffset + Main.screenPosition;

                Rectangle frame = vineTexture.Frame(2, 2, useFruitFrame.ToInt(), i == drawCount - 1 ? 1 : 0, -2, -2);

                // Make fruit emit small quantities of light.
                if (useFruitFrame)
                    Lighting.AddLight(lightPosition, Color.Pink.ToVector3() * 0.5f);
                Color color = Lighting.GetColor(lightPosition.ToTileCoordinates());

                Main.spriteBatch.Draw(vineTexture, vineCenter, frame, color, rotation, frame.Size() * 0.5f, 1f, 0f, 0);
            }
        }

        public override void DrawThingAtEndOfBranch(Branch branch)
        {
            int totalVinesToDraw = RNG.Next(5, 9);
            float swayFactor = Main.windSpeedCurrent + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.87f + RNG.NextFloat(0.94f)) * 0.4f;
            Vector2 top = branch.EndOfCurve + Vector2.UnitY * 12f;
            Vector2 bottom = top + Vector2.UnitY * totalVinesToDraw * 30f;
            DrawVine(top, bottom, totalVinesToDraw, swayFactor * 35f);
        }
    }
}
