using System;
using CalamityMod.CalPlayer;
using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities.Terraria.Utilities;

namespace CalamityMod.UI
{
    public class FishStocksUI : ModSystem
    {
        // These values put UI start point roughly at the top center of a 1080p screen. The position is adjusted from there.
        internal const float defaultUIPositionX = 46f;
        internal const float defaultUIPositionY = 1.481f;
        private static Texture2D lineTex, overlayGraphTex, overlayTex, overlayBorderTex, screenTex, fishyPoint1Tex, fishyPoint2Tex, fishyHappy1Tex, fishyHappy2Tex, fishyPanic1Tex, fishyPanic2Tex, fishyBye1Tex, fishyBye2Tex, fishyWhatTex;
        private static int time = 0;
        private static Color normalColor => Color.Cyan;
        private static Color goodColor => Color.Lime;
        private static Color badColor => Color.Red;
        private static Color shiftColor = goodColor;
        private static float jumpMult = 0;
        private static float shakeMult = 0;
        private static float screenFlicker = 1;
        private static bool waving = true;
        public override void OnModLoad()
        {
            string folder = "CalamityMod/UI/FishStocks/";
            lineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/LineThick", AssetRequestMode.ImmediateLoad).Value;
            overlayGraphTex = ModContent.Request<Texture2D>(folder + "FishStocksGraphOverlay", AssetRequestMode.ImmediateLoad).Value;
            overlayTex = ModContent.Request<Texture2D>(folder + "FishStocksOverlay", AssetRequestMode.ImmediateLoad).Value;
            overlayBorderTex = ModContent.Request<Texture2D>(folder + "FishStocksOverlayBorder", AssetRequestMode.ImmediateLoad).Value;
            screenTex = ModContent.Request<Texture2D>(folder + "FishStocksScreenOverlay", AssetRequestMode.ImmediateLoad).Value;
            fishyPoint1Tex = ModContent.Request<Texture2D>(folder + "FishyPoint1", AssetRequestMode.ImmediateLoad).Value;
            fishyPoint2Tex = ModContent.Request<Texture2D>(folder + "FishyPoint2", AssetRequestMode.ImmediateLoad).Value;
            fishyHappy1Tex = ModContent.Request<Texture2D>(folder + "FishyHappy1", AssetRequestMode.ImmediateLoad).Value;
            fishyHappy2Tex = ModContent.Request<Texture2D>(folder + "FishyHappy2", AssetRequestMode.ImmediateLoad).Value;
            fishyPanic1Tex = ModContent.Request<Texture2D>(folder + "FishyPanic1", AssetRequestMode.ImmediateLoad).Value;
            fishyPanic2Tex = ModContent.Request<Texture2D>(folder + "FishyPanic2", AssetRequestMode.ImmediateLoad).Value;
            fishyBye1Tex = ModContent.Request<Texture2D>(folder + "FishyBye1", AssetRequestMode.ImmediateLoad).Value;
            fishyBye2Tex = ModContent.Request<Texture2D>(folder + "FishyBye2", AssetRequestMode.ImmediateLoad).Value;
            fishyWhatTex = ModContent.Request<Texture2D>(folder + "FishyWhat", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void UpdateUI(GameTime gameTime)
        {
            Player player = Main.LocalPlayer;
            time++;
            float power = Math.Clamp(MathF.Pow((Math.Abs(player.Calamity().fishStockSlidingPower) / 2), 1.8f), 0, 1);

            bool dead = player.dead;

            Color attemptColor = dead ? Color.Gray : (player.Calamity().fishStockSlidingPower >= 0 ? Color.Lerp(normalColor, goodColor, power) : Color.Lerp(normalColor, badColor, power));
            shiftColor = Color.Lerp(shiftColor, attemptColor, 0.03f);

            bool doingBad = player.Calamity().fishStockPower <= -1;
            if (time % 60 < 10 && !doingBad && !waving && !dead)
                jumpMult = MathHelper.Lerp(jumpMult, player.Calamity().fishStockPower >= 1 ? 2.5f : 1, 0.17f);
            if (jumpMult > 0)
                jumpMult = MathHelper.Lerp(jumpMult, 0, 0.09f);
            
            shakeMult = MathHelper.Lerp(shakeMult, (doingBad && !dead) ? 1 : 0, 0.08f);

            screenFlicker = MathHelper.Lerp(0.6f, 1f, MathF.Abs(MathF.Sin(time * 0.25f)));
        }
        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            if (Main.gameMenu || Main.playerInventory || player.Calamity().fishStockVisual <= 0.001f)
                return;

            float baseScale = 0.65f;
            float UIMult = (baseScale * Main.UIScale);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

            Vector2 screenPos = new Vector2(defaultUIPositionX, defaultUIPositionY) * UIMult;
            screenPos.X = (int)(screenPos.X * 0.01f * Main.screenWidth);
            screenPos.Y = (int)(screenPos.Y * 0.01f * Main.screenHeight);

            float visual = 1 - MathF.Pow(1 - player.Calamity().fishStockVisual, player.Calamity().fishStocks ? 1.2f : 20);
            waving = (!player.Calamity().fishStocks && player.Calamity().fishStockPower >= 0) || visual < 0.999f;

            float stocks = player.Calamity().fishStockPower;
            bool panic = stocks <= -1f;
            bool happy = stocks >= 1f;
            bool frame2 = time % 60 < 30;
            Texture2D fishyTexture = player.dead ? fishyWhatTex : (waving ? frame2 ? fishyBye2Tex : fishyBye1Tex : happy ? frame2 ? fishyHappy2Tex : fishyHappy1Tex :
                panic ? frame2 ? fishyPanic2Tex : fishyPanic1Tex : frame2 ? fishyPoint2Tex : fishyPoint1Tex);

            Vector2 baseUIPosition = (screenPos + new Vector2(-1350 + overlayTex.Width * 1.5f * visual, 275 + 77 * MathF.Ceiling(player.CountBuffs() / 11f)) * UIMult);
            Vector2 fishyDrawPos = baseUIPosition + new Vector2(112, 40) * UIMult;
            
            Vector2 leftEdgePos = baseUIPosition - Vector2.UnitX * overlayTex.Width;
            Vector2 rightEdgePos = baseUIPosition - Vector2.UnitX * overlayTex.Width / 2.7f;
            float fullDist = leftEdgePos.Distance(rightEdgePos) * 0.748f * UIMult;
            int lines = 5;
            int backgroundLines = 12;

            float power = player.Calamity().fishStockSlidingPower;
            float scroll = (time * 0.23f) % ((fullDist / backgroundLines));

            PixelationManager.AddPixelatedDrawer((_) => 
            {
                // Overlay
                Main.spriteBatch.Draw(overlayTex, baseUIPosition, null, shiftColor, 0f, overlayTex.Size() / 2, UIMult, SpriteEffects.None, 0.1f);

                Vector2 xAdjust = (-Vector2.UnitX * overlayTex.Width / 2.565f) * UIMult;
                float yAdjust = (overlayTex.Height / 4.4f) * UIMult;
                for (int i = 1; i <= backgroundLines; i++) // background grid
                {
                    Vector2 start = new Vector2(15 * UIMult + (fullDist / backgroundLines * (i - 1)) - scroll, -yAdjust);
                    Vector2 end = new Vector2(15 * UIMult + (fullDist / backgroundLines * i) - scroll, yAdjust);
                    Vector2 scale = new Vector2(0.1f * UIMult, 0.0103f * start.Distance(end)) * 0.05f;
                    Main.spriteBatch.Draw(lineTex, baseUIPosition + xAdjust + start, null, shiftColor * 0.2f, 0, new Vector2(lineTex.Width / 2, 0), scale, SpriteEffects.None, 0.2f);
                }
                Main.spriteBatch.Draw(lineTex, baseUIPosition + xAdjust, null, shiftColor * 0.3f, -MathHelper.PiOver2, new Vector2(lineTex.Width / 2, 0), new Vector2(0.15f * UIMult, 0.0103f * fullDist * 0.99f) * 0.05f, SpriteEffects.None, 0.2f);
                for (int i = 1; i <= lines; i++) // The stock graph
                {
                    float maxHeight = 37f * UIMult;
                    Vector2 start = new Vector2((fullDist / lines) * (i - 1), maxHeight * GetHeight(i - 1, player));
                    Vector2 end = new Vector2((fullDist / lines) * i, maxHeight * GetHeight(i, player));
                    Vector2 scale = new Vector2(0.2f * UIMult, 0.0103f * start.Distance(end)) * 0.05f;
                    Main.spriteBatch.Draw(lineTex, baseUIPosition + xAdjust + start - Vector2.UnitY * 1f, null, shiftColor, start.DirectionTo(end).ToRotation() - MathHelper.PiOver2, new Vector2(lineTex.Width / 2, 0), scale, SpriteEffects.None, 0.2f);
                }

                // Fishy
                Vector2 positionOffset = new Vector2(4 * MathF.Sin(time * 0.65f) * shakeMult, -25 * jumpMult) * UIMult;
                float fishyScale = 1.45f * UIMult;
                Main.spriteBatch.Draw(fishyTexture, fishyDrawPos + positionOffset, null, Color.Lerp(shiftColor, Color.White, 0.35f), 0f, fishyTexture.Size() / 2, fishyScale, SpriteEffects.None, 0.2f);

                // Graph overlay
                Main.spriteBatch.Draw(overlayGraphTex, baseUIPosition, null, shiftColor, 0f, overlayGraphTex.Size() / 2, UIMult, SpriteEffects.None, 0.2f);

            }, Enums.GeneralDrawLayer.AfterEverything, default);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

            string fishStocksPower = (player.Calamity().fishStockPower > -0 ? "+" : "") + Math.Round(player.Calamity().fishStockPower, 2).ToString() + "x";
            Vector2 textPos = baseUIPosition / Main.UIScale + new Vector2(-148 - (3.5f * fishStocksPower.Length), -118.5f) * baseScale;

            // Text
            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, fishStocksPower, textPos, Color.Lerp(shiftColor, Color.White, 0.35f), Color.Black, 1.4f * baseScale);

            // Screen overlay
            if (!CalamityClientConfig.Instance.Photosensitivity)
                Main.spriteBatch.Draw(screenTex, baseUIPosition / Main.UIScale, null, shiftColor * 0.2f * screenFlicker, 0f, screenTex.Size() / 2, UIMult / Main.UIScale, SpriteEffects.None, 0.3f);
            
            // Overlay Border
            Main.spriteBatch.Draw(overlayBorderTex, baseUIPosition / Main.UIScale, null, Color.White, 0f, overlayBorderTex.Size() / 2, UIMult / Main.UIScale, SpriteEffects.None, 0.4f);
        }
        public static float GetHeight(int point, Player player)
        {
            (float, float, float, float, float) oldHeights = player.Calamity().fishStockOldPower;
            float height = point == 0 ? oldHeights.Item5 :
                    point == 1 ? oldHeights.Item4 : point == 2 ? oldHeights.Item3 :
                    point == 3 ? oldHeights.Item2 : point == 4 ? oldHeights.Item1 : player.Calamity().fishStockSlidingPower;
            return -height;
        }
    }
}
