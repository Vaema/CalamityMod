using CalamityMod.Tiles.FloralParadise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Waterfalls
{
    public static class WaterfallRenderer
    {
        private static PrimitiveTrail renderer = null;

        private static Vector2 currentWaterfallPosition;

        private static float currentWaterfallBrightness;

        private static float currentWaterfallHeight;

        public static PrimitiveTrail Renderer
        {
            get
            {
                if (renderer is null)
                    renderer = new PrimitiveTrail(PrimitiveWidthFunction, PrimitiveColorFunction, PrimitiveTrail.RigidPointRetreivalFunction, GameShaders.Misc["CalamityMod:Waterfall"]);
                return renderer;
            }
        }

        internal static float PrimitiveWidthFunction(float completionRatio)
        {
            float baseWidth = MathHelper.Lerp(16f, 6f, Utils.GetLerpValue(36f, 216f, currentWaterfallHeight, true));
            float positionOffset = currentWaterfallPosition.X * 0.17f % 300f + currentWaterfallPosition.Y * 0.33f % 300f;
            float x1Offset = (float)(Math.Sin(-Main.GlobalTimeWrappedHourly * 3f + completionRatio * 9f + positionOffset) * 0.5f + 0.5f) * baseWidth * 0.2f;
            float x2Offset = (float)(Math.Sin(-Main.GlobalTimeWrappedHourly * 6f + completionRatio * 6f + positionOffset) * 0.5f + 0.5f) * baseWidth * 0.25f;

            if (completionRatio < 0.2f)
                return MathHelper.SmoothStep(1f, baseWidth + x2Offset - x1Offset, Utils.GetLerpValue(0f, 0.2f, completionRatio, true));
            return baseWidth + x2Offset + MathHelper.SmoothStep(0f, baseWidth * 0.85f, Utils.GetLerpValue(0.84f, 0.96f, completionRatio, true)) - x1Offset;
        }

        internal static Color PrimitiveColorFunction(float completionRatio)
        {
            Color c = Color.Lerp(Color.Cyan, new Color(1f, 1f, 1f, 0f), (float)Math.Pow(Utils.GetLerpValue(0.75f, 1f, completionRatio, true), 0.68));
            return c;
        }

        public static IEnumerable<Vector2> FindNearbyWaterfallRenderers()
        {
            int waterfallGeneratorID = ModContent.TileType<WaterfallCreator>();
            Point center = Main.LocalPlayer.Center.ToTileCoordinates();
            for (int i = -Main.screenWidth / 16 - 10; i < Main.screenWidth / 16 + 10; i++)
            {
                for (int j = -Main.screenHeight / 16 - 10; j < Main.screenHeight / 16 + 10; j++)
                {
                    Tile tile = CalamityUtils.ParanoidTileRetrieval(center.X + i, center.Y + j);
                    if (tile.TileType != waterfallGeneratorID || !tile.HasTile)
                        continue;

                    yield return new Vector2(center.X + i, center.Y + j);
                }
            }
        }

        public static void DrawWaterfalls()
        {
            foreach (Vector2 drawTilePosition in FindNearbyWaterfallRenderers())
            {
                Vector2 drawPosition = drawTilePosition * 16f;
                Vector2 bottom = drawPosition;
                for (int i = 0; i < 16; i++)
                {
                    if (CalamityUtils.ParanoidTileRetrieval((int)drawTilePosition.X, (int)drawTilePosition.Y + i).LiquidAmount > 0)
                    {
                        bottom.Y += i * 16f;
                        break;
                    }
                }

                bottom.Y += 4f;
                currentWaterfallHeight = MathHelper.Distance(bottom.Y, drawPosition.Y);
                if (currentWaterfallHeight <= 20f)
                    continue;

                currentWaterfallBrightness = Lighting.Brightness((int)(drawPosition.X / 16f), (int)(drawPosition.Y / 16f));
                currentWaterfallPosition = drawPosition + Vector2.One * 8f;

                Vector2[] drawPoints = new Vector2[]
                {
                    drawPosition,
                    Vector2.Lerp(drawPosition, bottom, 0.5f),
                    bottom
                };

                // Determine the position of the waterfall's ambience sound.
                float distanceFromWaterfall = Main.LocalPlayer.Distance(drawPosition + Vector2.One * 8f);
                float distanceFromOldWaterfall = 100000f;
                if (Main.ambientWaterfallX != -1f && Main.ambientWaterfallY != -1f)
                    distanceFromOldWaterfall = Main.LocalPlayer.Distance(new Vector2(Main.ambientWaterfallX, Main.ambientWaterfallY));

                if (distanceFromOldWaterfall > distanceFromWaterfall)
                {
                    Main.ambientWaterfallX = drawPosition.X + 8f;
                    Main.ambientWaterfallY = drawPosition.Y + 8f;
                    Main.ambientWaterfallStrength = MathHelper.Max(Main.ambientWaterfallStrength, Utils.GetLerpValue(740f, 300f, distanceFromWaterfall, true));
                }

                GameShaders.Misc["CalamityMod:Waterfall"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/WorleyNoise"));
                GameShaders.Misc["CalamityMod:Waterfall"].UseOpacity((float)Math.Pow(currentWaterfallBrightness, 0.62));
                Renderer.Draw(drawPoints, Vector2.One * 8f - Main.screenPosition, 80);
            }
        }
    }
}
