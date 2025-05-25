using CatalystMod.Items;
using System;
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
using System.IO;

namespace CalamityMod.Rarities
{
    public class BurnishedAuric : ModRarity
    {
        // Burnished Auric is Rarity 15 (Once was known as Violet)
        public override Color RarityColor => TextClr * 2f;

        public static float MaxY = 4.5f;
        public static Color BloomClr = new Color(48, 33, 4, 0);
        public static Color TextClr = new Color(157, 110, 11, 50);

        public static void Draw(Item Item, SpriteBatch spriteBatch, string text, int X, int Y, Color textColor, Color lightColor, float rotation,
        Vector2 origin, Vector2 baseScale, float time, bool renderTextSparkles, DynamicSpriteFont font)
        {
            var fontSize = font.MeasureString(text);
            var center = fontSize / 2.0f;

            var glowPosition = new Vector2(X + center.X, Y + center.Y / 1.5f);
            textColor.A = 0;

            float pulsing = 1.5f + (float)Math.Sin(time * 5f);
            for (float f = 0f; f < MathHelper.TwoPi; f += 0.79f)
            {
                ChatManager.DrawColorCodedString(
                    spriteBatch, font, text,
                    new Vector2(X, Y) + new Vector2(pulsing, 0f).RotatedBy(f + time * 2f % MathHelper.TwoPi),
                    textColor * 0.5f, rotation, origin, baseScale);
            }

            textColor.A = 255;

            // Shadow
            ChatManager.DrawColorCodedStringShadow(spriteBatch, font, text, new Vector2(X, Y), textColor * 2f, rotation, origin, baseScale);

            // Outline base
            ChatManager.DrawColorCodedString(spriteBatch, font, text, new Vector2(X, Y), new Color(77, 0, 33), rotation, origin, baseScale);

            float shineWidth = 40f; //set to charSize
            float shineSpeed = 80f;

            float shineDisp = time * shineSpeed; //predicted current location of the shine based on the time and speed
            float shinePos = (shineDisp % (fontSize.X + shineWidth));

            Vector2 basePos = new Vector2(X, Y);

            float charOffsetX = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                string c = text[i].ToString();
                Vector2 charSize = font.MeasureString(c);
                Vector2 charOrigin = charSize * 0.5f;

                Vector2 charPos = basePos + new Vector2(charOffsetX, 0f);

                float centerX = charPos.X + (charSize.X * baseScale.X) / 2f + 10.5f;
                float dist = Math.Abs(centerX - (X + shinePos - shineWidth * 0.15f));
                float intensity = 1f - MathHelper.Clamp(dist / shineWidth, 0f, 1f);

                if (intensity > 0f)
                {
                    Color shineColor = new Color(255, 233, 155) * intensity * 1f;
                    ChatManager.DrawColorCodedString(
                        spriteBatch, font, c, charPos, shineColor, rotation, origin, baseScale);
                }

                charOffsetX += charSize.X - text.Length * 0.0085f;
            }

            if (!renderTextSparkles)
                return;

            var rand = new UnifiedRandom(Main.LocalPlayer.name.GetHashCode() + (int)(center.X + center.Y));
        }

        public static void Draw(Item Item, string text, int X, int Y, float rotation, Vector2 origin, Vector2 baseScale, Color? textColor = null, Color? lightColor = null, bool? renderTextSparkles = null)
        {
            Draw(Item, Main.spriteBatch, text, X, Y, Colors.AlphaDarken(textColor == null ? TextClr : textColor.Value), lightColor == null ? BloomClr : lightColor.Value, rotation, origin, baseScale, Main.GlobalTimeWrappedHourly,
                renderTextSparkles == null ? Language.ActiveCulture == GameCulture.FromCultureName(GameCulture.CultureName.Chinese) ? false : CalamityClientConfig.Instance.textEffects : renderTextSparkles.Value,
                FontAssets.MouseText.Value);
        }

        public static void Draw(Item Item, DrawableTooltipLine line)
        {
            Draw(Item, line.Text, line.X, line.Y, line.Rotation, line.Origin, line.BaseScale);
        }

        public override int GetPrefixedRarity(int offset, float valueMult) => offset switch
        {
            -2 => ModContent.RarityType<PureGreen>(),
            -1 => ModContent.RarityType<CosmicPurple>(),
            1 => ModContent.RarityType<HotPink>(),
            2 => ModContent.RarityType<CalamityRed>(),
            _ => Type,
        };
    }
}
