using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.UI.ModeIndicator;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
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

        public override void Load()
        {
            MostAlternateDifficulties = 1;
            //Initialize base mod difficulties
            Difficulties = new List<DifficultyMode>() { new NoDifficulty(), new ExpertDifficulty(), new MasterDifficulty(), new RevengeanceDifficulty(), new DeathDifficulty() };

            CalculateDifficultyData();
        }

        public override void Unload()
        {
            Difficulties = null;
        }

        // Makes the world automatically convert to Death if in Master, or out of Death if in Expert
        public override void PostUpdateWorld()
        {
            if (Main.expertMode && !Main.masterMode && GetCurrentDifficulty == DeathDifficulty.Instance)
                ModeIndicatorUI.SwitchToDifficulty(RevengeanceDifficulty.Instance);
            if (Main.masterMode && GetCurrentDifficulty == RevengeanceDifficulty.Instance)
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

        public SoundStyle ActivationSound;

        public Color ChatTextColor;

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
            set => Main.GameMode = value == true ? GameModeID.Normal : GameModeID.Expert;
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
            Name = GetText("UI.Classic");
            ShortDescription = GetText("UI.ClassicInfo");

            ActivationSound = SoundID.MenuTick with { Volume = 1f };

            ChatTextColor = Color.White;
            Instance = this;
        }

        public static NoDifficulty Instance { get; private set; } = null;
    }

    public class ExpertDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => Main.expertMode;
            set => Main.GameMode = value == true ? GameModeID.Expert : GameModeID.Normal;
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
            Name = GetText("UI.Expert");
            ShortDescription = GetText("UI.ExpertShortInfo");

            ActivationSound = SoundID.ForceRoarPitched;

            ChatTextColor = new Color(255, 186, 117);
            Instance = this;
        }

        public static ExpertDifficulty Instance { get; private set; } = null;
    }

    public class MasterDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => Main.masterMode;
            set => Main.GameMode = value == true ? GameModeID.Master : GameModeID.Expert;
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
            Name = GetText("UI.Master");
            ShortDescription = GetText("UI.MasterShortInfo");

            ActivationSound = SoundID.NPCDeath10;

            ChatTextColor = new Color(28, 255, 170);
            Instance = this;
        }

        public static MasterDifficulty Instance { get; private set; } = null;
    }

    public class RevengeanceDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => CalamityWorld.revenge;
            set => CalamityWorld.revenge = value;
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

            ActivationSound = SoundID.Item119;

            ChatTextColor = new Color(211, 42, 42);
            Instance = this;
        }

        public static RevengeanceDifficulty Instance { get; private set; } = null;
    }

    public class DeathDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => CalamityWorld.death;
            set => CalamityWorld.death = value;
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

            ActivationSound = DemonshadeHelm.ActivationSound;

            ChatTextColor = new Color(192, 64, 219);
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
