using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static CalamityMod.UI.WhisperingPearls.WhisperingPearlUI;
using static ReLogic.Graphics.DynamicSpriteFont;

namespace CalamityMod.UI.WhisperingPearls
{
    public class WhisperingPearlUI : UIState
    {
        public static readonly Dictionary<string, SoundStyle> DialogueSounds = new()
        {
            { "Amidias", SoundID.NPCHit1 },
            { "Otonilou", SoundID.NPCHit25 }
        };

        /// <summary>
        /// Controls how many characters can currently be displayed
        /// </summary>
        public static float textTimer = -1;
        /// <summary>
        /// Which page are we reading?
        /// </summary>
        public static int currentPage = -1;
        /// <summary>
        /// The localization key for the dialogue
        /// </summary>
        public static string Key = "";
        /// <summary>
        /// The bottom position of the text
        /// </summary>
        public static Vector2 position = Vector2.Zero;
        /// <summary>
        /// How many page there are
        /// </summary>
        public static int pageCount = 0;

        public static bool SwitchingPage = false;
        private int SwitchCounter = 0;

        public static readonly char[] PausePunctuation =
        [
            '.',
            '?',
            '!',
            ';',
            ':'
        ];


        internal static string Text = "";
        internal static int textIndex = 0;
        internal static Dialogue[] DialogueData;

        //Effects
        internal static Dictionary<int, (float IndexOffset, string[] hexcodes)> UniqueColors;
        internal static Dictionary<int, float> Pauses;
        internal static Dictionary<int, List<(TextEffect Effect, float[] args)>> TextEffects;
        internal static Dictionary<int, Vector2> UniqueScales;

        private static DialogueCharacterData[] CharacterData;
        private static Color BaseColor = Color.White;
        private static float DistanceOpacity = 1f;
        private static bool Crawling = true;
        private static int storedDelay = 0;

        public override void OnActivate()
        {
            DialogueData = WhisperingPearlSystem.Deserialize(Key);

            if (currentPage == 0)
                pageCount = DialogueData.Length;

            Text = "";
            UniqueColors = [];
            Pauses = [];
            TextEffects = [];
            UniqueScales = [];

            int[] lineLengths = new int[DialogueData[currentPage].Lines.Length];

            int fullLength = 0;
            for (int i = 0; i < DialogueData[currentPage].Lines.Length; i++)
            {
                string fullLine = DialogueData[currentPage].Lines[i];

                FindEffects(ref fullLine, fullLength);

                Text += fullLine + '\n';
                fullLength += fullLine.Length + 1;
                lineLengths[i] = fullLine.Length + 1;
            }

            BaseColor = WhisperingPearlSystem.GetColorFromHex(DialogueData[currentPage].BaseColor);
            CharacterData = new DialogueCharacterData[Text.Length];
            for (int i = 0; i < Text.Length; i++)
            {
                int j = 0;
                int summedLength = 0;
                for(; j < lineLengths.Length; j++)
                {
                    summedLength += lineLengths[j];
                    if (i < summedLength)
                        break;
                }
                CharacterData[i] = new(i, Text.Length, j);
            }

            textIndex = 0;
            Crawling = true;
            textTimer = -30;
            storedDelay = 0;

            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 zero = Vector2.Zero;
            bool newLine = true;

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];

                #region Positioning
                Vector2 scale = Vector2.One;
                if (UniqueScales.TryGetValue(i, out Vector2 result))
                    scale = result;

                //Checks for Special Characters, and handles Line Breaks
                switch (c)
                {
                    case '\n':
                        zero.X = 0;
                        zero.Y += font.LineSpacing * scale.Y;
                        newLine = true;
                        continue;
                    case '\r':
                        continue;
                }

                //Sets the character's position within the full text
                SpriteCharacterData spriteData = font.SpriteCharacters[c];
                Vector3 kerning = spriteData.Kerning;
                Rectangle padding = spriteData.Padding;

                if (newLine)
                    kerning.X = Math.Max(kerning.X, 0f);
                else
                    zero.X += font.CharacterSpacing * scale.X;

