using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CalamityMod.Projectiles.Boss;
using CalamityMod.UI.DraedonSummoning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static ReLogic.Graphics.DynamicSpriteFont;

namespace CalamityMod.UI.DialogueDisplay
{
    public class DialogueDisplayUI : UIState
    {
        public static readonly Dictionary<string, DialogueDisplay> Dialogues = [];
        public static readonly Dictionary<string, Entity> DialogueEntities = [];
        public static readonly Dictionary<string, int> DialogueUptimes = [];
        public static readonly List<string> DialoguesToRemove = [];

        public override void Update(GameTime gameTime)
        {
            foreach (string key in DialoguesToRemove)
            {
                RemoveChild(Dialogues[key]);
                Dialogues.Remove(key);
                DialogueEntities.Remove(key);
                DialogueUptimes.Remove(key);
            }
            DialoguesToRemove.Clear();

            foreach (var pair in DialogueEntities)
            {
                if (pair.Value != null && pair.Value.active)
                    Dialogues[pair.Key].Position = pair.Value.Center;
                else
                    Dialogues[pair.Key].ClosingDialogue = true;
            }

            foreach (var pair in DialogueUptimes)
            {
                if (Dialogues[pair.Key].Uptime >= pair.Value)
                {
                    if (Dialogues[pair.Key].ProgressDialogue)
                        Dialogues[pair.Key].SwitchingPage = true;
                    else
                        Dialogues[pair.Key].ClosingDialogue = true;
                }
            }

            base.Update(gameTime);
        }
    }

    public class DialogueDisplay(string key, DialogueDisplayEffects displayEffects, int startPage = 0, bool screenLocked = false, string font = "MouseText") : UIElement
    {
        public static readonly Dictionary<string, SoundStyle> DialogueSounds = new()
        {
            { "Amidias", SoundID.NPCHit1 },
            { "Otonilou", SoundID.NPCHit25 }
        };

        //A more centralized spot for Fonts might be desired in the future
        public static readonly Dictionary<string, DynamicSpriteFont> Fonts = new()
        {
            { "MouseText", FontAssets.MouseText.Value },
            { "ItemStack", FontAssets.ItemStack.Value },
            { "DeathText", FontAssets.DeathText.Value },
            { "CombatText1", FontAssets.CombatText[0].Value },
            { "CombatText2", FontAssets.CombatText[1].Value },
            { "WingDings", DoGWingdings.Wingdings },
            { "CodebreakerDialog", CodebreakerUI.DialogFont },
            { "Impact", CalamityMod.Instance.Assets.Request<DynamicSpriteFont>("Fonts/Impact", AssetRequestMode.ImmediateLoad).Value },
            { "Flexure", CalamityMod.Instance.Assets.Request<DynamicSpriteFont>("Fonts/Flexure", AssetRequestMode.ImmediateLoad).Value }
        };

        public int TextTimer = 0;
        /// <summary>
        /// Which page are we reading?
        /// </summary>
        public int currentPage = startPage;
        /// <summary>
        /// The localization key for the dialogue
        /// </summary>
        public string Key = key;
        /// <summary>
        /// The bottom position of the text
        /// </summary>
        public Vector2 Position = Vector2.Zero;
        /// <summary>
        /// How many page there are
        /// </summary>
        public int pageCount => DialogueData.Length;

        public bool SwitchingPage = false;
        public bool ProgressDialogue = true;
        public bool ClosingDialogue = false;
        public bool ScreenLocked = screenLocked;

        public Vector2 TextSize { get; private set; }
        public Vector2 SizeOffsetFromStart { get; private set; }

        public bool Switching => SwitchingPage || ClosingDialogue;
        private int SwitchCounter = 0;

        internal DialogueTextData DialogueData = DialogueDisplaySystem.Deserialize(key);
        internal DialogueDisplayEffects DisplayEffects = displayEffects;
        internal string Text = "";
        private int CharacterTimer = 0;
        internal int textIndex = 0;
        internal int Uptime = 0;
        internal string Font = font;


