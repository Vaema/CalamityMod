using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace CalamityMod.UI.DebuffSystem
{
    public class BestiaryDebuffInfo : IBestiaryInfoElement
    {
        public string[] elements;

        public BestiaryDebuffInfo(string[] elements)
        {
            this.elements = elements;
        }

        public UIElement ProvideUIElement(BestiaryUICollectionInfo info)
        {
            // Don't show debuff info until stats are unlocked
            if (info.UnlockState < BestiaryEntryUnlockState.CanShowStats_2)
            {
                return null;
            }
            // The main background panel
            UIPanel backgroundPanel = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, 12, 7)
            {
                Width = new StyleDimension(-11f, 1f),
                Height = new StyleDimension(180f, 0f),
                BackgroundColor = new Color(43, 56, 101),
                BorderColor = Color.Transparent,
                Left = new StyleDimension(2.5f, 0f),
                PaddingLeft = 4f,
                PaddingRight = 4f
            };

            // The title of the panel
            UIText titleText = new UIText(CalamityUtils.GetTextValue("UI.DebuffSystem.Title"), 1f)
            {
                HAlign = 0f,
                VAlign = 0f,
                Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
                IsWrapped = true
            };

            backgroundPanel.Append(titleText);

            // Add all five elements to the panel, each one below the previous
            for (int i = 0; i < 5; i++)
            {
                string path = "CalamityMod/UI/DebuffSystem/";
                switch (i)
                {
                    case 0:
                        path += "Heat";
                        break;
                    case 1:
                        path += "Sickness";
                        break;
                    case 2:
                        path += "Cold";
                        break;
                    case 3:
                        path += "Electricity";
                        break;
                    case 4:
                        path += "Water";
                        break;
                }
                path += "DebuffType";
                float topPos = 0.2f + i * 0.175f;
                // Icon
                UIImage elementImage = new UIImage(ModContent.Request<Texture2D>(path))
                {
                    HAlign = 0f,
                    VAlign = 0f,
                    Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
                    Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
                    Top = new StyleDimension(0f, topPos - 0.0525f),
                    Left = new StyleDimension(8f, 0f),
                    ImageScale = 0.8f,
                };
                backgroundPanel.Append(elementImage);

                // Text
                UIText elementText = new UIText(Language.GetText(elements[i]), 0.8f)
                {
                    HAlign = 0f,
                    VAlign = 0f,
                    Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
                    Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
                    Top = new StyleDimension(0f, topPos),
                    Left = new StyleDimension(0f, 0.05f),
                };
                backgroundPanel.Append(elementText);
            }

            // Summarize the debuff resistance system when hovering over the panel
            backgroundPanel.OnUpdate += ElementDescription;
            return backgroundPanel;
        }

        public static void ElementDescription(UIElement uelement)
        {
            if (uelement.IsMouseHovering)
            {
                Main.instance.MouseText(CalamityUtils.GetTextValue("UI.DebuffSystem.Description"), 0, 0);
            }
        }
    }
}
