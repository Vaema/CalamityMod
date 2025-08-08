using System;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria.ID;

namespace CalamityMod.Rarities
{
    public class CalamityRed : ModRarity
    {
        // Calamity Red is Rarity 17
        // This rarity should never be assigned to any items.
        // It is the equivalent of vanilla's Purple rarity (11): only used for positive reforges on top-rarity items.
        //public override Color RarityColor => new Color(163, 25, 26); // #A3191A
        //                                                             // (139, 0, 0) is the classic donator item color

        public override Color RarityColor => TextClr * 2f;

        public static float MaxY = 4.5f;
        public static Color BloomClr = new Color(180, 20, 75, 0);
        public static Color TextClr = new Color(242, 27, 27, 255);

        public static void Draw(Item Item, SpriteBatch spriteBatch, string text, int X, int Y, Color textColor, Color lightColor, float rotation,
            Vector2 origin, Vector2 baseScale, float time, bool renderTextSparkles, DynamicSpriteFont font)
        {
            var crystalTextGlow = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/UI/CrystalTextGlow").Value;
            var sparkle = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/UI/CrystalTextSparkle").Value;
            var fontSize = font.MeasureString(text);
            var center = fontSize / 2f;

            var glowPosition = new Vector2(X + center.X, Y + center.Y / 1.5f);
            textColor.A = 0;
            float pulsing = 10f + (float)Math.Sin(time * 20f);
            float baseScalePulse = 1.03f;
            float flameHeight = 5f;
            float distortionAmount = 2f;

            if (renderTextSparkles)

            for (float f = 0f; f < MathHelper.TwoPi; f += 0.79f)
            {
                float angle = f + (time * 2f % MathHelper.TwoPi);

                float distortion = (float)Math.Sin((Y + f * 100f + time * 20f) * 0.05f) * distortionAmount;

                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * pulsing * 0.4f + distortion,
                    -(float)Math.Abs((float)Math.Sin(angle)) * flameHeight
                );

                float scaleVariation = 0.95f + 0.05f * (float)Math.Sin(time * 15f + f * 2f);
                Vector2 scale = new Vector2(baseScalePulse * scaleVariation);

                Color flameLayerColor = new Color(
                    (int)(textColor.R * 0.5f),
                    (int)(textColor.G * 0.5f),
                    (int)(textColor.B * 0.5f),
                    (int)(textColor.A * 0.5f)
                );

                ChatManager.DrawColorCodedString(
                    spriteBatch,
                    font,
                    text,
                    new Vector2(X, Y) + offset,
                    flameLayerColor,
                    rotation,
                    origin,
                    scale
                );
            }

            // Draw crisp center text
            ChatManager.DrawColorCodedString(
                spriteBatch,
                font,
                text,
                new Vector2(X, Y),
                textColor,
                rotation,
                origin,
                new Vector2(baseScalePulse)
            );

            textColor.A = 255;

            ChatManager.DrawColorCodedStringShadow(spriteBatch, font, text, new Vector2(X, Y), textColor * 2f, rotation, origin, baseScale);

            var bloomColor = ColorTool.Rainbowing(time * 4 - 0.9f);

            spriteBatch.Draw(crystalTextGlow, glowPosition, null, lightColor, rotation + MathHelper.PiOver2, new Vector2(6f, 33f),
               new Vector2(1.6f, fontSize.X / crystalTextGlow.Height * 1.2f), SpriteEffects.None, 0f);

            ChatManager.DrawColorCodedString(spriteBatch, font, text, new Vector2(X, Y), Color.Black, rotation, origin, baseScale);

            // Is this a scrapped visual feature of the rarity?
            /*int sparkleCount = Main.rand.Next((int)fontSize.X / 7, (int)fontSize.X / 5) + 1;
            var color2 = lightColor * 1.00f;
            color2.A = 0;
            var sparkleOrigin = new Vector2(15f, 15f);
            for (int i = 0; i < sparkleCount; i++)
            {
                var v = new Vector2(Main.rand.NextFloat(fontSize.X), Main.rand.NextFloat(fontSize.Y * 0.6f) + 1f);
                float lifeTime = Main.GlobalTimeWrappedHourly * 4f + Main.rand.NextFloat(MathHelper.TwoPi * 7f);
                lifeTime %= MathHelper.TwoPi * 1f;
            }*/

            return;
        }

        public static void Draw(Item Item, string text, int X, int Y, float rotation, Vector2 origin, Vector2 baseScale, Color? textColor = null, Color? lightColor = null, bool? renderTextSparkles = null)
        {
            Draw(Item, Main.spriteBatch, text, X, Y, Colors.AlphaDarken(textColor ?? TextClr), lightColor ?? BloomClr, rotation, origin, baseScale, Main.GlobalTimeWrappedHourly,
                renderTextSparkles ?? CalamityClientConfig.Instance.TextEffects, FontAssets.MouseText.Value);
        }

        public static void Draw(Item Item, DrawableTooltipLine line)
        {
            Draw(Item, line.Text, line.X, line.Y, line.Rotation, line.Origin, line.BaseScale);
        }

        // TODO: Add a cooler alternative for reforge rarities
        /*public override int GetPrefixedRarity(int offset, float valueMult) => offset switch
        {
            -2 => ModContent.RarityType<BurnishedAuric>(),
            -1 => ModContent.RarityType<HotPink>(),
            _ => Type,
        };*/
    }
}