        //Effects
        internal Dictionary<int, (float IndexOffset, string[] hexcodes)> UniqueColors = [];
        internal Dictionary<int, float> Pauses = [];
        internal Dictionary<int, List<(TextEffect Effect, float[] args)>> TextEffects = [];
        internal Dictionary<int, Vector2> UniqueScales = [];

        private DialogueCharacterData[] CharacterData;
        private Color BaseColor = Color.White;
        internal bool Crawling = true;
        private int storedDelay = 0;
        private bool lockDelay = false;

        public override void OnActivate()
        {
            Text = "";
            UniqueColors = [];
            Pauses = [];
            TextEffects = [];
            UniqueScales = [];

            if (DialogueData.Dialogues[currentPage].Event != null)
                return;

            int[] lineLengths = new int[DialogueData[currentPage].Lines.Length];

            int fullLength = 0;
            for (int i = 0; i < DialogueData[currentPage].Lines.Length; i++)
            {
                string fullLine = DialogueData[currentPage].Lines[i];

                FindEffects(ref fullLine, fullLength);

                if (i != DialogueData[currentPage].Lines.Length - 1 && fullLine[^1] != ' ')
                    fullLine += ' ';

                Text += fullLine;
                if (i < DialogueData[currentPage].Lines.Length - 1)
                    Text += '\n';
                fullLength += fullLine.Length + 1;
                lineLengths[i] = fullLine.Length + 1;
            }

            if (DialogueData[currentPage].BaseColor != null)
                BaseColor = DialogueDisplaySystem.GetColorFromHex(DialogueData[currentPage].BaseColor);
            else if (DialogueData.DefaultColor != null)
                BaseColor = DialogueDisplaySystem.GetColorFromHex(DialogueData.DefaultColor);

            CharacterData = new DialogueCharacterData[Text.Length];

            for (int i = 0; i < Text.Length; i++)
            {
                int j = 0;
                int summedLength = 0;
                for (; j < lineLengths.Length; j++)
                {
                    summedLength += lineLengths[j];
                    if (i < summedLength)
                        break;
                }
                CharacterData[i] = new(i, Text.Length, j);
            }

            textIndex = 0;
            Crawling = true;
            CharacterTimer = -30;
            storedDelay = 0;
            lockDelay = false;
            TextTimer = 0;
            Uptime = 0;

            Vector2 zero = Vector2.Zero;
            bool newLine = true;

            float textWidth = 0f;

            float highestFirstLineYScale = 1f;
            for (int j = 0; j < Text.Length; j++)
            {
                if (Text[j] == '\n')
                    break;
                if (UniqueScales.TryGetValue(j, out Vector2 uniqueScale) && uniqueScale.Y > highestFirstLineYScale)
                    highestFirstLineYScale = uniqueScale.Y;
            }

            SizeOffsetFromStart = new(8, 16 * highestFirstLineYScale);

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];

                #region Positioning
                Vector2 scale = Vector2.One;
                if (UniqueScales.TryGetValue(i, out Vector2 result))
                    scale = result;
                else if (DialogueData[currentPage].TextScale != -1)
                    scale *= DialogueData[currentPage].TextScale;
                else
                    scale *= DialogueData.DefaultScale;

                //Checks for Special Characters, and handles Line Breaks
                switch (c)
                {
                    case '\n':

                        if (zero.X > textWidth)
                            textWidth = zero.X;

                        zero.X = 0;

                        float highestYscale = 1f;
                        for (int j = i + 1; j < Text.Length; j++)
                        {
                            if (Text[j] == '\n')
                                break;
                            if (UniqueScales.TryGetValue(j, out Vector2 uniqueScale) && uniqueScale.Y > highestYscale)
                                highestYscale = uniqueScale.Y;
                        }
                        zero.Y += Fonts[Font].LineSpacing * highestYscale;
                        newLine = true;
                        continue;
                    case '\r':
                        continue;
                }

                //Sets the character's position within the full text
                SpriteCharacterData spriteData = Fonts[Font].SpriteCharacters[c];
                Vector3 kerning = spriteData.Kerning;
                Rectangle padding = spriteData.Padding;

