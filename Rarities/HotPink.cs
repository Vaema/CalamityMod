using System;
using System.Collections.Generic;
using CalamityMod.Items.Accessories.Wings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace CalamityMod.Rarities
{
    public class HotPink : ModRarity
    {
        // Hot Pink is used for developer items. It Has a system built in for custom rarity effects.
        // It is a unique rarity and does not have its items rarity change on reforge.
        public override Color RarityColor => TextColor;
        public static Color TextColor => new Color(255, 0, 255);

        public static Dictionary<int, Func<string, TextSnippet>> CustomRarities = new()
        {
            { ModContent.ItemType<TiredTail>(), text => new TiredTailTextEffects(text) }
        };
        public static void Draw(Item Item, SpriteBatch spriteBatch, string text, int X, int Y, Color textColor, Color lightColor, float rotation,
        Vector2 origin, Vector2 baseScale, float time, bool renderTextSparkles, DynamicSpriteFont font)
        {
            TextSnippet[] snippets = ChatManager.ParseMessage(text, textColor).ToArray();


            if (CustomRarities.ContainsKey(Item.type)) //For items in the custom rarity table, give them custom rarity effects.
            {
                for (int i = 0; i < snippets.Length; i++)
                {
                    TextSnippet textSnippet = snippets[i];
                    if (snippets[i].GetType() == typeof(TextSnippet))
                    {
                        snippets[i] = CustomRarities[Item.type].Invoke(textSnippet.Text);
                        continue;
                    }
                }
            }
            else
                ChatManager.ConvertNormalSnippets(snippets);

            ChatManager.DrawColorCodedString(spriteBatch, font, snippets, new(X, Y), textColor, 0, Vector2.Zero, baseScale, out _, -1, true);

        }

        public static void Draw(Item Item, string text, int X, int Y, float rotation, Vector2 origin, Vector2 baseScale, Color? textColor = null, Color? lightColor = null, bool? renderTextSparkles = null)
        {
            Draw(Item, Main.spriteBatch, text, X, Y, Colors.AlphaDarken(textColor ?? TextColor), lightColor ?? Color.White, rotation, origin, baseScale, Main.GlobalTimeWrappedHourly,
                renderTextSparkles ?? CalamityClientConfig.Instance.TextEffects, FontAssets.MouseText.Value);
        }

        public static void Draw(Item Item, DrawableTooltipLine line)
        {
            Draw(Item, line.Text, line.X, line.Y, line.Rotation, line.Origin, line.BaseScale);
        }

    }
}
