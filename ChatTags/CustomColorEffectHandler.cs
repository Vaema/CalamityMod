using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace CalamityMod.ChatTags
{
    // This is a tag handler for storing custom color effects for text all within the "ceffect" tag; the parameter determining the actual effect in use.
    // i.e [ceffect/darksun:Hello World] would apply darksun effect to "Hello World".
    public sealed class CustomColorEffectHandler : AbstractTagHandler<CustomColorEffectHandler>
    {
        protected override string[] TagNames { get; } = ["ceffect"];
        public override TextSnippet Parse(string text, Color baseColor = new(), string options = null)
        {
            if (options.Equals("darksun", StringComparison.OrdinalIgnoreCase))
                return new DarksunTextSnippet(text);
            return new TextSnippet(text);
        }
    }

    public sealed class DarksunTextSnippet(string text) : TextSnippet
    {
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = new Vector2(), Color color = new Color(), float scale = 1)
        {
            size = new Vector2(GetStringLength(FontAssets.MouseText.Value), FontAssets.MouseText.Value.MeasureString(" ").Y * scale);

            if (!justCheckingString && (color.R != 0 || color.G != 0 || color.B != 0))
            {
                var font = FontAssets.MouseText.Value;
                var BorderColor = new Color(255, 191, 73);
                var HazeColor = new Color(238, 226, 153);

                for (float f = 0f; f < MathHelper.TwoPi; f += MathHelper.TwoPi * 0.05f)
                {
                    ChatManager.DrawColorCodedString(spriteBatch, font, text, position + new Vector2(2, 0).RotatedBy(f + Main.GlobalTimeWrappedHourly), Color.Lerp(BorderColor, HazeColor, (Main.mouseTextColor - 190) / 65f * 0.1f), 0f, Vector2.Zero, new Vector2(scale));
                }
                ChatManager.DrawColorCodedString(spriteBatch, font, text, position, Color.Black, 0f, Vector2.Zero, new Vector2(scale));
            }
            return true;
        }

        public override float GetStringLength(DynamicSpriteFont font)
        {
            float size = font.MeasureString(text).X;
            return size * Scale;
        }
    }
}