                zero.X += kerning.X * scale.X;
                Vector2 position = zero + spriteData.Glyph.Size() * 0.5f;
                position.X += padding.X * scale.X;
                position.Y += padding.Y * scale.Y;

                CharacterData[i].TextPosition = position - (Vector2.UnitY * scale.Y * font.LineSpacing * 0.5f);

                zero.X += (kerning.Y + kerning.Z) * scale.X;
                newLine = false;
                #endregion
            }
        }

        public override void Update(GameTime gameTime)
        {
            var pearlTilePos = new Point((int)position.X / 16, (int)position.Y / 16);
            var target = CalamityUtils.ParanoidTileRetrieval(pearlTilePos.X, pearlTilePos.Y);
            // If there is no pearl, stop the dialogue
            if (!target.HasTile || target.TileType != ModContent.TileType<WhisperingPearl>())
            {
                WhisperingPearlSystem.FinishDialogue();
                return;
            }


            float distFromPearl = Vector2.Distance(Main.LocalPlayer.Center, pearlTilePos.ToWorldCoordinates());
            // If the player is too far from the pearl, cancel the dialogue
            if (distFromPearl > 300)
            {
                WhisperingPearlSystem.FinishDialogue();
                return;
            }

            base.Update(gameTime);

            if (!SwitchingPage)
            {
                DistanceOpacity = 1 - MathHelper.Clamp((distFromPearl - 150) / 150f, 0f, 1f);

                int textDelay = DialogueData[currentPage].TextDelay;

                if (textIndex < Text.Length)
                {
                    if (textTimer == 0)
                    {
                        switch (Text[textIndex])
                        {
                            case '.':
                            case '?':
                            case '!':
                            case ';':
                            case ':':
                                storedDelay += DialogueData[currentPage].PunctuationDelay;
                                break;
                            case '-':
                                if (Text[textIndex + 1] == ' ')
                                    storedDelay += DialogueData[currentPage].PunctuationDelay;
                                break;
                            case ',':
                                storedDelay += DialogueData[currentPage].PunctuationDelay / 2;
                                break;
                        }

                        if (Pauses.TryGetValue(textIndex, out float pause))
                            storedDelay = (int)(pause * 60);
                    }

                    if (++textTimer % (Text[textIndex] == ' ' && storedDelay > 0 ? textDelay + storedDelay : textDelay) == 0 && textTimer >= 0)
                    {
                        if (Text[textIndex] == ' ')
                            storedDelay = 0;
                        else
                            SoundEngine.PlaySound(DialogueSounds[DialogueData[currentPage].Speaker]);

                        textTimer = 0;
                        ++textIndex;
                    }
                }
                else
                    Crawling = false;
            }
            else
            {
                DistanceOpacity = 1 - MathHelper.Clamp(CalamityUtils.CircOutEasing(SwitchCounter / 60f, 1), 0f, 1f);

                if(SwitchCounter >= 60)
                {
                    SwitchingPage = false;
                    SwitchCounter = 0;
                    currentPage++;
                    Activate();
                    return;
                }

                SwitchCounter++;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Vector2 size = FontAssets.MouseText.Value.MeasureString(Text);
            Vector2 pageTop = position - Main.screenPosition - new Vector2(size.X / 2f, (size.Y + 40));

            for (int i = 0; i < textIndex; i++)
            {
                char c = Text[i];

                DynamicSpriteFont font = FontAssets.MouseText.Value;

                if (c == '\r' || c == '\n')
                    continue;

                #region Drawing
                if (CharacterData == null)
                    Activate();
                Vector2 drawPos;
                //Vector2 scale = myDialogue.Scale;
                float opacity = 1f;
                float time = 30f;

                if (CharacterData[i].Timer / time < 1f)
                {
                    float easing = (float)Math.Sin(CharacterData[i].Timer / time * MathF.PI / 2f);
                    drawPos = Vector2.Lerp(position - Main.screenPosition, pageTop + CharacterData[i].TextPosition, easing);
                    opacity = CalamityUtils.SineInOutEasing(MathHelper.Clamp(CharacterData[i].Timer / 20f, 0f, 1f), 1);
                }
                else
                    drawPos = pageTop + CharacterData[i].TextPosition;

                Color color;
                if (UniqueColors.TryGetValue(i, out var value))
                {
                    Color[] colors = new Color[value.hexcodes.Length];
                    for(int j = 0; j < colors.Length; j++)
                        colors[j] = WhisperingPearlSystem.GetColorFromHex(value.hexcodes[j]);

                    color = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly + (i * value.IndexOffset), colors);
                }
                else
                    color = BaseColor;
                float rotation = 0f;
                Vector2 scale = Vector2.One;
                if (UniqueScales.TryGetValue(i, out Vector2 result))
                    scale = result;

                foreach (var l in TextEffects.Where(v => v.Key == i))
                {
                    foreach ((TextEffect Effect, float[] args) in l.Value)
                    {
                        drawPos = Effect.ModifyPos(drawPos, CharacterData[i], args);

                        rotation = Effect.ModifyRot(rotation, CharacterData[i], args);

                        color = Effect.ModifyColor(color, CharacterData[i], args);

                        scale = Effect.ModifyScale(scale, CharacterData[i], args);
                    }
                }

                Color borderColor = color;
                borderColor.R /= 3;
                borderColor.G /= 3;
                borderColor.B /= 3;

                scale = Vector2.Lerp(Vector2.Zero, scale, CalamityUtils.CircOutEasing(MathHelper.Clamp(CharacterData[i].Timer / time, 0f, 1f), 1));

                SpriteCharacterData spriteData = font.SpriteCharacters[c];
                Vector2 origin = spriteData.Glyph.Size() * 0.5f;

                for (int j = 0; j < ChatManager.ShadowDirections.Length; j++)
                {
                    spriteBatch.Draw(spriteData.Texture, drawPos + (ChatManager.ShadowDirections[j] * 2), spriteData.Glyph, borderColor * opacity * DistanceOpacity, rotation, origin, scale * (2 - DistanceOpacity), SpriteEffects.None, 1);
                }
                spriteBatch.Draw(spriteData.Texture, drawPos, spriteData.Glyph, color * opacity * DistanceOpacity, rotation, origin, scale * (2 - DistanceOpacity), SpriteEffects.None, 1);
                #endregion

                CharacterData[i].Timer++;
            }
        }

        public static void FindEffects(ref string fullLine, int fullLength)
        {
            Stack<int> returnPoints = [];
            Stack<string> returnString = [];

            for (int j = 0; j < fullLine.Length; j++)
            {
                char c = fullLine[j];

                if (c == '[')
                {
                    int k = j + 1;
                    string currentData = "[";
                    bool readingData = true;
                    for (k = j + 1; k < fullLine.Length; k++)
                    {
                        if (fullLine[k] == ']')
                            break;
                        if (fullLine[k] == '[')
                        {
                            returnPoints.Push(j);
                            returnString.Push(currentData);
                            currentData = "[";
                            c = fullLine[k];
                            j = k;
                            readingData = true;
                        }
                        else if (readingData)
                        {
                            currentData += fullLine[k];
                            if (fullLine[k] == ':')
                                readingData = false;
                        }

                    }
                    if (fullLine[k] != ']')
                        throw new Exception("[ was found without a ] preceeding it.");

                    string effect = fullLine[j..k];
                    string ID = "";
                    string Text = "";
                    List<float> Params = [];
                    List<string> ColorParams = [];
                    string Param = "";
                    bool readingText = false;
                    bool readingParams = false;
                    for(int l = 1; l < effect.Length; l++)
                    {
                        char ch = effect[l];
                        if (ch == '(')
                        {
                            readingParams = true;
                            continue;
                        }
                        else if (ch == ':')
                        {
                            readingText = true;
                            continue;
                        }
                        else if (ch == ']')
                            break;

                        if (readingText)
                            Text += ch;
                        else if (readingParams)
                        {
                            if (ch == ',' || ch == ')')
                            {
                                if (ID == "Colors")
                                {
                                    if (float.TryParse(Param, out float result))
                                        Params.Add(result);
                                    else
                                        ColorParams.Add(Param);
                                }
                                else
                                {
                                    if (float.TryParse(Param, out float result))
                                        Params.Add(result);
                                    else
                                        throw new Exception("Invalid Parameter found");
                                }
                                Param = "";
                            }
                            else if (ch == ' ')
                                continue;
                            else
                                Param += ch;
                        }
                        else
                            ID += ch;                    
                    }

                    fullLine = fullLine.Remove(j, k - j + 1);
                    fullLine = fullLine.Insert(j, Text);

                    if (ID == "Pause")
                    {
                        Pauses.Add(j - 1 + fullLength - ID.Length - Params[0].ToString().Length, Params[0]);
                    }
                    else
                    {
                        for (int i = 0; i < Text.Length; i++)
                        {
                            int index = j + i + fullLength;
                            if (ID == "Colors")
                            {
                                int storedLen = 0;
                                foreach (string s in returnString)
                                    storedLen += s.Length;

                                UniqueColors.Add(index - storedLen, (Params.Count == 0 ? 0 : Params[0], [.. ColorParams]));
                            }
                            else if(ID == "Scale")
                            {
                                int storedLen = 0;
                                foreach (string s in returnString)
                                    storedLen += s.Length;

                                Vector2 scale;
                                if (Params.Count == 0)
                                    scale = Vector2.One;
                                else if (Params.Count == 1)
                                    scale = new(Params[0], Params[0]);
                                else
                                    scale = new(Params[0], Params[1]);


                                UniqueScales.Add(index - storedLen, scale);
                            }
                            else
                            {
                                int storedLen = 0;
                                foreach (string s in returnString)
                                    storedLen += s.Length;

                                string path = "CalamityMod.UI.WhisperingPearls.";
                                Type t = Type.GetType(path + ID) ?? throw new Exception("Invalid text effect ID found");
                                TextEffect te = (TextEffect)Activator.CreateInstance(t);
                                if (TextEffects.TryGetValue(index - storedLen, out var value))
                                    value.Add(new(te, [.. Params]));
                                else
                                    TextEffects.Add(index - storedLen, [ new(te, [.. Params]) ]);
                            }
                        }
                    }

                    if (returnPoints.Count > 0)
                    {
                        j = returnPoints.Pop() - 1;                       
                        returnString.Pop();
                    }
                }
            }
        }

        /*public static void DrawSelf(SpriteBatch sb)
        {
            if (WhisperingPearlSystem.IsActive)
            {
                


                // Separate the text into pages
                string[] separated = text.Split("\n\n");
                pageCount = separated.Length;
                // If the current page is more than the total amount of pages, stop the dialogue
                if (currentPage > separated.Length - 1)
                {
                    WhisperingPearlSystem.FinishDialogue();
                    return;
                }
                var colorCode = separated[currentPage].Split('\n')[0].TrimStart();

                // Chat sounds
                if ((int)textTimer % Main.rand.Next(2, 8) == 0 && textTimer < maxTextTime)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                // System.Drawing use?????? you'd be dead to see it
                var col = System.Drawing.ColorTranslator.FromHtml("#" + colorCode);
                var lineColor = new Color(col.R, col.G, col.B);

                separated[currentPage] = separated[currentPage].Remove(0, 6);

                maxTextTime = separated[currentPage].Length;

                var size = FontAssets.MouseText.Value.MeasureString(separated[currentPage]);
                // Shave off text based on the timer
                var charstoRemove = separated[currentPage].Length - (int)textTimer;
                if (charstoRemove > -1)
                {
                    separated[currentPage] = separated[currentPage].Remove((int)textTimer, charstoRemove);
                }

                // Pause briefly on elipses
                var increment = 0.33f;
                if (separated[currentPage].EndsWith("..."))
                {
                    increment /= 20f; 
                }
                // How much further the BOTTOM of the text should be drawn above the pearl
                float yOffset = 40;
                Utils.DrawBorderString(sb, separated[currentPage], position - Main.screenPosition - Vector2.UnitY * (size.Y + yOffset), lineColor, anchorx: 0.5f, anchory: 0, maxCharactersDisplayed: 100000);

                // Increment the text timer
                // 0.33 means that it takes 3ish frames for 1 letter to appear
                if (textTimer < maxTextTime)
                {
                    textTimer += increment;
                }
            }
            // If dialogue can't be active, finish it
            else
            {
                WhisperingPearlSystem.FinishDialogue();
            }
        }*/
    }

    public class WhisperingPearlSystem : ModSystem
    {
        /// <summary>
        /// Checks if the dialogue is active
        /// </summary>
        public static bool IsActive => textTimer >= 0 && currentPage >= 0 && Key != "" && position != Vector2.Zero;

        public static WhisperingPearlUI State;

        public static UserInterface UI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                UI = new();
                State = new();
                Key = "RoyalBlue";
                currentPage = 0;
                textTimer = 0;
                pageCount = 0;
                State.Activate();
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseIndex != -1)
            {
                layers.Insert(mouseIndex, new LegacyGameInterfaceLayer("Whispering Pearl", () =>
                {
                    UI.Draw(Main.spriteBatch, new());
                    return true;
                }, InterfaceScaleType.Game));
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (UI?.CurrentState != null)
                UI?.Update(gameTime);
        }

        public static Dialogue[] Deserialize(string key)
        {
            string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
            string path = "UI/WhisperingPearls/" + activeExtension + "/" + key + ".json";

            // Fall back to english if not found
            if (!CalamityMod.Instance.FileExists(path))
                path = "UI/WhisperingPearls/en-US/" + key + ".json";

            // Throw if we cant find english either
            if (!CalamityMod.Instance.FileExists(path))
                throw new FileNotFoundException($"Could not find the dialog file {path}.");

            Stream stream = CalamityMod.Instance.GetFileStream(path);

            Dialogue[] data = JsonSerializer.Deserialize<Dialogue[]>(stream);

            stream.Close();

            return data;
        }

        public static Color GetColorFromHex(string hex)
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml('#' + hex);
            int r = Convert.ToInt16(color.R);
            int g = Convert.ToInt16(color.G);
            int b = Convert.ToInt16(color.B);
            return new Color(r, g, b);
        }

        /// <summary>
        /// Manually progresses dialogue
        /// </summary>
        public static void ProgressDialogue(Vector2 drawPos, string pearlKey)
        {
            var samePearl = pearlKey == Key && drawPos == position;
            if (IsActive && samePearl)
            {
                if (SwitchingPage)
                    return;

                // If the text crawl hasnt finished, finish it instantly
                if (textIndex < Text.Length - 1)
                    textIndex = Text.Length - 1;
                // If the text crawl has finished, progress to the next page or finish if we're out of pages
                else
                {
                    if (currentPage + 1 >= pageCount)
                        FinishDialogue();
                    else
                        SwitchingPage = true;
                    textTimer = 0;
                }
            }
            else if (!samePearl)
                StartDialogue(drawPos, pearlKey);
            else
                FinishDialogue();
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
            Key = pearlkey;
            pageCount = 0;

            UI = new();
            State = new();
            UI?.SetState(State);
        }

        /// <summary>
        /// Resets all of the dialogue's variables
        /// </summary>
        public static void FinishDialogue()
        {
            position = Vector2.Zero;
            currentPage = -1;
            textTimer = -1;
            Key = "";
            pageCount = 0;

            UI?.SetState(null);
        }
    }

    public class Dialogue
    {
        public string BaseColor { get; set; }
        public string Speaker { get; set; }
        public string[] Lines { get; set; }

        public int TextDelay { get; set; } = 3;
        public int PunctuationDelay { get; set; } = 10;
    }

    public class DialogueCharacterData(int index, int textLength, int lineNumber)
    {
        public int Index = index;

        public int TextLength = textLength;

        public int LineNumber = lineNumber;

        public float CompletionRatio => Index / (float)TextLength;

        public Vector2 TextPosition = Vector2.Zero;

        public int Timer = 0;
    }
}
