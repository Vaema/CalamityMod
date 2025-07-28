using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.UI.ModeIndicator;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Systems
{
    public class DifficultyModeSystem : ModSystem
    {
        internal static bool _hasCheckedItOutYet = false; //Simple variable to add a cool effect to the mode selector 

        public static List<DifficultyMode> Difficulties = new List<DifficultyMode>(); //Difficulty modes ordered by ascending difficulty
        public static List<DifficultyMode[]> DifficultyTiers; //Difficulty modes grouped together by difficulty
        public static int MostAlternateDifficulties; //The most alternate difficulties at any tier that exists. Used to know the widest space to take in the ui

        public static FieldInfo journeySliderCacheField; // The current value of the journey mode difficulty slider
        public static MethodInfo journeyDifficultyUpdateMethod; // The method which updates difficulty modes in journey mode

        public override void Load()
        {
            MostAlternateDifficulties = 1;
            //Initialize base mod difficulties
            Difficulties = new List<DifficultyMode>() { new NoDifficulty(), new ExpertDifficulty(), new MasterDifficulty(), new RevengeanceDifficulty(), new DeathDifficulty() };

            // Reflect private journey difficulty slider info
            journeySliderCacheField = typeof(CreativePowers.DifficultySliderPower).GetField("_sliderCurrentValueCache", BindingFlags.Instance | BindingFlags.NonPublic);
            journeyDifficultyUpdateMethod = typeof(CreativePowers.DifficultySliderPower).GetMethod("UpdateInfoFromSliderValueCache", BindingFlags.Instance | BindingFlags.NonPublic);

            CalculateDifficultyData();
        }

        public override void Unload()
        {
            Difficulties = null;
        }

        // Makes the world automatically convert to Death if in Master, or out of Death if in Expert
        public override void PostUpdateWorld()
        {
            if (Main.GameMode == GameModeID.Expert && GetCurrentDifficulty == DeathDifficulty.Instance)
                ModeIndicatorUI.SwitchToDifficulty(RevengeanceDifficulty.Instance);
            if (Main.GameMode == GameModeID.Master && GetCurrentDifficulty == RevengeanceDifficulty.Instance)
                ModeIndicatorUI.SwitchToDifficulty(DeathDifficulty.Instance);
        }

        public static DifficultyMode GetCurrentDifficulty
        {
            get
            {
                DifficultyMode mode = Difficulties[0];

                for (int i = 1; i < Difficulties.Count; i++)
                {
                    if (Difficulties[i].Enabled)
                        mode = Difficulties[i];
                }

                return mode;
            }
        }

        public static void CalculateDifficultyData()
        {
            MostAlternateDifficulties = 1;
            Difficulties = Difficulties.OrderBy(d => d.DifficultyScale).ToList();

            //Difficulties are arranged in "tiers". This is done so that multiple mods can add their own alternate difficulties sharing a tier with the base ones
            DifficultyTiers = new List<DifficultyMode[]>();
            float currentTier = -1;
            int tierIndex = -1;

            for (int i = 0; i < Difficulties.Count; i++)
            {
                //if we are at a new tier, create a new list of difficulties at that tier.
                if (currentTier != Difficulties[i].DifficultyScale)
                {
                    DifficultyTiers.Add(new DifficultyMode[] { Difficulties[i] });
                    currentTier = Difficulties[i].DifficultyScale;
                    tierIndex++;
                }

                //if the tier already exists, just add it to the list of other difficulties at that tier.
                else
                {
                    //ugly
                    DifficultyTiers[tierIndex] = DifficultyTiers[tierIndex].Append(Difficulties[i]).ToArray();
                    MostAlternateDifficulties = Math.Max(DifficultyTiers[tierIndex].Length, MostAlternateDifficulties);
                }

                Difficulties[i]._difficultyTier = tierIndex;
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            //Apparently this is ran after worldgen so it cant always be set to true
            tag["hasCheckedOutTheCoolDifficultyUI"] = _hasCheckedItOutYet;
        }

        public override void OnWorldLoad()
        {
            _hasCheckedItOutYet = false;
        }

        public override void OnWorldUnload()
        {
            _hasCheckedItOutYet = false;
        }

        public override void PostWorldGen()
        {
            _hasCheckedItOutYet = false;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _hasCheckedItOutYet = tag.GetBool("hasCheckedOutTheCoolDifficultyUI");

            //No need to check it out if rev is already on (Such as in old worlds)
            if (CalamityWorld.revenge)
                _hasCheckedItOutYet = true;
        }
    }

    public abstract class DifficultyMode
    {
        public abstract bool Enabled
        {
            get; set;
        }

        public abstract Asset<Texture2D> Texture { get; }
        public abstract Asset<Texture2D> TextureDisabled { get; }
        public virtual LocalizedText ExpandedDescription => LocalizedText.Empty;

        public float DifficultyScale;
        public LocalizedText Name;
        public LocalizedText ShortDescription;
        public Color ChatTextColor;

        public LocalizedText FTWName;
        public Color FTWTextColor;

        public SoundStyle ActivationSound;

        internal int _difficultyTier;

        /// <summary>
        /// Used to know which difficulties to toggle on when selecting a particular difficulty.
        /// </summary>
        public virtual bool RequiresDifficulty(DifficultyMode mode) => false;

        public virtual int[] FavoredDifficultyAtTier(int tier) => [0];
    }

    public class NoDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => true;
            set
                {
                    if (!Main.GameModeInfo.IsJourneyMode)
                    {
                        Main.GameMode = value == true ? GameModeID.Normal : GameModeID.Expert;
                    }
                    else
                    {
                        CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
                        if (power.GetIsUnlocked())
                        {
                            DifficultyModeSystem.journeySliderCacheField.SetValue(power, value == true ? 0.33f : 0.66f);
                            DifficultyModeSystem.journeyDifficultyUpdateMethod.Invoke(power, null);
                        }
                    }
                }
        }

        private Asset<Texture2D> _texture;
        public override Asset<Texture2D> Texture
        {
            get
            {
                if (_texture == null)
                    _texture = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Classic");

                return _texture;
            }
        }
        private Asset<Texture2D> _textureDisabled;
        public override Asset<Texture2D> TextureDisabled
        {
            get
            {
                if (_textureDisabled == null)
                    _textureDisabled = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Classic_Off");

                return _textureDisabled;
            }
        }

        public NoDifficulty()
        {
            DifficultyScale = 0;
            Name = Language.GetText("UI.Normal");
            ShortDescription = GetText("UI.ClassicInfo");
            ChatTextColor = Color.White;

            FTWName = Language.GetText("UI.Expert");
            FTWTextColor = new Color(255, 186, 117); // World display: Main.mcColor

            ActivationSound = SoundID.MenuTick with { Volume = 1f };

            Instance = this;
        }

        public static NoDifficulty Instance { get; private set; } = null;
    }

    public class ExpertDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => Main.getGoodWorld ? Main.masterMode : Main.expertMode;
            set
            {
                if (!Main.GameModeInfo.IsJourneyMode)
                {
                    Main.GameMode = value == true ? GameModeID.Expert : GameModeID.Normal;
                }
                else
                {
                    CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
                    if (power.GetIsUnlocked())
                    {
                        DifficultyModeSystem.journeySliderCacheField.SetValue(power, value == true ? 0.66f : 0.33f);
                        DifficultyModeSystem.journeyDifficultyUpdateMethod.Invoke(power, null);
                    }
                }
            }
        }

        private Asset<Texture2D> _texture;
        public override Asset<Texture2D> Texture
        {
            get
            {
                if (_texture == null)
                    _texture = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Expert");

                return _texture;
            }
        }
        private Asset<Texture2D> _textureDisabled;
        public override Asset<Texture2D> TextureDisabled
        {
            get
            {
                if (_textureDisabled == null)
                    _textureDisabled = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Expert_Off");

                return _textureDisabled;
            }
        }

        public override LocalizedText ExpandedDescription => GetText("UI.ExpertExpandedInfo");

        public ExpertDifficulty()
        {
            DifficultyScale = 0.1f;
            Name = Language.GetText("UI.Expert");
            ShortDescription = GetText("UI.ExpertShortInfo");
            ChatTextColor = new Color(255, 186, 117); // World display: Main.mcColor

            FTWName = Language.GetText("UI.Master");
            FTWTextColor = new Color(28, 255, 170); // World display: Main.hcColor

            ActivationSound = SoundID.ForceRoarPitched;

            Instance = this;
        }

        public static ExpertDifficulty Instance { get; private set; } = null;
    }

    public class MasterDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get =>  Main.getGoodWorld ? CalamityWorld.LegendaryMode : Main.masterMode;
            set
            {
                if (!Main.GameModeInfo.IsJourneyMode)
                {
                    Main.GameMode = value == true ? GameModeID.Master : GameModeID.Expert;
                }
                else
                {
                    CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
                    if (power.GetIsUnlocked())
                    {
                        DifficultyModeSystem.journeySliderCacheField.SetValue(power, value == true ? 1f : 0.66f);
                        DifficultyModeSystem.journeyDifficultyUpdateMethod.Invoke(power, null);
                    }
                }
            }
        }

        private Asset<Texture2D> _texture;
        public override Asset<Texture2D> Texture
        {
            get
            {
                if (_texture == null)
                    _texture = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Master");

                return _texture;
            }
        }
        private Asset<Texture2D> _textureDisabled;
        public override Asset<Texture2D> TextureDisabled
        {
            get
            {
                if (_textureDisabled == null)
                    _textureDisabled = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Master_Off");

                return _textureDisabled;
            }
        }

        public override LocalizedText ExpandedDescription => GetText("UI.MasterExpandedInfo");

        public MasterDifficulty()
        {
            DifficultyScale = 0.25f;
            Name = Language.GetText("UI.Master");
            ShortDescription = GetText("UI.MasterShortInfo");
            ChatTextColor = new Color(28, 255, 170); // World display: Main.hcColor

            FTWName = Language.GetText("UI.Legendary");
            FTWTextColor = Main.legendaryModeColor;

            ActivationSound = SoundID.NPCDeath10;

            Instance = this;
        }

        public static MasterDifficulty Instance { get; private set; } = null;
    }

    public class RevengeanceDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => CalamityWorld.revenge;
            set
            {
                if (Main.getGoodWorld)
                {
                    CalamityWorld.revenge = value;
                    CalamityWorld.death = value;
                }
                else
                    CalamityWorld.revenge = value;
            }
        }

        private Asset<Texture2D> _texture;
        public override Asset<Texture2D> Texture
        {
            get
            {
                if (_texture == null)
                    _texture = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Rev");

                return _texture;
            }
        }
        private Asset<Texture2D> _textureDisabled;
        public override Asset<Texture2D> TextureDisabled
        {
            get
            {
                if (_textureDisabled == null)
                    _textureDisabled = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Rev_Off");

                return _textureDisabled;
            }
        }

        public override LocalizedText ExpandedDescription
        {
            get
            {
                string rageKey = "[c/FFCE85:" + CalamityKeybinds.RageHotKey.TooltipHotkeyString() + "]";
                string adrenKey = "[c/79DFBF:" + CalamityKeybinds.AdrenalineHotKey.TooltipHotkeyString() + "]";
                return GetText("UI.RevengeanceExpandedInfo").WithFormatArgs(rageKey, adrenKey);
            }
        }

        public RevengeanceDifficulty()
        {
            DifficultyScale = 0.25f;
            Name = GetText("UI.Revengeance");
            ShortDescription = GetText("UI.RevengeanceShortInfo");
            ChatTextColor = new Color(211, 42, 42);

            FTWName = GetText("UI.Death");
            FTWTextColor = new Color(192, 64, 219);

            ActivationSound = SoundID.Item119;

            Instance = this;
        }

        public static RevengeanceDifficulty Instance { get; private set; } = null;
    }

    public class DeathDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => Main.getGoodWorld ? CalamityWorld.death && CalamityWorld.LegendaryMode : CalamityWorld.death;
            set
            {
                if (Main.getGoodWorld)
                {
                    if (!Main.GameModeInfo.IsJourneyMode)
                    {
                        Main.GameMode = value == true ? GameModeID.Master : GameModeID.Expert;
                    }
                    else
                    {
                        CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
                        if (power.GetIsUnlocked())
                        {
                            DifficultyModeSystem.journeySliderCacheField.SetValue(power, value == true ? 1f : 0.66f);
                            DifficultyModeSystem.journeyDifficultyUpdateMethod.Invoke(power, null);
                        }
                    }
                    CalamityWorld.death = value;
                }
                else
                    CalamityWorld.death = value;
            }
        }

        private Asset<Texture2D> _texture;
        public override Asset<Texture2D> Texture
        {
            get
            {
                if (_texture == null)
                    _texture = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Death");

                return _texture;
            }
        }
        private Asset<Texture2D> _textureDisabled;
        public override Asset<Texture2D> TextureDisabled
        {
            get
            {
                if (_textureDisabled == null)
                    _textureDisabled = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Death_Off");

                return _textureDisabled;
            }
        }

        public override LocalizedText ExpandedDescription => GetText("UI.DeathExpandedInfo");

        public DeathDifficulty()
        {
            DifficultyScale = 0.5f;
            Name = GetText("UI.Death");
            ShortDescription = GetText("UI.DeathShortInfo");
            ChatTextColor = new Color(192, 64, 219);

            FTWName = GetText("UI.Malice");
            FTWTextColor = new Color(240, 128, 128);

            ActivationSound = DemonshadeHelm.ActivationSound;

            Instance = this;
        }

        public override int[] FavoredDifficultyAtTier(int tier)
        {
            DifficultyMode[] tierList = DifficultyModeSystem.DifficultyTiers[tier];

            List<int> difficulties = new List<int>();

            for (int i = 0; i < tierList.Length; i++)
            {
                if (tierList[i].Name == GetText("UI.Master") || tierList[i].Name == GetText("UI.Revengeance"))
                    difficulties.Add(i);
            }

            if (difficulties.Count <= 0)
                difficulties.Add(0);

            return difficulties.ToArray();
        }
        public static DeathDifficulty Instance { get; private set; } = null;
    }
}
