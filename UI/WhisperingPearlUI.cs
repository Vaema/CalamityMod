using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.UI
{
    public class WhisperingPearlUI
    {
        /// <summary>
        /// Controls how many characters can currently be displayed
        /// </summary>
        public static float textTimer = -1;
        /// <summary>
        /// The maximum amount of characters to display for this page
        /// </summary>
        public static int maxTextTime = -1;
        /// <summary>
        /// Which page are we reading?
        /// </summary>
        public static int currentPage = -1;
        /// <summary>
        /// The localization key for the dialogue
        /// </summary>
        public static string key = "";
        /// <summary>
        /// The bottom position of the text
        /// </summary>
        public static Vector2 position = Vector2.Zero;
        /// <summary>
        /// How many page there are
        /// </summary>
        public static int pageCount = 0;

        public static void Draw(SpriteBatch sb)
        {
            if (IsActive)
            {
                bool noPearl = false;
                Point pearlTilePos = new Point((int)position.X / 16, (int)position.Y / 16);
                Tile target = CalamityUtils.ParanoidTileRetrieval(pearlTilePos.X, pearlTilePos.Y);
                // If there is no pearl, stop the dialogue
                if (!target.HasTile || target.TileType != ModContent.TileType<WhisperingPearl>())
                {
                    noPearl = true;
                }
                // If the player is too far from the pearl, cancel the dialogue
                else if (!Main.LocalPlayer.IsInTileInteractionRange(pearlTilePos.X, pearlTilePos.Y, TileReachCheckSettings.Simple))
                {
                    noPearl = true;
                }

                if (noPearl)
                {
                    FinishDialogue();
                    return;
                }

                string text = CalamityUtils.GetTextValue("UI.WhisperingPearl." + key);

                // Separate the text into pages
                string[] separated = text.Split("\n\n");
                pageCount = separated.Length;
                // If the current page is more than the total amount of pages, stop the dialogue
                if (currentPage > separated.Length - 1)
                {
                    FinishDialogue();
                    return;
                }
                string colorCode = separated[currentPage].Split('\n')[0].TrimStart();

                // Chat sounds
                if ((int)textTimer % Main.rand.Next(2, 8) == 0 && textTimer < maxTextTime)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                // System.Drawing use?????? you'd be dead to see it
                System.Drawing.Color col = System.Drawing.ColorTranslator.FromHtml("#" + colorCode);
                Color lineColor = new Color(col.R, col.G, col.B);

                separated[currentPage] = separated[currentPage].Remove(0, 6);

                maxTextTime = separated[currentPage].Length;

                Vector2 size = FontAssets.MouseText.Value.MeasureString(separated[currentPage]);
                // Shave off text based on the timer
                int charstoRemove = (separated[currentPage].Length - (int)textTimer);
                if (charstoRemove > -1)
                {
                    separated[currentPage] = separated[currentPage].Remove((int)textTimer, charstoRemove);
                }

                // How much further the BOTTOM of the text should be drawn above the pearl
                float yOffset = 40;
                Utils.DrawBorderString(sb, separated[currentPage], position - Main.screenPosition - Vector2.UnitY * (size.Y + yOffset), lineColor, anchorx: 0.5f, anchory: 0, maxCharactersDisplayed: 100000);

                // Increment the text timer
                // 0.33 means that it takes 3ish frames for 1 letter to appear
                if (textTimer < maxTextTime)
                {
                    textTimer += 0.33f;
                }
            }
            // If dialogue can't be active, finish it
            else
            {
                FinishDialogue();
            }
        }

        /// <summary>
        /// Checks if the dialogue is active
        /// </summary>
        public static bool IsActive => textTimer >= 0 && currentPage >= 0 && key != "" && position != Vector2.Zero;

        /// <summary>
        /// Manually progresses dialogue
        /// </summary>
        public static void ProgressDialogue(Vector2 drawPos, string pearlKey)
        {
            bool samePearl = pearlKey == key && drawPos == position;
            if (IsActive && samePearl)
            {
                // If the timer hasn't finished, set it to max
                if (textTimer < maxTextTime)
                {
                    textTimer = maxTextTime;
                }
                // If the tmier has finished, progress to the next page or finish if we're out of pages
                else
                {
                    if (currentPage + 1 > pageCount)
                    {
                        FinishDialogue();
                    }
                    else
                    {
                        currentPage++;
                    }
                    textTimer = 0;
                }
            }
            else if (!samePearl)
            {
                StartDialogue(drawPos, pearlKey);
            }
            else
            {
                FinishDialogue();
            }
        }

        /// <summary>
        /// Starts up pearl dialogue
        /// </summary>
        /// <param name="drawPos">The position of the bottom of the text</param>
        /// <param name="pearlkey">The name of the pearl's localization key</param>
        public static void StartDialogue(Vector2 drawPos, string pearlkey)
        {
            position = drawPos;
            currentPage = 0;
            textTimer = 0;
            key = pearlkey;
            pageCount = 0;
        }

        /// <summary>
        /// Resets all of the dialogue's variables
        /// </summary>
        public static void FinishDialogue()
        {
            position = Vector2.Zero;
            currentPage = -1;
            textTimer = -1;
            key = "";
            pageCount = 0;
        }
    }
}
