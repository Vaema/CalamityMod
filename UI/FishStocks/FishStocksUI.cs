using System;
using CalamityMod.CalPlayer;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.UI
{
    public class FishStocksUI : ModSystem
    {
        // These values put UI start point roughly at the top center of a 1080p screen. The position is adjusted from there.
        internal const float defaultUIPositionX = 46f;
        internal const float defaultUIPositionY = 1.481f;
        private static Texture2D lineTex, backgroundTex, overlayTex, fishyPoint1Tex, fishyPoint2Tex, fishyHappy1Tex, fishyHappy2Tex, fishyPanic1Tex, fishyPanic2Tex, fishyBye1Tex, fishyBye2Tex;
        private static int time = 0;
        private static Color goodColor = Color.Lime;
        private static Color badColor = Color.Red;
        private static Color shiftColor = goodColor;
        public override void OnModLoad()
        {
            string folder = "CalamityMod/UI/FishStocks/";
            lineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLineThick", AssetRequestMode.ImmediateLoad).Value;
            backgroundTex = ModContent.Request<Texture2D>(folder + "FishStocksBackground", AssetRequestMode.ImmediateLoad).Value;
            overlayTex = ModContent.Request<Texture2D>(folder + "FishStocksOverlay", AssetRequestMode.ImmediateLoad).Value;
            fishyPoint1Tex = ModContent.Request<Texture2D>(folder + "FishyPoint1", AssetRequestMode.ImmediateLoad).Value;
            fishyPoint2Tex = ModContent.Request<Texture2D>(folder + "FishyPoint2", AssetRequestMode.ImmediateLoad).Value;
            fishyHappy1Tex = ModContent.Request<Texture2D>(folder + "FishyHappy1", AssetRequestMode.ImmediateLoad).Value;
            fishyHappy2Tex = ModContent.Request<Texture2D>(folder + "FishyHappy2", AssetRequestMode.ImmediateLoad).Value;
            fishyPanic1Tex = ModContent.Request<Texture2D>(folder + "FishyPanic1", AssetRequestMode.ImmediateLoad).Value;
            fishyPanic2Tex = ModContent.Request<Texture2D>(folder + "FishyPanic2", AssetRequestMode.ImmediateLoad).Value;
            fishyBye1Tex = ModContent.Request<Texture2D>(folder + "FishyBye1", AssetRequestMode.ImmediateLoad).Value;
            fishyBye2Tex = ModContent.Request<Texture2D>(folder + "FishyBye2", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void UpdateUI(GameTime gameTime)
        {
            Player player = Main.LocalPlayer;
            time++;
            float power = Math.Clamp(MathF.Pow(1 - (Math.Abs(player.Calamity().fishStockSlidingPower) / 2), 2f), 0, 1);

            Color attemptColor = (player.Calamity().fishStockSlidingPower >= 0 ? Color.Lerp(goodColor, Color.Cyan, power) : Color.Lerp(badColor, Color.Cyan, power));
            shiftColor = Color.Lerp(shiftColor, attemptColor, 0.03f);
        }
        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            if (Main.gameMenu || Main.playerInventory || player.Calamity().fishStockVisual <= 0.001f)
                return;

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

            Vector2 screenPos = new Vector2(defaultUIPositionX, defaultUIPositionY);
            screenPos.X = (int)(screenPos.X * 0.01f * Main.screenWidth);
            screenPos.Y = (int)(screenPos.Y * 0.01f * Main.screenHeight);

            float visual = 1 - MathF.Pow(1 - player.Calamity().fishStockVisual, player.Calamity().fishStocks ? 1.2f : 20);

            float stocks = player.Calamity().fishStockPower;
            bool panic = stocks <= -1f;
            bool happy = stocks >= 1f;
            bool bye = (!player.Calamity().fishStocks && player.Calamity().fishStockPower >= 0) || visual < 0.999f;
            bool frame1 = player.miscCounter % 60 < 30;
            Texture2D fishyTexture = (bye ? frame1 ? fishyBye1Tex : fishyBye2Tex : happy ? frame1 ? fishyHappy1Tex : fishyHappy2Tex :
                panic ? frame1 ? fishyPanic1Tex : fishyPanic2Tex : frame1 ? fishyPoint1Tex : fishyPoint2Tex);

            Vector2 baseUIPosition = screenPos + new Vector2(-1200 + overlayTex.Width * 1.5f * visual, 185);
            Vector2 fishyDrawPos = baseUIPosition + new Vector2(110, 40);

            // Background
            Main.spriteBatch.Draw(backgroundTex, baseUIPosition, null, Color.White * 0.7f, 0f, backgroundTex.Size() / 2, 1f, SpriteEffects.None, 0f);

            Vector2 leftEdgePos = baseUIPosition - Vector2.UnitX * backgroundTex.Width;
            Vector2 rightEdgePos = baseUIPosition - Vector2.UnitX * backgroundTex.Width / 2.7f;
            float fullDist = leftEdgePos.Distance(rightEdgePos);
            int lines = 5;
            int backgroundLines = 25;

            float power = player.Calamity().fishStockSlidingPower;
            float scroll = (time * 0.33f) % ((fullDist / backgroundLines));
            for (int i = 1; i <= backgroundLines; i++) // background grid
            {
                Vector2 start = new Vector2(3 + (fullDist / backgroundLines * (i - 1)) - scroll, -backgroundTex.Height / 2.13f);
                Vector2 end = new Vector2(3 + (fullDist / backgroundLines * i) - scroll, backgroundTex.Height / 2.13f);
                Vector2 scale = new Vector2(0.1f, 0.0103f * start.Distance(end)) * 0.05f;
                Main.spriteBatch.Draw(lineTex, baseUIPosition - backgroundTex.Width / 2.1f * Vector2.UnitX + start, null, shiftColor with { A = 0 } * 0.3f, 0, new Vector2(lineTex.Width / 2, 0), scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(lineTex, baseUIPosition - Vector2.UnitX * backgroundTex.Width / 2, null, shiftColor with { A = 0 } * 0.3f, -MathHelper.PiOver2, new Vector2(lineTex.Width / 2, 0), new Vector2(0.2f, 0.0103f * fullDist) * 0.05f, SpriteEffects.None, 0f);
            for (int i = 1; i <= lines; i++) // The stock graph
            {
                Vector2 start = new Vector2((fullDist / lines) * (i - 1), 40 * GetHeight(i - 1, player));
                Vector2 end = new Vector2((fullDist / lines) * i, 40 * GetHeight(i, player));
                Vector2 scale = new Vector2(0.1f, 0.0103f * start.Distance(end)) * 0.05f;
                Main.spriteBatch.Draw(lineTex, baseUIPosition - backgroundTex.Width / 2.1f * Vector2.UnitX + start, null, shiftColor with { A = 0 }, start.DirectionTo(end).ToRotation() - MathHelper.PiOver2, new Vector2(lineTex.Width / 2, 0), scale, SpriteEffects.None, 0f);
            }
            
            // Fishy
            Main.spriteBatch.Draw(fishyTexture, fishyDrawPos, null, Color.White, 0f, fishyTexture.Size() / 2, Main.UIScale, SpriteEffects.None, 0f);

            // Overlay
            Main.spriteBatch.Draw(overlayTex, baseUIPosition, null, Color.White, 0f, overlayTex.Size() / 2, 1f, SpriteEffects.None, 0f);

            string fishStocksPower = Math.Round(player.Calamity().fishStockPower, 2).ToString() + "x";
            Vector2 textPos = baseUIPosition + new Vector2(-150 - (player.Calamity().fishStockPower < 0 ? 10 : 0), -65);
            // Text
            Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value, fishStocksPower, textPos.X, textPos.Y, Color.White, Color.Black, default, 1);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
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
