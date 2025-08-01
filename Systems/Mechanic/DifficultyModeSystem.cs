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
            if (Main.GameMode == GameModeID.Expert && GetCurrentDifficulty == ModContent.GetInstance<DeathDifficulty>())
                ModeIndicatorUI.SwitchToDifficulty(ModContent.GetInstance<RevengeanceDifficulty>());
            if (Main.GameMode == GameModeID.Master && GetCurrentDifficulty == ModContent.GetInstance<RevengeanceDifficulty>())
                ModeIndicatorUI.SwitchToDifficulty(ModContent.GetInstance<DeathDifficulty>());
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

    public abstract class DifficultyMode : ModType
    {
        protected override void Register()
        {
            // This is registered in DifficultyModeSystem.Load
        }
        public abstract bool Enabled
        {
            get; set;
        }

        protected Asset<Texture2D> _texture;
        public abstract Asset<Texture2D> Texture { get; }
        protected Asset<Texture2D> _textureDisabled;
        public abstract Asset<Texture2D> TextureDisabled { get; }
        protected SoundStyle? _activationSound;
        public abstract SoundStyle ActivationSound{ get; }
        internal int _difficultyTier;
        public abstract float DifficultyScale{ get; }

        public new abstract LocalizedText Name { get; }
        public abstract Color ChatTextColor{ get; }
        public abstract LocalizedText ShortDescription{ get; }
        public virtual LocalizedText ExpandedDescription => LocalizedText.Empty;

        public abstract LocalizedText FTWName{ get; }
        public abstract Color FTWTextColor{ get; }



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

        public override Asset<Texture2D> Texture => _texture ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Classic");

        public override Asset<Texture2D> TextureDisabled => _textureDisabled ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Classic_Off");

        public override SoundStyle ActivationSound => _activationSound ??= SoundID.MenuTick with { Volume = 1f };

        public override float DifficultyScale => 0;

        public override LocalizedText Name => Language.GetText("UI.Normal");

        public override Color ChatTextColor => Color.White;

        public override LocalizedText ShortDescription => GetText("UI.ClassicInfo");

        public override LocalizedText FTWName => Language.GetText("UI.Expert");

        public override Color FTWTextColor => new Color(255, 186, 117); // World display: Main.mcColor
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

        public override Asset<Texture2D> Texture => _texture ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Expert");

        public override Asset<Texture2D> TextureDisabled => _textureDisabled ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Expert_Off");

        public override SoundStyle ActivationSound => _activationSound ??= SoundID.ForceRoarPitched;

        public override float DifficultyScale => 0.1f;

        public override LocalizedText Name => Language.GetText("UI.Expert");
        public override Color ChatTextColor => new Color(255, 186, 117); // World display: Main.mcColor

        public override LocalizedText ShortDescription => GetText("UI.ExpertShortInfo");

        public override LocalizedText ExpandedDescription => GetText("UI.ExpertExpandedInfo");

        public override LocalizedText FTWName => Language.GetText("UI.Master");

        public override Color FTWTextColor => new Color(28, 255, 170); // World display: Main.hcColor
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

        public override Asset<Texture2D> Texture => _texture ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Master");
        
        public override Asset<Texture2D> TextureDisabled => _textureDisabled ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Master_Off");

        public override SoundStyle ActivationSound => _activationSound ??= SoundID.NPCDeath10;

        public override float DifficultyScale => 0.25f;

        public override LocalizedText Name => Language.GetText("UI.Master");

        public override Color ChatTextColor => new Color(28, 255, 170); // World display: Main.hcColor

        public override LocalizedText ShortDescription => GetText("UI.MasterShortInfo");

        public override LocalizedText ExpandedDescription => GetText("UI.MasterExpandedInfo");

        public override LocalizedText FTWName => Language.GetText("UI.Legendary");

        public override Color FTWTextColor => Main.legendaryModeColor;

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

        public override Asset<Texture2D> Texture => _texture ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Rev");

        public override Asset<Texture2D> TextureDisabled => _textureDisabled ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Rev_Off");

        public override SoundStyle ActivationSound => _activationSound ??= SoundID.Item119;

        public override float DifficultyScale => 0.25f;

        public override LocalizedText Name => GetText("UI.Revengeance");

        public override Color ChatTextColor => new Color(211, 42, 42);

        public override LocalizedText ShortDescription => GetText("UI.RevengeanceShortInfo");

        public override LocalizedText ExpandedDescription
        {
            get
            {
                string rageKey = "[c/FFCE85:" + CalamityKeybinds.RageHotKey.TooltipHotkeyString() + "]";
                string adrenKey = "[c/79DFBF:" + CalamityKeybinds.AdrenalineHotKey.TooltipHotkeyString() + "]";
                return GetText("UI.RevengeanceExpandedInfo").WithFormatArgs(rageKey, adrenKey);
            }
        }

        public override LocalizedText FTWName => GetText("UI.Death");

        public override Color FTWTextColor => new Color(192, 64, 219);
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

        public override Asset<Texture2D> Texture => _texture ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Death");

        public override Asset<Texture2D> TextureDisabled => _textureDisabled ??= ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicator_Death_Off");

        public override SoundStyle ActivationSound => _activationSound ??= DemonshadeHelm.ActivationSound;

        public override float DifficultyScale => 0.5f;

        public override LocalizedText Name => GetText("UI.Death");

        public override Color ChatTextColor => new Color(192, 64, 219);

        public override LocalizedText ShortDescription => GetText("UI.DeathShortInfo");

        public override LocalizedText ExpandedDescription => GetText("UI.DeathExpandedInfo");


        public override LocalizedText FTWName => GetText("UI.Malice");

        public override Color FTWTextColor => new Color(240, 128, 128);

        public override int[] FavoredDifficultyAtTier(int tier)
        {
            DifficultyMode[] tierList = DifficultyModeSystem.DifficultyTiers[tier];

            List<int> difficulties = new List<int>();

            for (int i = 0; i < tierList.Length; i++)
            {
                if (tierList[i] is MasterDifficulty || tierList[i] is RevengeanceDifficulty)
                    difficulties.Add(i);
            }

            if (difficulties.Count <= 0)
                difficulties.Add(0);

            return difficulties.ToArray();
        }
    }
}