                if (newLine)
                    kerning.X = Math.Max(kerning.X, 0f);
                else
                    zero.X += Fonts[Font].CharacterSpacing * scale.X;

                zero.X += kerning.X * scale.X;
                Vector2 position = zero + spriteData.Glyph.Size() * 0.5f;
                position.X += padding.X * scale.X;
                position.Y += padding.Y * scale.Y;

                CharacterData[i].TextPosition = position - (Vector2.UnitY * scale.Y * Fonts[Font].LineSpacing * 0.5f);

                zero.X += (kerning.Y + kerning.Z) * scale.X;
                newLine = false;
                #endregion
            }

            if (zero.X > textWidth)
                textWidth = zero.X;
            float textHeight = zero.Y;

            TextSize = new Vector2(textWidth + 8, textHeight + 12) + SizeOffsetFromStart;
            Main.NewText(Text);
        }

        private void FindEffects(ref string fullLine, int fullLength)
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
                    for (int l = 1; l < effect.Length; l++)
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
                            else if (ID == "Scale")
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

                                string path = "CalamityMod.UI.DialogueDisplay.";
                                Type t = Type.GetType(path + ID) ?? throw new Exception("Invalid text effect ID found");
                                TextEffect te = (TextEffect)Activator.CreateInstance(t);
                                if (TextEffects.TryGetValue(index - storedLen, out var value))
                                    value.Add(new(te, [.. Params]));
                                else
                                    TextEffects.Add(index - storedLen, [new(te, [.. Params])]);
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

        public override void Update(GameTime gameTime)
        {
            float distFromSource = 0;
            if (DialogueDisplayUI.Dialogues[Key].DisplayEffects.FadeWhenTooFar)
            {
                distFromSource = Vector2.Distance(Main.LocalPlayer.Center, Position);
                // If the player is too far from the pearl, cancel the dialogue
                if (distFromSource > DialogueDisplayUI.Dialogues[Key].DisplayEffects.FadeBuffer + DialogueDisplayUI.Dialogues[Key].DisplayEffects.FadeDistance)
                {
                    DialogueDisplaySystem.RemoveDialogue(Key);
                    return;
                }
            }
            base.Update(gameTime);

            if (DialogueData[currentPage].Event != null)
            {
                DialogueData[currentPage].Event.UpdateEvent();
                if (DialogueData[currentPage].Event.IsOver)
                {
                    SwitchingPage = false;
                    SwitchCounter = 0;
                    currentPage++;

                    Activate();
                    return;
                }
            }
            else if (!Switching)
            {
                if (DialogueDisplayUI.Dialogues[Key].DisplayEffects.FadeWhenTooFar)
                    SwitchCounter = (int)(MathHelper.Clamp((distFromSource - DialogueDisplayUI.Dialogues[Key].DisplayEffects.FadeBuffer) / DialogueDisplayUI.Dialogues[Key].DisplayEffects.FadeDistance, 0f, 1f) * DialogueDisplayUI.Dialogues[Key].DisplayEffects.TimeToDisappear);

                int textDelay = DialogueData.TextDelay;
                if (DialogueData[currentPage].TextDelay != -1)
                    textDelay = DialogueData[currentPage].TextDelay;

                if (DialogueData[currentPage].Event != null && !DialogueData[currentPage].Event.IsOver)
                {
                    DialogueData[currentPage].Event.UpdateEvent();
                }

                if (textIndex < Text.Length)
                {
                    if (CharacterTimer == 0)
                    {
                        if (!lockDelay)
                        {
                            var data = DialogueData.BasePunctuationDelay;
                            if (DialogueData[currentPage].BasePunctuationDelay != null)
                                data = DialogueData[currentPage].BasePunctuationDelay;

                            if (DialogueData[currentPage].PunctuationDelays != null && DialogueData[currentPage].PunctuationDelays.TryGetValue(Text[textIndex].ToString(), out var value))
                                data = value;
                            else if (DialogueData.PunctuationDelays.TryGetValue(Text[textIndex].ToString(), out value))
                                data = value;

                            switch (Text[textIndex])
                            {
                                case '.':
                                case '?':
                                case '!':
                                case ';':
                                case ':':
                                case ',':
                                    if (data.ForceSet)
                                        storedDelay = data.Delay;
                                    else
                                        storedDelay += data.Delay;
                                    break;
                                case '-':
                                    if (Text[textIndex + 1] == ' ')
                                    {
                                        if (data.ForceSet)
                                            storedDelay = data.Delay;
                                        else
                                            storedDelay += data.Delay;
                                    }
                                    break;
                            }

                            if (data.Locks)
                                lockDelay = true;
                        }

                        if (Pauses.TryGetValue(textIndex, out float pause))
                            storedDelay = (int)(pause * 60);
                    }

                    if (++CharacterTimer % ((Text[textIndex] == ' ' || Text[textIndex] == '\n') && storedDelay > 0 ? textDelay + storedDelay : textDelay) == 0 && CharacterTimer >= 0)
                    {
                        if (Text[textIndex] == ' ')
                        {
                            storedDelay = 0;
                            lockDelay = false;
                        }
                        else
                        {
                            string speaker = DialogueData.DefaultSpeaker;
                            if (DialogueData[currentPage].Speaker != null)
                                speaker = DialogueData[currentPage].Speaker;

                            if (speaker != null)
                                SoundEngine.PlaySound(DialogueSounds[speaker]);
                        }

                        CharacterTimer = 0;
                        ++textIndex;
                    }
                }
                else
                    Crawling = false;
            }
            else
            {
                if (SwitchCounter >= DialogueDisplayUI.Dialogues[Key].DisplayEffects.TimeToDisappear)
                {
                    if (ClosingDialogue || !ProgressDialogue)
                        DialogueDisplaySystem.RemoveDialogue(Key);
                    else
                    {
                        if (++currentPage >= pageCount)
                            DialogueDisplaySystem.RemoveDialogue(Key);
                        else
                        {
                            SwitchingPage = false;
                            SwitchCounter = 0;
                            Activate();
                        }
                    }
                    return;
                }

                SwitchCounter++;
            }

            if (!Crawling)
                Uptime++;
            TextTimer++;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Vector2 textTop = DialogueDisplayUI.Dialogues[Key].DisplayEffects.TextOffsetFromStart(Position, TextSize);
            Vector2 pageTop = textTop - SizeOffsetFromStart;

            DisplayEffects.PreDraw(spriteBatch, pageTop, TextSize, TextTimer, SwitchCounter);

            #region Shadow Drawing
            for (int i = 0; i < textIndex; i++)
            {
                char c = Text[i];

                if (c == '\r' || c == '\n')
                    continue;

                if (CharacterData == null)
                    Activate();

                Vector2 drawPos;
                float rotation = 0f;
                float opacity = 1f;
                Vector2 scale = Vector2.One;
                if (UniqueScales.TryGetValue(i, out Vector2 result))
                    scale = result;

                Color color;
                if (UniqueColors.TryGetValue(i, out var value))
                {
                    Color[] colors = new Color[value.hexcodes.Length];
                    for (int j = 0; j < colors.Length; j++)
                        colors[j] = DialogueDisplaySystem.GetColorFromHex(value.hexcodes[j]);

                    color = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly + (i * value.IndexOffset), colors);
                }
                else
                    color = BaseColor;

                if (CharacterData[i].Timer < DialogueDisplayUI.Dialogues[Key].DisplayEffects.TimeToAppear)
                {
                    drawPos = DialogueDisplayUI.Dialogues[Key].DisplayEffects.AppearPositioning(Position, textTop + CharacterData[i].TextPosition, CharacterData[i].Timer, CharacterData[i]);
                    opacity = DialogueDisplayUI.Dialogues[Key].DisplayEffects.AppearOpacity(opacity, CharacterData[i].Timer, CharacterData[i]);
                    color = DialogueDisplayUI.Dialogues[Key].DisplayEffects.AppearColoring(color, CharacterData[i].Timer, CharacterData[i]);
                    rotation = DialogueDisplayUI.Dialogues[Key].DisplayEffects.AppearRotation(rotation, CharacterData[i].Timer, CharacterData[i]);
                    scale = DialogueDisplayUI.Dialogues[Key].DisplayEffects.AppearScale(scale, CharacterData[i].Timer, CharacterData[i]);
                }
                else
                    drawPos = textTop + CharacterData[i].TextPosition;

                if (SwitchCounter > 0)
                {
                    drawPos = DialogueDisplayUI.Dialogues[Key].DisplayEffects.DisappearPositioning(drawPos, SwitchCounter, CharacterData[i]);
                    opacity = DialogueDisplayUI.Dialogues[Key].DisplayEffects.DisappearOpacity(opacity, SwitchCounter, CharacterData[i]);
                    color = DialogueDisplayUI.Dialogues[Key].DisplayEffects.DisappearColoring(color, SwitchCounter, CharacterData[i]);
                    rotation = DialogueDisplayUI.Dialogues[Key].DisplayEffects.DisappearRotation(rotation, SwitchCounter, CharacterData[i]);
                    scale = DialogueDisplayUI.Dialogues[Key].DisplayEffects.DisappearScale(scale, SwitchCounter, CharacterData[i]);
                }

                if (!ScreenLocked)
                    drawPos -= Main.screenPosition;

                foreach (var l in TextEffects.Where(v => v.Key == i))
                    foreach ((TextEffect Effect, float[] args) in l.Value)
                    {
                        drawPos = Effect.ModifyPos(drawPos, CharacterData[i], args);

                        rotation = Effect.ModifyRot(rotation, CharacterData[i], args);

                        color = Effect.ModifyColor(color, CharacterData[i], args);

                        scale = Effect.ModifyScale(scale, CharacterData[i], args);
                    }

                Color borderColor = color;
                borderColor.R /= 3;
                borderColor.G /= 3;
                borderColor.B /= 3;

                SpriteCharacterData spriteData = Fonts[Font].SpriteCharacters[c];
                Vector2 origin = spriteData.Glyph.Size() * 0.5f;

                CharacterData[i].SetDrawInfo(drawPos, spriteData.Glyph, color * opacity, rotation, scale);

                foreach (var l in TextEffects.Where(v => v.Key == i))
                    foreach ((TextEffect Effect, float[] args) in l.Value)
                        Effect.PreDraw(spriteBatch, spriteData.Texture, CharacterData[i]);

                for (int j = 0; j < ChatManager.ShadowDirections.Length; j++)
                    spriteBatch.Draw(spriteData.Texture, drawPos + (ChatManager.ShadowDirections[j] * 2), spriteData.Glyph, borderColor * opacity, rotation, origin, scale, SpriteEffects.None, 0);
            }
            #endregion

            #region Character Drawing
            for (int i = 0; i < textIndex; i++)
            {
                char c = Text[i];

                if (c == '\r' || c == '\n')
                    continue;

                if (CharacterData == null)
                    Activate();

                SpriteCharacterData spriteData = Fonts[Font].SpriteCharacters[c];
                Vector2 origin = spriteData.Glyph.Size() * 0.5f;

                spriteBatch.Draw(spriteData.Texture, CharacterData[i].DrawPosition, spriteData.Glyph, CharacterData[i].DrawColor, CharacterData[i].Rotation, origin, CharacterData[i].Scale, SpriteEffects.None, 0);

                foreach (var l in TextEffects.Where(v => v.Key == i))
                    foreach ((TextEffect Effect, float[] args) in l.Value)
                        Effect.PostDraw(spriteBatch, spriteData.Texture, CharacterData[i]);

                CharacterData[i].Timer++;
            }
            #endregion

            DisplayEffects.PostDraw(spriteBatch, pageTop, TextSize, TextTimer, SwitchCounter);
        }
    }

    public class DialogueDisplaySystem : ModSystem
    {
        /// <summary>
        /// Checks if the dialogue is active
        /// </summary>
        public static DialogueDisplayUI State;

        public static UserInterface UI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                UI = new();
                State = new();
                State.Activate();
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int preInventory = layers.FindIndex(layer => layer.Name == "Vanilla: Interface Logic 2");
            if (preInventory != -1)
            {
                layers.Insert(preInventory, new LegacyGameInterfaceLayer("Dialogue Display", () =>
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

        public static DialogueTextData Deserialize(string key)
        {
            string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
            string path = "UI/DialogueDisplay/" + activeExtension + "/" + key + ".json";

            // Fall back to english if not found
            if (!CalamityMod.Instance.FileExists(path))
                path = "UI/DialogueDisplay/en-US/" + key + ".json";

            // Throw if we cant find english either
            if (!CalamityMod.Instance.FileExists(path))
                throw new FileNotFoundException($"Could not find the dialog file {path}.");

            Stream stream = CalamityMod.Instance.GetFileStream(path);

            DialogueTextData data = JsonSerializer.Deserialize<DialogueTextData>(stream);

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
        public static void ProgressDialogue(string pearlKey)
        {
            if (DialogueDisplayUI.Dialogues.TryGetValue(pearlKey, out var val))
            {
                if (val.SwitchingPage)
                    return;

                // If the text crawl hasnt finished, finish it instantly
                if (val.textIndex < val.Text.Length - 1)
                    val.textIndex = val.Text.Length - 1;
                // If the text crawl has finished, progress to the next page or finish if we're out of pages
                else
                    val.SwitchingPage = true;
            }
        }

        public static void EndDialogue(string pearlKey)
        {
            if (DialogueDisplayUI.Dialogues.TryGetValue(pearlKey, out var val))
                val.ClosingDialogue = true;
        }

        public static void RemoveDialogue(string key)
        {
            DialogueDisplayUI.DialoguesToRemove.Add(key);
        }

        /// <summary>
        /// Creates a dialogue instance in the world
        /// </summary>
        /// <param name="key">The name of the dialogue's localization key</param>
        /// <param name="startPosition">The position of the text in the world</param>
        public static void StartDialogue(string key, Vector2 startPosition, int Uptime = -1, bool progressDialogue = true, DialogueDisplayEffects effects = null)
        {
            UI ??= new();
            State ??= new();
            effects ??= new DialogueDisplayEffects();

            DialogueDisplay display = new(key, effects)
            {
                Position = startPosition,
                ProgressDialogue = progressDialogue,
            };
            DialogueDisplayUI.Dialogues.Add(key, display);
            if (Uptime != -1)
                DialogueDisplayUI.DialogueUptimes.Add(key, Uptime);
            State.Append(display);
            display.Activate();

            if (UI.CurrentState != State)
                UI?.SetState(State);
        }

        /// <summary>
        /// Creates a dialogue instance in the world
        /// </summary>
        /// <param name="key">The name of the dialogue's localization key</param>
        /// <param name="entity">The entity this dialogue will appear with</param>
        /// <param name="Uptime">The entity this dialogue will appear with</param>
        public static void StartDialogue(string key, Entity entity, int Uptime = -1, DialogueDisplayEffects effects = null)
        {
            UI ??= new();
            State ??= new();
            effects ??= new DialogueDisplayEffects();

            DialogueDisplay display = new(key, effects)
            {
                Position = entity.Center
            };
            DialogueDisplayUI.Dialogues.Add(key, display);
            DialogueDisplayUI.DialogueEntities.Add(key, entity);
            if (Uptime != -1)
                DialogueDisplayUI.DialogueUptimes.Add(key, Uptime);
            State.Append(display);
            display.Activate();

            if (UI.CurrentState != State)
                UI?.SetState(State);
        }

        /// <summary>
        /// Resets all of the dialogue's variables
        /// </summary>
        public static void EndAllDialogue()
        {
            DialogueDisplayUI.Dialogues.Clear();
            State.RemoveAllChildren();
            UI?.SetState(null);
        }
    }

    public class DialogueTextData
    {
        public DialogueLine[] Dialogues { get; set; }
        public DialogueLine this[int index] { get => Dialogues[index]; set => Dialogues[index] = value; }
        public int Length => Dialogues.Length;

        public string DefaultColor { get; set; } = null;
        public string DefaultSpeaker { get; set; } = null;

        public int DefaultScale { get; set; } = 1;

        public int TextDelay { get; set; } = 3;
        public PunctuationData BasePunctuationDelay { get; set; } = new PunctuationData();
        public int PunctuationDelayCap { get; set; } = 30;
        public Dictionary<string, PunctuationData> PunctuationDelays { get; set; } = [];
    }

    public class DialogueLine
    {
        public string[] Lines { get; set; }

        public string BaseColor { get; set; } = null;
        public string Speaker { get; set; } = null;
        public int TextScale { get; set; } = -1;

        public int TextDelay { get; set; } = -1;
        public PunctuationData BasePunctuationDelay { get; set; } = null;
        public int PunctuationDelayCap { get; set; } = -1;
        public Dictionary<string, PunctuationData> PunctuationDelays { get; set; } = null;

        public DialogueEvent Event { get; set; } = null;
    }

    public class PunctuationData
    {
        public int Delay { get; set; } = 10;
        public bool ForceSet { get; set; } = false;
        public bool Locks { get; set; } = false;
    }

    public class DialogueCharacterData(int index, int textLength, int lineNumber)
    {
        public int Timer = 0;

        #region Text Info
        public int Index = index;

        public int TextLength = textLength;

        public int LineNumber = lineNumber;

        public float CompletionRatio => Index / (float)TextLength;

        public Vector2 TextPosition = Vector2.Zero;
        #endregion

        #region Draw Info
        public Vector2 DrawPosition;
        public Rectangle Frame;
        public Color DrawColor;
        public float Rotation ;
        public Vector2 Scale;

        internal void SetDrawInfo(Vector2 drawPos, Rectangle frame, Color color, float rotation, Vector2 scale)
        {
            DrawPosition = drawPos;
            Frame = frame;
            DrawColor = color;
            Rotation = rotation;
            Scale = scale;
        }
        #endregion
    }

    public class DialogueDisplayEffects
    {
        public virtual Vector2 TextOffsetFromStart(Vector2 startPos, Vector2 textSize) => startPos + new Vector2(-textSize.X / 2f, -(textSize.Y + 40));

        public virtual bool FadeWhenTooFar => true;

        public virtual float FadeBuffer => 150f;

        public virtual float FadeDistance => 150f;

        public virtual void PreDraw(SpriteBatch spriteBatch, Vector2 textStart, Vector2 textSize, int textTimer, int switchTimer) { }

        public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 textStart, Vector2 textSize, int textTimer, int switchTimer) { }

        public virtual float TimeToAppear => 30;

        #region Appear Functions
        public virtual Vector2 AppearPositioning(Vector2 startPos, Vector2 goalPos, float time, DialogueCharacterData charData) => Vector2.Lerp(startPos, goalPos, CalamityUtils.SineOutEasing(time / TimeToAppear, 1));

        public virtual Color AppearColoring(Color goalColor, float time, DialogueCharacterData charData) => goalColor;

        public virtual float AppearOpacity(float goalOpacity, float time, DialogueCharacterData charData) => CalamityUtils.SineOutEasing(MathHelper.Clamp(time / 20f, 0f, 1f), 1);

        public virtual float AppearRotation(float goalRotation, float time, DialogueCharacterData charData) => goalRotation;

        public virtual Vector2 AppearScale(Vector2 goalScale, float time, DialogueCharacterData charData) => Vector2.Lerp(Vector2.Zero, goalScale, CalamityUtils.CircOutEasing(time / TimeToAppear, 1));
        #endregion

        public virtual float TimeToDisappear => 30;

        #region Disappear Functions
        public virtual Vector2 DisappearPositioning(Vector2 startPos, float time, DialogueCharacterData charData) => startPos;

        public virtual Color DisappearColoring(Color startColor, float time, DialogueCharacterData charData) => startColor;

        public virtual float DisappearOpacity(float startOpacity, float time, DialogueCharacterData charData) => 1 - CalamityUtils.SineOutEasing(MathHelper.Clamp(time / (TimeToDisappear * 0.66f), 0f, 1f), 1);

        public virtual float DisappearRotation(float startRotation, float time, DialogueCharacterData charData) => startRotation;

        public virtual Vector2 DisappearScale(Vector2 startScale, float time, DialogueCharacterData charData) => Vector2.Lerp(startScale, startScale * 1.5f, CalamityUtils.ExpOutEasing(time / TimeToDisappear, 1));
        #endregion
    }

    #region Display Types
    public class AlwayOnScreen : DialogueDisplayEffects
    {
        Vector2 StartPosition;

        public override bool FadeWhenTooFar => false;

        public override Vector2 TextOffsetFromStart(Vector2 startPos, Vector2 textSize)
        {
            StartPosition = startPos;

            Vector2 playerPos = Main.LocalPlayer.Center;
            Vector2 halfSize = textSize * 0.5f;
            Vector2 newPos = startPos - halfSize + (Vector2.UnitY * -(textSize.Y + 36));
            Vector2 screenPos = newPos.ToScreenPosition();

            Vector2 boundTopLeftScreen = new((Main.screenWidth / 2f) - (Main.screenWidth / 2.5f), (Main.screenHeight / 2f) - (Main.screenHeight / 2.5f));

            if (screenPos.X < boundTopLeftScreen.X)
                newPos.X = playerPos.X - (Main.screenWidth / 2.5f);
            if (screenPos.Y < boundTopLeftScreen.Y)
                newPos.Y = playerPos.Y - (Main.screenHeight / 2.5f);

            if (newPos.X > playerPos.X + (Main.screenWidth / 2.5f) - textSize.X)
                newPos.X = playerPos.X + (Main.screenWidth / 2.5f) - textSize.X;
            if (newPos.Y > playerPos.Y + (Main.screenHeight / 2.5f) - textSize.Y)
                newPos.Y = playerPos.Y + (Main.screenHeight / 2.5f) - textSize.Y;

            return newPos;
        }

        public override void PreDraw(SpriteBatch spriteBatch, Vector2 textTopLeft, Vector2 textSize, int textTimer, int switchTimer)
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/UI/DialogueDisplay/Assets/DialogueArrow").Value;
            Vector2 textCenter = textTopLeft + textSize * 0.5f;
            Vector2 toStart = (StartPosition - textCenter).SafeNormalize(-Vector2.UnitY) * 64;
            spriteBatch.Draw(tex, textCenter + toStart - Main.screenPosition, null, Color.White, toStart.ToRotation(), tex.Size() * 0.5f, 1f, 0, 0);
        }
    }

    public class WackyEffects : DialogueDisplayEffects
    {
        public override Vector2 AppearPositioning(Vector2 startPos, Vector2 goalPos, float time, DialogueCharacterData charData) => Vector2.Lerp(goalPos + Vector2.UnitX.RotatedBy(charData.Index) * 400, goalPos, time / TimeToAppear);

        public override float AppearRotation(float goalRotation, float time, DialogueCharacterData charData) => MathHelper.Lerp(goalRotation + MathHelper.TwoPi * 2, goalRotation, time / TimeToAppear);
    }

    public class WhisperingPearlEffects : DialogueDisplayEffects
    {
        public override void PreDraw(SpriteBatch spriteBatch, Vector2 textStart, Vector2 textSize, int textTimer, int switchTimer)
        {
            if (textTimer < 0)
                return;

            float Opacity = 1f;
            if (textTimer < 30f)
                Opacity = MathHelper.Lerp(0f, 1f, CalamityUtils.CircOutEasing(textTimer / 30f, 1));

            if (switchTimer > 0)
                Opacity *= 1 - CalamityUtils.CircOutEasing(switchTimer / 60f, 1);

            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/SmallBloom").Value;
            spriteBatch.Draw(tex, textStart + textSize * 0.5f - Main.screenPosition, null, Color.Black * 0.6f * Opacity, 0f, tex.Size() * 0.5f, new Vector2(textSize.X / 160f, textSize.Y / 120f), 0, 0);
        }
    }

    #endregion
}
