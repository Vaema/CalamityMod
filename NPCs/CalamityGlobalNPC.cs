using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CalamityMod.Balancing;
using CalamityMod.BiomeManagers;
using CalamityMod.Buffs;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Buffs.Summon.Whips;
using CalamityMod.CalPlayer;
using CalamityMod.DataStructures;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.ExtraTextures;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Graphics.Renderers.CalamityRenderers;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.Armor.PlagueReaper;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Items.Tools;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.Astral;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.DraedonLabThings;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;
using CalamityMod.Packets;
using CalamityMod.Particles;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Systems;
using CalamityMod.Systems.Collections;
using CalamityMod.Tiles.FurnitureAuric;
using CalamityMod.Tiles.Ores;
using CalamityMod.UI;
using CalamityMod.UI.DebuffSystem;
using CalamityMod.Walls.DraedonStructures;
using CalamityMod.World;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.UI.Chat;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs
{
    public partial class CalamityGlobalNPC : GlobalNPC
    {
        #region Variables

        /// <summary> Data structure used for storing Calamity's intended boss kill times. </summary>
        public static SortedDictionary<int, int> BossKillTimes;

        /// <summary> Data structure used for storing the damage reduction values of NPCs. </summary>
        public static SortedDictionary<int, float> DRValues { get; set; }

        /// <summary> Damage Reduction Value </summary>
        public float DR { get; set; } = 0f;

        /// <summary> If set to true, the NPC's damage reduction cannot be reduced via any means. This applies regardless of whether <see cref="customDR"/> is true or false. </summary>
        public bool unbreakableDR = false;

        /// <summary>
        /// Overrides the normal DR math and uses custom DR reductions for each debuff, registered separately.<br/>
        /// Current only used by Old Duke.
        /// </summary>
        public bool customDR = false;
        public Dictionary<int, float> flatDRReductions = new Dictionary<int, float>();
        public Dictionary<int, float> multDRReductions = new Dictionary<int, float>();

        public int KillTime { get; set; } = 0;

        /// <summary>
        /// Controls the effectiveness of heat debuffs against this NPC.<br/>
        /// If true, they are vulnerable and debuffs are 200% effective. If false, they are resistant and debuffs are 50% effective. If null, they are neutral and suffer standard effects.
        /// </summary>
        public bool? VulnerableToHeat = null;
        /// <summary>
        /// Controls the effectiveness of cold debuffs against this NPC.<br/>
        /// If true, they are vulnerable and debuffs are 200% effective. If false, they are resistant and debuffs are 50% effective. If null, they are neutral and suffer standard effects.
        /// </summary>
        public bool? VulnerableToCold = null;
        /// <summary>
        /// Controls the effectiveness of sickness debuffs against this NPC.<br/>
        /// If true, they are vulnerable and debuffs are 200% effective. If false, they are resistant and debuffs are 50% effective. If null, they are neutral and suffer standard effects.
        /// </summary>
        public bool? VulnerableToSickness = null;
        /// <summary>
        /// Controls the effectiveness of electricity debuffs against this NPC.<br/>
        /// If true, they are vulnerable and debuffs are 200% effective. If false, they are resistant and debuffs are 50% effective. If null, they are neutral and suffer standard effects.
        /// </summary>
        public bool? VulnerableToElectricity = null;
        /// <summary>
        /// Controls the effectiveness of water debuffs against this NPC.<br/>
        /// If true, they are vulnerable and debuffs are 200% effective. If false, they are resistant and debuffs are 50% effective. If null, they are neutral and suffer standard effects.
        /// </summary>
        public bool? VulnerableToWater = null;

        public const float BaseDoTDamageMult = 1f;
        public const float VulnerableToDoTDamageMult = 2f;
        public const float VulnerableToDoTDamageMult_Worms_SlimeGod = 1.5f;
        public const float ResistantToDoTDamageMult = 0.5f;

        public StatModifier TypelessDebuffMultiplier = new StatModifier();
        public StatModifier HeatDebuffMultiplier = new StatModifier();
        public StatModifier ColdDebuffMultiplier = new StatModifier();
        public StatModifier SicknessDebuffMultiplier = new StatModifier();
        public StatModifier WaterDebuffMultiplier = new StatModifier();
        public StatModifier ElectricDebuffMultiplier = new StatModifier();

        // These are all recalculated constantly, while the regular ones are recalulated only on hit
        public StatModifier ActiveTypelessDebuffMultiplier = new StatModifier();
        public StatModifier ActiveHeatDebuffMultiplier = new StatModifier();
        public StatModifier ActiveColdDebuffMultiplier = new StatModifier();
        public StatModifier ActiveSicknessDebuffMultiplier = new StatModifier();
        public StatModifier ActiveWaterDebuffMultiplier = new StatModifier();
        public StatModifier ActiveElectricDebuffMultiplier = new StatModifier();

        // Cold debuff effects
        public bool IncreasedColdEffects_EskimoSet = false;
        public bool IncreasedColdEffects_CryoStone = false;

        // Electric effects
        public bool IncreasedElectricityEffects_Unused = false;

        // Heat debuff effects
        public bool IncreasedHeatEffects_Fireball = false;
        public bool IncreasedHeatEffects_CinnamonRoll = false;
        public int IncreasedHeatEffects_FireBoots = 0;

        // Toxic Heart effect
        public bool IncreasedSicknessEffects_ToxicHeart = false;

        // Amulets effects
        public bool IncreasedWaterEffects_Amulet1 = false;
        public bool IncreasedWaterEffects_Amulet2 = false;

        // Sickness and Water debuff effects
        public bool IncreasedSicknessAndWaterEffects_EvergreenGin = false;
        public bool IncreasedSicknessAndWaterEffects_CorrosiveSpine = false;

        // Universal debuff effects
        public bool IncreasedDebuffEffects_Amalgam = false;

        /// <summary> Constant variable representing the grace period, in frames, in which a boss can remain outside of its native biome before enraging. </summary>
        public const int biomeEnrageTimerMax = 300;

        /// <summary>
        /// Variable for worm bosses used to prevent them from moving too fast upon swapping phases while far away from their target.<br/>
        /// Currently only used by DoG.
        /// </summary>
        public float velocityPriorToPhaseSwap = 0f;
        public const float velocityPriorToPhaseSwapIncrement = 0.1f;

        /// <summary> Allows hostile NPCs to deal defense damage to the player, used mostly for hard-hitting bosses. </summary>
        public bool canBreakPlayerDefense = false;

        /// <summary> Set this value to reduce target defense by a flat amount. </summary>
        public int miscDefenseLoss = 0;

        /// <summary>
        /// Constant representing a distance of 200 tiles in pixel measurement.<br/>
        /// Used by bosses to increase their velocity in order to catch up to their target.
        /// </summary>
        public const float CatchUpDistance200Tiles = 3200f;
        /// <summary>
        /// Constant representing a distance of 350 tiles in pixel measurement.<br/>
        /// Used by bosses to increase their velocity in order to catch up to their target.
        /// </summary>
        public const float CatchUpDistance350Tiles = 5600f;
        /// <summary>
        /// Constant representing a distance of 400 tiles in pixel measurement.<br/>
        /// Used as a cap on the distance away from a boss a player can be inflicted with Boss Effects.
        /// </summary>
        private const float BossZenDistance = 6400f;

        /// <summary>
        /// Destroyer laser colors, used for telegraphs.<br/>
        /// None = -1, Red = 0, Green = 1, Cyan = 2
        /// </summary>
        public int destroyerLaserColor = -1;

        /// <summary> Constant multiplier used to decrease the health and/or damage of pre-Hardmode Desert enemies. </summary>
        private const double DesertEnemyStatMultiplier = 0.75;

        /// <summary> Constant multiplier used for decreasing the health and damage of mechanical bosses if the Early Hardmode Progression Rework config is enabled. </summary>
        public const double EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic = 0.8;
        /// <summary> <inheritdoc cref="EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic" /> </summary>
        public const double EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic = 0.9;
        /// <summary> <inheritdoc cref="EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic" /> </summary>
        public const double EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert = 0.9;
        /// <summary> <inheritdoc cref="EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic" /> </summary>
        public const double EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert = 0.95;

        /// <summary> Constant multiplier used to increase coin drops in Classic Mode. </summary>
        private const double NPCValueMultiplier_ClassicCalamity = 1.5;
        /// <summary> Constant multiplier used to decrease coin drops in Expert Mode. </summary>
        private const double NPCValueMultiplier_ExpertVanilla = 2.5;
        /// <summary> <inheritdoc cref="NPCValueMultiplier_ExpertVanilla"/> </summary>
        private const double NPCValueMultiplier_ExpertCalamity = 1.5;

        // Dash damage immunity timer
        public const int maxPlayerImmunities = Main.maxPlayers + 1;
        public int[] dashImmunityTime = new int[maxPlayerImmunities];

        /// <summary> Used to control the animation of the Town NPC Shop Alert icon, if the respective config is enabled. </summary>
        public int shopAlertAnimTimer = 0;
        /// <summary> <inheritdoc cref="shopAlertAnimTimer"/> </summary>
        public int shopAlertAnimFrame = 0;

        /// <summary>
        /// If set to false, prevents this NPC from allowing Rage to be generated by nearby players, regardless of other factors.<br/>
        /// Defaults to true.
        /// </summary>
        public bool ProvidesProximityRage = true;

        // NewAI
        internal const int maxAIMod = 4;
        public float[] newAI = new float[maxAIMod];
        public int AITimer = 0;

        /// <summary> Used for allowing Patreon names for Town NPCs. </summary>
        public bool setNewName = true;

        /// <summary> If set to true, the Boss Health Bar for this NPC will count the total health of all individual segments using worm segment logic. </summary>
        public bool SplittingWorm = false;
        /// <summary> If set to true, allows this NPC to draw a Boss Health Bar, regardless of other factors. </summary>
        public bool CanHaveBossHealthBar = false;
        /// <summary> If set to true, allows for manually disabling this NPC's Boss Health Bar, even if they are still active. </summary>
        public bool ShouldCloseHPBar = false;

        /// <summary> Constant representing the cooldown, in frames, before a boss can be affected by a slowing debuff. </summary>
        public const int slowingDebuffResistanceMin = 1800;
        /// <summary> Tracks the current slowing debuff cooldown for this NPC. </summary>
        public int debuffResistanceTimer = 0;

        #region Debuffs
        public bool vaporfied = false;
        public bool timeDistortion = false;
        public bool glacialState = false;
        public bool galvanicCorrosion = false;
        public bool temporalSadness = false;
        public bool eutrophication = false;
        public bool webbed = false;
        public bool electrified = false;
        public bool pearlAura = false;

        public float manaBurn = 0f;
        public float manaBurnPeak = 0f;
        public float playerManaBurnIntensity = 0f;
        /// <summary>
        /// Counter variable that increments while the NPC is inflicted with Pearl Aura.<br/>
        /// Used to determine when Giant Pearl's pearl shards should rain down onto the NPC.
        /// </summary>
        public int pearlAuraCounter = 0;
        /// <summary>
        /// When an NPC is inflicted with Pearl Aura, this variable is set to index of the player who inflicted it.<br/>
        /// Used for properly counting pearl shard amount and for giving pearl shards an owner.
        /// </summary>
        public int pearlAuraOwner = -1;
        public bool burningBlood = false;
        public bool brainRot = false;
        public bool heavyBleeding = false;
        public bool laceration = false;
        public bool elementalMix = false;
        public bool markedForDeath = false;
        public bool absorberAffliction = false;
        public bool irradiated = false;
        public double irradiatedContactBoost = 1.5;
        public bool brimstoneFlames = false;
        public bool demonicFlames = false;
        public int demonicFlamesBonusDamage = 0;
        public bool holyFlames = false;
        public bool plague = false;
        public bool armorCrunch = false;
        public bool crumble = false;

        public int antlionCloudDebuffTimer = 0;
        public bool scionsCurioEffected = false;
        public int warbannerBurnTime = 0; // Determines the rate that the enemy is damaged
        public int warbannerBurnTimer = 0; // The duration of the debuff
        public int warbannerBurnStacks = 0; // The stacks increase how fast the debuff hits
        public int warbannerBurnDamage = 0; // Damage of the hits based on player's damage
        public Vector2 warbannerBurnDirection;
        public float warbannerBurnIntensity = 0;
        public bool warbannerBurnMarked = false;
        public bool warbannerBurnHideEffects = false;
        /// <summary> Constant variable representing the delay, in frames, before Verium Bolt's extra damage applies. </summary>
        public const int veriumDoomTime = 90;
        public int veriumDoomTimer = 0;
        public int veriumDoomStacks = 0;
        public bool veriumDoomMarked = false;

        public bool laserBurnMarked = false;
        /// <summary>
        /// The type of laser burn that this NPC is inflicted with.<br/>
        /// When set to 1, applies all accrued damage in a single hit. When set to 2, deals constant flat damage + extra flat damage from stacks.
        /// </summary>
        public int laserBurnType = 0;
        public int laserBurnDamage = 0; // Only used if laser burn type is 1
        public const int laserBurnTime = 300;
        public int laserBurnTimer = 0;
        public int laserBurnStacks = 0;

        public bool hyperiusMarked = false;
        public int hyperiusDamage = 0;
        public static int hyperiusOverflowTime = 100;
        public int hyperiusOverflowTimer = hyperiusOverflowTime;
        /// <summary> Constant variable representing the % of max health Hyperius Bullet's damage stacks must reach before they start to overflow. </summary>
        public const float hyperiusLifePercentThreshold = 0.07f;
        public int hyperiusFxTimer = 0;

        /// <summary>
        /// Tracks the strength of Calamity's cursor effect; increments by 2 on every frame.<br/>
        /// If this value reaches <see cref="cursorFocusMax"/>, the enemy is afflicted with True Vulnerability Hex.
        /// </summary>
        public int cursorFocus = 0;
        public const int cursorFocusMax = 300;
        public int demonSwordImpales = 0;
        public int impalePacketTimer = 0;

        /// <summary>
        /// If set to true, prevents this NPC from dealing contact damage.<br/>
        /// Used by Septic Skewer's execution attack.
        /// </summary>
        public bool pacified = false;

        // Soma Prime Shred deals damage with DirectStrikes instead of with direct debuff damage
        // It also stacks, scales with ranged damage, and can crit, meaning it needs to know who applied it most recently
        /// <summary> Tracks how many stacks of the Shred debuff this NPC is inflicted with. </summary>
        public int somaShredStacks = 0;
        /// <summary> Tracks the index of the player that inflicted this NPC with Shred, for the purpose of scaling damage. </summary>
        public int somaShredApplicator = -1;
        /// <summary> Counter used for removing stacks of Shred. The number of stacks is subtracted every frame, and when it hits zero, it is reset and one stack is removed. </summary>
        public int somaShredFalloff = Shred.StackFalloffFrames;

        public bool crushDepth = false;
        public bool riptide = false;
        public bool hadopelagicPressure = false;
        public bool godSlayerInferno = false;
        public bool dragonFire = false;
        public bool vermillionFlux = false;
        public bool auricRebuke = false;
        public bool staticDischarge = false;
        public bool miracleBlight = false;
        public bool astralInfection = false;
        public bool whisperingDeath = false;
        public bool nightwither = false;
        /// <summary> If greater than 0, this NPC has been "shocked" by Ilmeris' Spark's on hurt effect. </summary>
        public int shocked = 0;
        public bool voidfrost = false;
        public bool shellfishStaffDebuff = false;
        public bool snapClamDebuff = false;
        public bool sulphurPoison = false;
        /// <summary> If greater than 0, makes this NPC constantly spawn heart gores. </summary>
        public int ladHearts = 0;
        public bool relicOfResilienceWeakness = false;
        public bool sagePoison = false;
        public int sagePoisonDamage = 0;
        public bool vulnerabilityHex = false;
        public bool trueVulnerabilityHex = false;
        public bool banishingFire = false;
        public bool wither = false;
        /// <summary>
        /// If greater than 0, this enemy will appear to disintegrate into ash when killed.<br/>
        /// Used by Rancor's laser beam.
        /// </summary>
        public int ashesOnDeath = 0;
        #endregion

        // whoAmI Variables
        public static int[] bobbitWormBottom = new int[5];
        public static int hiveMind = -1;
        public static int perfHive = -1;
        public static int slimeGodPurple = -1;
        public static int slimeGodRed = -1;
        public static int slimeGod = -1;
        public static int laserEye = -1;
        public static int fireEye = -1;
        public static int primeLaser = -1;
        public static int primeCannon = -1;
        public static int primeVice = -1;
        public static int primeSaw = -1;
        public static int aquaticScourge = -1;
        public static int brimstoneElemental = -1;
        public static int cataclysm = -1;
        public static int catastrophe = -1;
        public static int calamitas = -1;
        public static int LeviAndAna = -1;
        public static int leviathan = -1;
        public static int siren = -1;
        public static int astrumAureus = -1;
        public static int scavenger = -1;
        public static int energyFlame = -1;
        public static int doughnutBoss = -1;
        public static int doughnutBossDefender = -1;
        public static int doughnutBossHealer = -1;
        public static int holyBossAttacker = -1;
        public static int holyBossDefender = -1;
        public static int holyBossHealer = -1;
        public static int holyBoss = -1;
        public static int voidBoss = -1;
        public static int signus = -1;
        public static int ghostBossClone = -1;
        public static int ghostBoss = -1;
        public static int DoGHead = -1;
        public static int DoGP2 = -1;
        public static int yharon = -1;
        public static int yharonP2 = -1;
        public static int SCalCataclysm = -1;
        public static int SCalCatastrophe = -1;
        public static int SCal = -1;
        public static int SCalWorm = -1;
        public static int SCalGrief = -1;
        public static int SCalLament = -1;
        public static int SCalEpiphany = -1;
        public static int SCalAcceptance = -1;
        public static int draedon = -1;
        public static int draedonAmbience = -1;
        public static int draedonExoMechWorm = -1;
        public static int draedonExoMechTwinRed = -1;
        public static int draedonExoMechTwinGreen = -1;
        public static int draedonExoMechPrime = -1;
        public static int draedonExoMechPrimePlasmaCannon = -1;
        public static int adultEidolonWyrmHead = -1;

        // Drawing variables.
        public FireParticleSet VulnerabilityHexFireDrawer = null;
        public FireParticleSet ManaBurnFireDrawer = null;

        /// <summary>
        /// Boss Enrage variable for use with the boss health UI.<br/>
        /// The logic behind this is as follows:
        /// <para>1 - For special cases with super-enrages (specifically Yharon/SCal with their arenas), go solely based on whether that enrage is active. That information is most important to the player.</para>
        /// <para>2 - Check if the Demonshade enrage is active. If it is, register this as true. If not, go to step 3.</para>
        /// <para>3 - Check if a specific enrage condition (such as Duke Fishron's Ocean check) is met. If it is, and Boss Rush is not active, set this to true. If not, go to step 4.</para>
        /// <para>4 - Check if Boss Rush isn't active. If so, set this to true.</para>
        /// </summary>
        public bool CurrentlyEnraged;

        /// <summary>
        /// Increased defense or DR variable for use with the boss health UI.<br/>
        /// The logic behind this is as follows:
        /// <para>1 - When bosses are transitioning phases they gain a massive DR increase.</para>
        /// <para>2 - When bosses are using certain attacks that make them particularly vulnerable they gain a massive DR or defense increase.</para>
        /// While either of these are occuring, this variable should be set to true.
        /// </summary>
        public bool CurrentlyIncreasingDefenseOrDR;

        /// <summary> If set to true, this NPC will be ignored by Boss Rush's whitelist and will always be allowed to exist. </summary>
        public bool DoesNotDisappearInBossRush;

        /// <summary> Variable used for Gladiator's Locket's on-kill effect to ensure it only triggers once per kill. </summary>
        public bool gladiatorOnKill = true;
        /// <summary> Cooldown variable for Unstable Granite Core's arc zap effect. </summary>
        public int arcZapCooldown = 0;

        /// <summary> Timer for animating worm enemies in the bestiary. </summary>
        public float bestiaryWormTimer = 0;
        #endregion

        #region Instance Per Entity and TML 1.4 Cloning
        public override bool InstancePerEntity => true;

        // Ozzatron 25APR2022: This function was required by TML 1.4's new clone behavior,
        // which broke every custom NPC in the game simultaneously when it was introduced.
        // It manually copies everything because I don't trust the base clone behavior after seeing the insane bugs.
        // Considering the continuing revisions to Entity cloning, it's possible that this is no longer needed.
        // Don't risk it and don't remove this code unless it's clear that it is causing problems.
        //
        // ANY TIME YOU ADD A VARIABLE TO CalamityGlobalNPC, IT MUST BE COPIED IN THIS FUNCTION.
        public override GlobalNPC Clone(NPC npc, NPC npcClone)
        {
            CalamityGlobalNPC myClone = (CalamityGlobalNPC)base.Clone(npc, npcClone);

            myClone.DR = DR;
            myClone.unbreakableDR = unbreakableDR;
            myClone.flatDRReductions = new Dictionary<int, float>();
            foreach (var flatDR in flatDRReductions)
                myClone.flatDRReductions.Add(flatDR.Key, flatDR.Value);
            myClone.multDRReductions = new Dictionary<int, float>();
            foreach (var multDR in multDRReductions)
                myClone.multDRReductions.Add(multDR.Key, multDR.Value);

            myClone.KillTime = KillTime;

            myClone.VulnerableToHeat = VulnerableToHeat;
            myClone.VulnerableToCold = VulnerableToCold;
            myClone.VulnerableToSickness = VulnerableToSickness;
            myClone.VulnerableToElectricity = VulnerableToElectricity;
            myClone.VulnerableToWater = VulnerableToWater;

            myClone.IncreasedColdEffects_EskimoSet = IncreasedColdEffects_EskimoSet;
            myClone.IncreasedColdEffects_CryoStone = IncreasedColdEffects_CryoStone;
            myClone.IncreasedElectricityEffects_Unused = IncreasedElectricityEffects_Unused;
            myClone.IncreasedHeatEffects_Fireball = IncreasedHeatEffects_Fireball;
            myClone.IncreasedHeatEffects_CinnamonRoll = IncreasedHeatEffects_CinnamonRoll;
            myClone.IncreasedHeatEffects_FireBoots = IncreasedHeatEffects_FireBoots;
            myClone.IncreasedSicknessEffects_ToxicHeart = IncreasedSicknessEffects_ToxicHeart;
            myClone.IncreasedWaterEffects_Amulet1 = IncreasedWaterEffects_Amulet1;
            myClone.IncreasedWaterEffects_Amulet2 = IncreasedWaterEffects_Amulet2;
            myClone.IncreasedSicknessAndWaterEffects_CorrosiveSpine = IncreasedSicknessAndWaterEffects_CorrosiveSpine;
            myClone.IncreasedSicknessAndWaterEffects_EvergreenGin = IncreasedSicknessAndWaterEffects_EvergreenGin;
            myClone.IncreasedDebuffEffects_Amalgam = IncreasedDebuffEffects_Amalgam;

            myClone.velocityPriorToPhaseSwap = velocityPriorToPhaseSwap;

            myClone.canBreakPlayerDefense = canBreakPlayerDefense;

            myClone.miscDefenseLoss = miscDefenseLoss;

            myClone.destroyerLaserColor = destroyerLaserColor;

            myClone.dashImmunityTime = new int[maxPlayerImmunities];
            for (int i = 0; i < maxPlayerImmunities; ++i)
                myClone.dashImmunityTime[i] = dashImmunityTime[i];

            myClone.shopAlertAnimTimer = shopAlertAnimTimer;
            myClone.shopAlertAnimFrame = shopAlertAnimFrame;

            myClone.ProvidesProximityRage = ProvidesProximityRage;

            myClone.newAI = new float[maxAIMod];
            for (int i = 0; i < maxAIMod; ++i)
                myClone.newAI[i] = newAI[i];
            myClone.AITimer = AITimer;

            myClone.setNewName = setNewName;

            myClone.SplittingWorm = SplittingWorm;
            myClone.CanHaveBossHealthBar = CanHaveBossHealthBar;
            myClone.ShouldCloseHPBar = ShouldCloseHPBar;

            myClone.debuffResistanceTimer = debuffResistanceTimer;

            myClone.vaporfied = vaporfied;
            myClone.timeDistortion = timeDistortion;
            myClone.glacialState = glacialState;
            myClone.galvanicCorrosion = galvanicCorrosion;
            myClone.temporalSadness = temporalSadness;
            myClone.eutrophication = eutrophication;
            myClone.webbed = webbed;
            myClone.electrified = electrified;
            myClone.pearlAura = pearlAura;
            myClone.pearlAuraCounter = pearlAuraCounter;
            myClone.burningBlood = burningBlood;
            myClone.brainRot = brainRot;
            myClone.heavyBleeding = heavyBleeding;
            myClone.laceration = laceration;
            myClone.elementalMix = elementalMix;
            myClone.markedForDeath = markedForDeath;
            myClone.absorberAffliction = absorberAffliction;
            myClone.irradiated = irradiated;
            myClone.irradiatedContactBoost = irradiatedContactBoost;
            myClone.brimstoneFlames = brimstoneFlames;
            myClone.demonicFlames = demonicFlames;
            myClone.demonicFlamesBonusDamage = demonicFlamesBonusDamage;
            myClone.holyFlames = holyFlames;
            myClone.plague = plague;
            myClone.armorCrunch = armorCrunch;
            myClone.crumble = crumble;

            myClone.antlionCloudDebuffTimer = antlionCloudDebuffTimer;
            myClone.scionsCurioEffected = scionsCurioEffected;
            myClone.warbannerBurnTime = warbannerBurnTime;
            myClone.warbannerBurnTimer = warbannerBurnTimer;
            myClone.warbannerBurnStacks = warbannerBurnStacks;
            myClone.warbannerBurnDamage = warbannerBurnDamage;
            myClone.warbannerBurnDirection = warbannerBurnDirection;
            myClone.warbannerBurnIntensity = warbannerBurnIntensity;
            myClone.warbannerBurnMarked = warbannerBurnMarked;
            myClone.warbannerBurnHideEffects = warbannerBurnHideEffects;
            myClone.veriumDoomTimer = veriumDoomTimer;
            myClone.veriumDoomStacks = veriumDoomStacks;
            myClone.veriumDoomMarked = veriumDoomMarked;
            myClone.laserBurnDamage = laserBurnDamage;
            myClone.laserBurnMarked = laserBurnMarked;
            myClone.laserBurnStacks = laserBurnStacks;
            myClone.laserBurnTimer = laserBurnTimer;
            myClone.laserBurnType = laserBurnType;
            myClone.hyperiusDamage = hyperiusDamage;
            myClone.hyperiusMarked = hyperiusMarked;
            myClone.hyperiusOverflowTimer = hyperiusOverflowTimer;
            myClone.hyperiusFxTimer = hyperiusFxTimer;
            myClone.cursorFocus = cursorFocus;
            myClone.demonSwordImpales = demonSwordImpales;
            myClone.impalePacketTimer = impalePacketTimer;

            myClone.pacified = pacified;

            myClone.somaShredStacks = somaShredStacks;
            myClone.somaShredApplicator = somaShredApplicator;
            myClone.somaShredFalloff = somaShredFalloff;

            myClone.crushDepth = crushDepth;
            myClone.riptide = riptide;
            myClone.hadopelagicPressure = hadopelagicPressure;
            myClone.godSlayerInferno = godSlayerInferno;
            myClone.miracleBlight = miracleBlight;
            myClone.dragonFire = dragonFire;
            myClone.vermillionFlux = vermillionFlux;
            myClone.auricRebuke = auricRebuke;
            myClone.staticDischarge = staticDischarge;
            myClone.astralInfection = astralInfection;
            myClone.whisperingDeath = whisperingDeath;
            myClone.nightwither = nightwither;
            myClone.shocked = shocked;
            myClone.voidfrost = voidfrost;
            myClone.shellfishStaffDebuff = shellfishStaffDebuff;
            myClone.snapClamDebuff = snapClamDebuff;
            myClone.sulphurPoison = sulphurPoison;
            myClone.ladHearts = ladHearts;
            myClone.relicOfResilienceWeakness = relicOfResilienceWeakness;
            myClone.sagePoison = sagePoison;
            myClone.sagePoisonDamage = sagePoisonDamage;
            myClone.vulnerabilityHex = vulnerabilityHex;
            myClone.trueVulnerabilityHex = trueVulnerabilityHex;
            myClone.banishingFire = banishingFire;
            myClone.wither = wither;
            myClone.ashesOnDeath = ashesOnDeath;

            // This gets set up as needed.
            myClone.VulnerabilityHexFireDrawer = null;
            myClone.ManaBurnFireDrawer = null;

            myClone.CurrentlyEnraged = CurrentlyEnraged;

            myClone.CurrentlyIncreasingDefenseOrDR = CurrentlyIncreasingDefenseOrDR;

            myClone.DoesNotDisappearInBossRush = DoesNotDisappearInBossRush;

            return myClone;
        }
        #endregion

        #region Reset Effects
        public override void ResetEffects(NPC npc)
        {
            void ResetSavedIndex(ref int type, int type1, int type2 = -1)
            {
                if (type >= 0)
                {
                    if (!Main.npc[type].active)
                    {
                        type = -1;
                    }
                    else if (type2 == -1)
                    {
                        if (Main.npc[type].type != type1)
                            type = -1;
                    }
                    else
                    {
                        if (Main.npc[type].type != type1 && Main.npc[type].type != type2)
                            type = -1;
                    }
                }
            }

            for (int i = 0; i < bobbitWormBottom.Length; i++)
                ResetSavedIndex(ref bobbitWormBottom[i], NPCType<BobbitWormSegment>());

            ResetSavedIndex(ref hiveMind, NPCType<HiveMind.HiveMind>());
            ResetSavedIndex(ref perfHive, NPCType<PerforatorHive>());
            ResetSavedIndex(ref slimeGodPurple, NPCType<SlimeGod.EbonianPaladin>(), NPCType<SplitEbonianPaladin>());
            ResetSavedIndex(ref slimeGodRed, NPCType<CrimulanPaladin>(), NPCType<SplitCrimulanPaladin>());
            ResetSavedIndex(ref slimeGod, NPCType<SlimeGodCore>());
            ResetSavedIndex(ref laserEye, NPCID.Retinazer);
            ResetSavedIndex(ref fireEye, NPCID.Spazmatism);
            ResetSavedIndex(ref primeLaser, NPCID.PrimeLaser);
            ResetSavedIndex(ref primeCannon, NPCID.PrimeCannon);
            ResetSavedIndex(ref primeVice, NPCID.PrimeVice);
            ResetSavedIndex(ref primeSaw, NPCID.PrimeSaw);
            ResetSavedIndex(ref aquaticScourge, NPCType<AquaticScourgeHead>());
            ResetSavedIndex(ref brimstoneElemental, NPCType<BrimstoneElemental.BrimstoneElemental>());
            ResetSavedIndex(ref cataclysm, NPCType<Cataclysm>());
            ResetSavedIndex(ref catastrophe, NPCType<Catastrophe>());
            ResetSavedIndex(ref calamitas, NPCType<CalamitasClone>());
            ResetSavedIndex(ref LeviAndAna, NPCType<Leviathan.Leviathan>(), NPCType<Anahita>());
            ResetSavedIndex(ref leviathan, NPCType<Leviathan.Leviathan>());
            ResetSavedIndex(ref siren, NPCType<Anahita>());
            ResetSavedIndex(ref astrumAureus, NPCType<AstrumAureus.AstrumAureus>());
            ResetSavedIndex(ref scavenger, NPCType<RavagerBody>());
            ResetSavedIndex(ref energyFlame, NPCType<ProfanedEnergyBody>());
            ResetSavedIndex(ref doughnutBoss, NPCType<ProfanedGuardianCommander>());
            ResetSavedIndex(ref doughnutBossDefender, NPCType<ProfanedGuardianDefender>());
            ResetSavedIndex(ref doughnutBossHealer, NPCType<ProfanedGuardianHealer>());
            ResetSavedIndex(ref holyBossAttacker, NPCType<ProvSpawnOffense>());
            ResetSavedIndex(ref holyBossDefender, NPCType<ProvSpawnDefense>());
            ResetSavedIndex(ref holyBossHealer, NPCType<ProvSpawnHealer>());
            ResetSavedIndex(ref holyBoss, NPCType<Providence.Providence>());
            ResetSavedIndex(ref voidBoss, NPCType<CeaselessVoid.CeaselessVoid>());
            ResetSavedIndex(ref signus, NPCType<Signus.Signus>());
            ResetSavedIndex(ref ghostBossClone, NPCType<PolterPhantom>());
            ResetSavedIndex(ref ghostBoss, NPCType<Polterghast.Polterghast>());
            ResetSavedIndex(ref DoGHead, NPCType<DevourerofGodsHead>());
            ResetSavedIndex(ref DoGP2, NPCType<DevourerofGodsHead>());
            ResetSavedIndex(ref yharon, NPCType<Yharon.Yharon>());
            ResetSavedIndex(ref yharonP2, NPCType<Yharon.Yharon>());
            ResetSavedIndex(ref SCalCataclysm, NPCType<SupremeCataclysm>());
            ResetSavedIndex(ref SCalCatastrophe, NPCType<SupremeCatastrophe>());
            ResetSavedIndex(ref SCal, NPCType<SupremeCalamitas.SupremeCalamitas>());
            ResetSavedIndex(ref SCalGrief, NPCType<SupremeCalamitas.SupremeCalamitas>());
            ResetSavedIndex(ref SCalLament, NPCType<SupremeCalamitas.SupremeCalamitas>());
            ResetSavedIndex(ref SCalEpiphany, NPCType<SupremeCalamitas.SupremeCalamitas>());
            ResetSavedIndex(ref SCalAcceptance, NPCType<SupremeCalamitas.SupremeCalamitas>());
            ResetSavedIndex(ref SCalWorm, NPCType<SepulcherHead>());

            ResetSavedIndex(ref draedon, NPCType<Draedon>());
            ResetSavedIndex(ref draedonAmbience, NPCType<Draedon>());
            ResetSavedIndex(ref draedonExoMechWorm, NPCType<ThanatosHead>());
            ResetSavedIndex(ref draedonExoMechTwinRed, NPCType<Artemis>());
            ResetSavedIndex(ref draedonExoMechTwinGreen, NPCType<Apollo>());
            ResetSavedIndex(ref draedonExoMechPrime, NPCType<AresBody>());
            ResetSavedIndex(ref draedonExoMechPrimePlasmaCannon, NPCType<AresPlasmaFlamethrower>());

            ResetSavedIndex(ref adultEidolonWyrmHead, NPCType<PrimordialWyrmHead>());

            // Reset the enraged state every frame. The expectation is that bosses will continuously set it back to true if necessary.
            CurrentlyEnraged = false;
            CurrentlyIncreasingDefenseOrDR = false;
            CanHaveBossHealthBar = false;
            ShouldCloseHPBar = false;
            if (arcZapCooldown > 0) { arcZapCooldown--; }

            //Debuff Bool clearing.
            // Doze 2jun2025 - Moved here from PostAI so drawing can read the bools.
            if (debuffResistanceTimer > 0)
                debuffResistanceTimer--;

            timeDistortion = false;
            galvanicCorrosion = false;
            glacialState = false;
            temporalSadness = false;
            eutrophication = false;
            webbed = false;
            vaporfied = false;
            electrified = false;
            pearlAura = false;
            burningBlood = false;
            brainRot = false;
            heavyBleeding = false;
            laceration = false;
            elementalMix = false;
            if (!trueVulnerabilityHex && !vulnerabilityHex)
            {
                cursorFocus = 0;
            }
            trueVulnerabilityHex = false;
            vulnerabilityHex = false;
            markedForDeath = false;
            absorberAffliction = false;
            irradiated = false;
            if (scionsCurioEffected)
                irradiatedContactBoost = 2f;
            brimstoneFlames = false;
            if (!demonicFlames)
                demonicFlamesBonusDamage = 0;
            demonicFlames = false;
            holyFlames = false;
            plague = false;
            // Soma Prime's Shred stacks have a unique falloff mechanic in the debuff's own file.
            armorCrunch = false;
            crumble = false;
            crushDepth = false;
            hadopelagicPressure = false;
            riptide = false;
            godSlayerInferno = false;
            dragonFire = false;
            vermillionFlux = false;
            auricRebuke = false;
            staticDischarge = false;
            miracleBlight = false;
            astralInfection = false;
            whisperingDeath = false;
            nightwither = false;
            if (shocked > 0)
                shocked--;
            voidfrost = false;
            shellfishStaffDebuff = false;
            snapClamDebuff = false;
            sulphurPoison = false;
            sagePoison = false;
            if (ladHearts > 0)
                ladHearts--;
            banishingFire = false;
            wither = false;
            if (ashesOnDeath > 0)
                ashesOnDeath--;

            if (antlionCloudDebuffTimer > 0)
                antlionCloudDebuffTimer--;
            if (cursorFocus > 0 && cursorFocus < cursorFocusMax)
                cursorFocus--;
            relicOfResilienceWeakness = false;
        }
        #endregion

        #region Life Regen
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.damage > 0 && !npc.boss && !npc.friendly && !npc.dontTakeDamage && BiomeTileCounterSystem.SulphurTiles > 30 &&
                !npc.buffImmune[BuffID.Poisoned] && !npc.buffImmune[BuffType<CrushDepth>()])
            {
                if (npc.wet)
                    npc.AddBuff(BuffID.Poisoned, 2);

                if (Main.raining)
                    npc.AddBuff(BuffType<Irradiated>(), 2);
            }

            #region Stacking Debuff Effects
            // Lionfish, Leviathan Teeth, and Jaws of Oblivion debuff stacking
            if (npc.venom)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                int projectileCount = 0;
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if ((p.type == ProjectileType<LionfishProj>() || p.type == ProjectileType<LeviathanTooth>() || p.type == ProjectileType<JawsProjectile>()) &&
                        p.ai[0] == 1f && p.ai[1] == npc.whoAmI)
                    {
                        projectileCount++;
                    }
                }

                if (projectileCount > 0)
                {
                    npc.lifeRegen -= projectileCount * 30;

                    if (damage < projectileCount * 6)
                        damage = projectileCount * 6;
                }
            }

            // Bonebreaker debuff stacking
            if (npc.javelined)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                int projectileCount = 0;
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.type == ProjectileType<BonebreakerProjectile>() &&
                        p.ai[0] == 1f && p.ai[1] == npc.whoAmI)
                    {
                        projectileCount++;
                    }
                }

                if (projectileCount > 0)
                {
                    npc.lifeRegen -= projectileCount * 20;

                    if (damage < projectileCount * 4)
                        damage = projectileCount * 4;
                }
            }
            #endregion

            // Debuff vulnerabilities and resistances.
            // Damage multiplier calcs.
            // Worms that are vulnerable to debuffs and Slime God slimes take reduced damage from vulnerabilities.
            #region Debuff System Multiplier Calculations
            bool wormBoss = CalamityNPCTypeSets.DesertScourge.Contains(npc.type) || CalamityNPCTypeSets.EaterOfWorlds.Contains(npc.type) || CalamityNPCTypeSets.Perforators.Contains(npc.type) ||
                CalamityNPCTypeSets.AquaticScourge.Contains(npc.type) || CalamityNPCTypeSets.AstrumDeus.Contains(npc.type) || CalamityNPCTypeSets.StormWeaver.Contains(npc.type);
            bool slimeGod = CalamityNPCTypeSets.SlimeGod.Contains(npc.type);


            ActiveHeatDebuffMultiplier = HeatDebuffMultiplier;
            ActiveColdDebuffMultiplier = ColdDebuffMultiplier;
            ActiveSicknessDebuffMultiplier = SicknessDebuffMultiplier;
            ActiveElectricDebuffMultiplier = ElectricDebuffMultiplier;
            ActiveWaterDebuffMultiplier = WaterDebuffMultiplier;

            if (irradiated)
            {
                float irradiatedBoost = scionsCurioEffected ? 1.75f : 1f;
                ActiveSicknessDebuffMultiplier += irradiatedBoost;
            }

            if (npc.drippingSlime || npc.drippingSparkleSlime)
            {
                ActiveHeatDebuffMultiplier += 1;
            }

            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.dripping)
            {
                ActiveElectricDebuffMultiplier += 1;
            }

            if (npc.wet || npc.honeyWet || npc.dripping)
            {
                ActiveColdDebuffMultiplier += 1;
                ActiveHeatDebuffMultiplier -= 0.5f;
            }
            if (npc.HasBuff(BuffID.Chilled)) //Nothing inflicts this at the moment. Put here so we can start using it.
            {
                ActiveWaterDebuffMultiplier += 1;
            }
            if (VulnerableToHeat.HasValue)
            {
                if (VulnerableToHeat.Value)
                    ActiveHeatDebuffMultiplier *= wormBoss ? VulnerableToDoTDamageMult_Worms_SlimeGod : VulnerableToDoTDamageMult;
                else
                    ActiveHeatDebuffMultiplier *= ResistantToDoTDamageMult;
            }

            if (VulnerableToCold.HasValue)
            {
                if (VulnerableToCold.Value)
                    ActiveColdDebuffMultiplier *= wormBoss ? VulnerableToDoTDamageMult_Worms_SlimeGod : VulnerableToDoTDamageMult;
                else
                    ActiveColdDebuffMultiplier *= ResistantToDoTDamageMult;
            }

            if (VulnerableToSickness.HasValue)
            {
                if (VulnerableToSickness.Value)
                    ActiveSicknessDebuffMultiplier *= wormBoss ? VulnerableToDoTDamageMult_Worms_SlimeGod : VulnerableToDoTDamageMult;
                else
                    ActiveSicknessDebuffMultiplier *= ResistantToDoTDamageMult;
            }
            if (VulnerableToElectricity.HasValue)
            {
                if (VulnerableToElectricity.Value)
                    ActiveElectricDebuffMultiplier *= wormBoss ? VulnerableToDoTDamageMult_Worms_SlimeGod : VulnerableToDoTDamageMult;
                else
                    ActiveElectricDebuffMultiplier *= ResistantToDoTDamageMult;
            }
            if (VulnerableToWater.HasValue)
            {
                if (VulnerableToWater.Value)
                    ActiveWaterDebuffMultiplier *= wormBoss ? VulnerableToDoTDamageMult_Worms_SlimeGod : VulnerableToDoTDamageMult;
                else
                    ActiveWaterDebuffMultiplier *= ResistantToDoTDamageMult;
            }
            #endregion

            
            //Apply DoT Debuffs
            for (var index = 0; index < npc.buffType.Count(); index++)
            {
                var type = npc.buffType[index];
                var debuffData = BuffDatasets.DebuffDataset[type];
                if (debuffData == null || debuffData == DebuffData.Oiled) //Oiled is done after
                    continue;
                debuffData.NPCLifeRegenMethod(npc, type, ref index,ref damage);
            }
            //Oiled comes after so that we can detect if they have a heat debuff in the above loop
            bool hasVanillaOil = npc.onFrostBurn || npc.onFrostBurn2 || npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame;
            if (npc.oiled)
            {
                var oil = DebuffData.Oiled;
                int index = npc.FindBuffIndex(BuffID.Oiled);
                if (hasVanillaOil)
                    npc.lifeRegen -= oil.EnemyVanillaRegenToCancelOut;
                oil.NPCLifeRegenMethod(npc, BuffID.Oiled, ref index, ref damage);
            }

            // Debuffs that aren't affected by weaknesses or resistances.
            if (somaShredStacks > 0)
                Shred.TickDebuff(npc, this);

            // Reduce DoT on worm bosses and Creepers by 75%.
            if ((wormBoss || npc.type == NPCID.Creeper) && npc.lifeRegen < 0)
            {
                npc.lifeRegen /= 4;
                if (npc.lifeRegen > -1)
                    npc.lifeRegen = -1;

                // Every other EoW body segment and the head segments are immune to DoT.
                if (((npc.ai[2] % 2f == 0f && npc.type == NPCID.EaterofWorldsBody) || npc.type == NPCID.EaterofWorldsHead) && (CalamityWorld.death || BossRushEvent.BossRushActive))
                    npc.lifeRegen = 0;
            }

            // Mana Burn
            //This is at the end to leave it full effect on worms, and to force the DOT numbers to match mana burn
            if (manaBurn > 0)
            {
                if (manaBurnPeak >= 0.1f)
                {
                    manaBurnPeak *= 0.999f;
                }
                manaBurnPeak = Math.Max(manaBurnPeak, manaBurn);
                int burnPerSecond = (int)MathF.Ceiling(manaBurn * 0.5f);
                manaBurn -= burnPerSecond / 60f;

                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= burnPerSecond * 2;
                damage += (int)(burnPerSecond * 0.5f);
            }
            else
            {
                manaBurnPeak = 0;
                playerManaBurnIntensity = 0;
            }
        }

        public void ApplyDPSDebuff(int lifeRegenValue, int damageValue, ref int lifeRegen, ref int damage)
        {
            if (lifeRegen > 0)
                lifeRegen = 0;

            lifeRegen -= lifeRegenValue;

            if (damage < damageValue)
                damage = damageValue;
        }
        #endregion

        #region Load/Unload
        public override void Load()
        {
            #region Setup Vanilla DR Values
            DRValues = new SortedDictionary<int, float> {
                { NPCID.CultistBoss, 0.15f },
                { NPCID.DukeFishron, 0.15f },
                { NPCID.DungeonGuardian, 0.9f },
                { NPCID.Golem, 0.15f },
                { NPCID.GolemFistLeft, 0.15f },
                { NPCID.GolemFistRight, 0.15f },
                { NPCID.GolemHead, 0.15f },
                { NPCID.MoonLordCore, 0.15f },
                { NPCID.MoonLordHand, 0.15f },
                { NPCID.MoonLordHead, 0.15f },
                { NPCID.Plantera, 0.15f },
                { NPCID.HallowBoss, 0.15f },
                { NPCID.PrimeCannon, 0.2f },
                { NPCID.PrimeLaser, 0.2f },
                { NPCID.PrimeSaw, 0.2f },
                { NPCID.PrimeVice, 0.2f },
                { NPCID.Retinazer, 0.2f },
                { NPCID.SkeletronPrime, 0.2f },
                { NPCID.Spazmatism, 0.2f },
                { NPCID.TheDestroyer, 0.1f },
                { NPCID.TheDestroyerBody, 0.2f },
                { NPCID.TheDestroyerTail, 0.35f },
                { NPCID.WallofFlesh, 0.15f },
            };
            #endregion

            // Somehow the SetStatic is called few times before SetStaticDefaults
            // So We Initialize the Dictionary first. And Push Data later (At SetStaticDefaults)
            BossKillTimes = [];
        }

        public override void Unload()
        {
            DRValues?.Clear();
            DRValues = null;

            BossKillTimes?.Clear();
            BossKillTimes = null;
        }
        #endregion

        #region Set Defaults
        public override void SetStaticDefaults()
        {
            #region Add Entries to BossKillTimes
            BossKillTimes.AddRange<int, int>(new Dictionary<int, int>(){

                //
                // VANILLA BOSSES
                //
                { NPCID.KingSlime, 5400 }, // 1:30 (90 seconds)
                { NPCID.EyeofCthulhu, 5400 }, // 1:30 (90 seconds)
                { NPCID.EaterofWorldsHead, 7200 }, // 2:00 (120 seconds)
                { NPCID.EaterofWorldsBody, 7200 },
                { NPCID.EaterofWorldsTail, 7200 },
                { NPCID.BrainofCthulhu, 7200 }, // 2:00 (120 seconds, total length of fight including Creepers phase)
                { NPCID.Creeper, 1800 }, // 0:30 (30 seconds, length of Creepers phase)
                { NPCID.Deerclops, 5400 }, // 1:30 (90 seconds)
                { NPCID.QueenBee, 7200 }, // 2:00 (120 seconds)
                { NPCID.SkeletronHead, 7200 }, // 2:00 (120 seconds)
                { NPCID.WallofFlesh, 7200 }, // 2:00 (120 seconds)
                { NPCID.WallofFleshEye, 7200 },
                { NPCID.QueenSlimeBoss, 7200 }, // 2:00 (120 seconds)
                { NPCID.Spazmatism, 10800 }, // 3:00 (180 seconds)
                { NPCID.Retinazer, 10800 },
                { NPCID.TheDestroyer, 10800 }, // 3:00 (180 seconds)
                { NPCID.TheDestroyerBody, 10800 },
                { NPCID.TheDestroyerTail, 10800 },
                { NPCID.SkeletronPrime, 10800 }, // 3:00 (180 seconds)
                { NPCID.Plantera, 10800 }, // 3:00 (180 seconds)
                { NPCID.Golem, 9000 }, // 2:30 (150 seconds)
                { NPCID.GolemHead, 3600 }, // 1:00 (60 seconds)
                { NPCID.DukeFishron, 9000 }, // 2:30 (150 seconds)
                { NPCID.HallowBoss, 10800 }, // 3:00 (180 seconds)
                { NPCID.CultistBoss, 9000 }, // 2:30 (150 seconds)
                { NPCID.MoonLordCore, 14400 }, // 4:00 (240 seconds)
                { NPCID.MoonLordHand, 7200 }, // 2:00 (120 seconds)
                { NPCID.MoonLordHead, 7200 }, // 2:00 (120 seconds)

                //
                // CALAMITY BOSSES
                //
                { NPCType<DesertScourgeHead>(), 5400 }, // 1:30 (90 seconds)
                { NPCType<DesertScourgeBody>(), 5400 },
                { NPCType<DesertScourgeTail>(), 5400 },
                { NPCType<Crabulon.Crabulon>(), 5400 }, // 1:30 (90 seconds)
                { NPCType<HiveMind.HiveMind>(), 7200 }, // 2:00 (120 seconds)
                { NPCType<PerforatorHive>(), 7200 }, // 2:00 (120 seconds)
                { NPCType<SlimeGodCore>(), 9000 }, // 2:30 (150 seconds) -- total length of Slime God fight
                { NPCType<EbonianPaladin>(), 4500 }, // 1:15 (75 seconds)
                { NPCType<CrimulanPaladin>(), 4500 }, // 1:15 (75 seconds)
                { NPCType<SplitEbonianPaladin>(), 4500 }, // 1:15 (75 seconds) -- split slimes should spawn at 1:15 and die at around 2:30
                { NPCType<SplitCrimulanPaladin>(), 4500 }, // 1:15 (75 seconds)
                { NPCType<Cryogen.Cryogen>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<AquaticScourgeHead>(), 9000 }, // 2:30 (150 seconds)
                { NPCType<AquaticScourgeBody>(), 9000 },
                { NPCType<AquaticScourgeBodyAlt>(), 9000 },
                { NPCType<AquaticScourgeTail>(), 9000 },
                { NPCType<BrimstoneElemental.BrimstoneElemental>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<CalamitasClone>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<Anahita>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<Leviathan.Leviathan>(), 10800 },
                { NPCType<AstrumAureus.AstrumAureus>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<AstrumDeusHead>(), 7200 }, // 2:00 (120 seconds) -- first phase is 1:00
                { NPCType<AstrumDeusBody>(), 7200 },
                { NPCType<AstrumDeusTail>(), 7200 },
                { NPCType<PlaguebringerGoliath.PlaguebringerGoliath>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<RavagerBody>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<ProfanedGuardianCommander>(), 7200 }, // 2:00 (120 seconds)
                { NPCType<Dragonfolly>(), 7200 }, // 2:00 (120 seconds)
                { NPCType<Providence.Providence>(), 14400 }, // 4:00 (240 seconds)
                { NPCType<CeaselessVoid.CeaselessVoid>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<DarkEnergy>(), 1200 }, // 0:20 (20 seconds)
                { NPCType<StormWeaverHead>(), 8100 }, // 2:15 (135 seconds)
                { NPCType<StormWeaverBody>(), 8100 },
                { NPCType<StormWeaverTail>(), 8100 },
                { NPCType<Signus.Signus>(), 7200 }, // 2:00 (120 seconds)
                { NPCType<Polterghast.Polterghast>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<OldDuke.OldDuke>(), 10800 }, // 3:00 (180 seconds)
                { NPCType<DevourerofGodsHead>(), 14400 }, // 4:00 (240 seconds)
                { NPCType<DevourerofGodsBody>(), 14400 }, // DoG Phase 1 is 1:30, DoG Phase 2 is 2:30
                { NPCType<DevourerofGodsTail>(), 14400 },
                { NPCType<Yharon.Yharon>(), 14400 }, // 4:00 (240 seconds)
                { NPCType<Apollo>(), 21600 }, // 6:00 (360 seconds)
                { NPCType<Artemis>(), 21600 },
                { NPCType<AresBody>(), 21600 }, // 6:00 (360 seconds)
                { NPCType<AresGaussNuke>(), 21600 },
                { NPCType<AresLaserCannon>(), 21600 },
                { NPCType<AresPlasmaFlamethrower>(), 21600 },
                { NPCType<AresTeslaCannon>(), 21600 },
                { NPCType<ThanatosHead>(), 21600 }, // 6:00 (360 seconds)
                { NPCType<ThanatosBody1>(), 21600 },
                { NPCType<ThanatosBody2>(), 21600 },
                { NPCType<ThanatosTail>(), 21600 },
                { NPCType<SupremeCalamitas.SupremeCalamitas>(), 18000 }, // 5:00 (300 seconds)
                { NPCType<PrimordialWyrmHead>(), 18000 } // 5:00 (300 seconds)
            });
            #endregion

            // Set Plantera to be able to update oldPos[x]
            // This is only used for her Rev+ AI charge attacks
            NPCID.Sets.TrailingMode[NPCID.Plantera] = 1;

            // Allow Moon Lord to directly be summoned in Multiplayer.
            // This is used for the modified Celestial Sigil without Impending Doom.
            NPCID.Sets.MPAllowedEnemies[NPCID.MoonLordCore] = true;
        }

        public override void SetDefaults(NPC npc)
        {
            for (int i = 0; i < maxPlayerImmunities; i++)
                dashImmunityTime[i] = 0;

            for (int m = 0; m < maxAIMod; m++)
                newAI[m] = 0f;

            // Apply DR to vanilla NPCs.
            // This also applies DR to other mods' NPCs who have set up their NPCs to have DR.
            if (DRValues.ContainsKey(npc.type))
            {
                DRValues.TryGetValue(npc.type, out float newDR);
                DR = newDR;
            }

            // Aquatic Scourge sets kill time in AI, not here.
            if (BossKillTimes.TryGetValue(npc.type, out int revKillTime) && !CalamityNPCTypeSets.AquaticScourge.Contains(npc.type))
            {
                KillTime = revKillTime;
            }

            // Fixing more red mistakes
            if (npc.type == NPCID.WallofFleshEye)
                npc.netAlways = true;

            sagePoisonDamage = 0;
            if (npc.type == NPCID.Golem && (CalamityWorld.revenge || BossRushEvent.BossRushActive))
                npc.noGravity = true;

            DeclareBossHealthUIVariables(npc);

            BaseVanillaBossHPAdjustments(npc);

            if (BossRushEvent.BossRushActive)
                BossRushStatChanges(npc, Mod);

            if (CalamityWorld.revenge)
                RevDeathStatChanges(npc, Mod);

            OtherStatChanges(npc);

            // Change Queen Slime's fart sound on death to something more serious. Except GFB though because naturally
            if (npc.type == NPCID.QueenSlimeBoss)
                npc.DeathSound = Main.zenithWorld ? new SoundStyle("CalamityMod/Sounds/Item/GFBScreams/Scream", 8) : SoundID.NPCDeath1;

            // Function lives in NPCDebuffs.cs
            // This applies to ALL NPCs, vanilla AND Calamity.
            // Calamity NPC debuff immunity definitions live here.
            // Changes to vanilla debuff immunities are applied holistically in the function.
            // Sweeping debuff vulnerabilities for special effects are also applied in this function.
            //
            // NO CALAMITY NPC DEFINES THEIR DEBUFF VULNERABILITIES IN THEIR OWN FILE.
            // THEY ALL RELY ON THIS SINGLE DATABASE.
            npc.SetDebuffImmunities();

            VulnerabilitiesAndResistances(npc);

            BoundNPCSafety(Mod, npc);
        }

        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            // Allow the free Golem Head to pass through platforms in Rev+
            if (npc.type == NPCID.GolemHeadFree && (CalamityWorld.revenge || BossRushEvent.BossRushActive))
                return true;
            return base.CanFallThroughPlatforms(npc);
        }
        #endregion

        #region Boss Health UI Variable Setting
        public void DeclareBossHealthUIVariables(NPC npc)
        {
            if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail)
                SplittingWorm = true;
        }
        #endregion

        #region Base Vanilla Boss HP Adjustments
        private void BaseVanillaBossHPAdjustments(NPC npc)
        {
            switch (npc.type)
            {
                case NPCID.MoonLordCore:
                    npc.lifeMax = 92000;
                    break;

                case NPCID.CultistBoss:
                    npc.lifeMax = 80000;
                    break;

                case NPCID.CultistDragonBody1:
                case NPCID.CultistDragonBody2:
                case NPCID.CultistDragonBody3:
                case NPCID.CultistDragonBody4:
                case NPCID.CultistDragonHead:
                case NPCID.CultistDragonTail:
                    npc.lifeMax = 20000;
                    break;

                case NPCID.AncientCultistSquidhead:
                    npc.lifeMax = 3200;
                    break;

                case NPCID.DukeFishron:
                    npc.lifeMax = 100000;
                    break;

                case NPCID.Golem:
                    npc.lifeMax = 40000;
                    break;

                case NPCID.GolemHead:
                    npc.lifeMax = 26500;
                    break;

                case NPCID.GolemFistRight:
                case NPCID.GolemFistLeft:
                    npc.lifeMax = 7500;
                    break;

                case NPCID.HallowBoss:
                    npc.lifeMax = 100000;
                    break;

                case NPCID.Plantera:
                    npc.lifeMax = 72000;
                    break;

                case NPCID.PlanterasTentacle:
                    npc.lifeMax = 500;
                    break;

                case NPCID.Retinazer:
                    npc.lifeMax = 23500;
                    break;

                case NPCID.Spazmatism:
                    npc.lifeMax = 24500;
                    break;

                case NPCID.QueenSlimeBoss:
                    npc.lifeMax = 27000;
                    break;

                case NPCID.WallofFlesh:
                case NPCID.WallofFleshEye:
                    npc.lifeMax = 12800;
                    break;

                case NPCID.Deerclops:
                    npc.lifeMax = 10000;
                    break;

                case NPCID.BrainofCthulhu:
                    npc.lifeMax = 1500;
                    break;

                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsTail:
                    npc.lifeMax = 175;
                    break;

                case NPCID.EyeofCthulhu:
                    npc.lifeMax = 3000;
                    break;
            }
        }
        #endregion

        #region Boss Rush Stat Changes
        private void BossRushStatChanges(NPC npc, Mod mod)
        {
            if (CalamityNPCSets.BossRushHealth.TryGetValue(npc.type, out var newHP))
            {
                npc.lifeMax = newHP;
            }
        }
        #endregion

        #region Revengeance and Death Mode Stat Changes
        private void RevDeathStatChanges(NPC npc, Mod mod)
        {
            if (npc.type == NPCID.Mothron)
            {
                npc.scale *= 1.25f;
            }
            else if (npc.type == NPCID.MoonLordCore || npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead || npc.type == NPCID.MoonLordLeechBlob)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);

                if (npc.type == NPCID.MoonLordCore)
                    npc.npcSlots = 36f;
            }
            else if (npc.type == NPCID.CultistBoss || (npc.type >= NPCID.CultistDragonHead && npc.type <= NPCID.CultistDragonTail) || npc.type == NPCID.AncientCultistSquidhead)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);

                if (npc.type == NPCID.CultistBoss)
                    npc.npcSlots = 20f;
            }
            else if (npc.type == NPCID.DukeFishron)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.4);
                npc.npcSlots = 20f;
            }
            else if (npc.type == NPCID.Sharkron || npc.type == NPCID.Sharkron2)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 5D);
            }
            else if (npc.type == NPCID.Golem)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);
                npc.npcSlots = 64f;
            }
            else if (npc.type == NPCID.GolemHead)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);
            }
            else if (npc.type == NPCID.GolemFistLeft || npc.type == NPCID.GolemFistRight)
            {
                npc.scale *= 1.15f;
            }
            else if (npc.type == NPCID.HallowBoss)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);
                npc.npcSlots = 32f;
            }
            else if (npc.type == NPCID.Plantera)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);
                npc.npcSlots = 32f;
            }
            else if (CalamityNPCTypeSets.Destroyer.Contains(npc.type))
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.25);
                npc.scale *= Main.zenithWorld ? 2f : 1.2f;
                npc.npcSlots = 10f;
            }
            else if (npc.type == NPCID.Probe)
            {
                if (CalamityWorld.death)
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 2D);

                npc.scale *= Main.zenithWorld ? 2f : 1.2f;
            }
            else if (npc.type == NPCID.SkeletronPrime)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);
                npc.npcSlots = 12f;
            }
            else if (npc.type <= NPCID.PrimeLaser && npc.type >= NPCID.PrimeCannon)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.65);
                npc.scale = 1.15f;
            }
            else if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);
                npc.npcSlots = 10f;
            }
            else if (npc.type == NPCID.QueenSlimeBoss)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);
                npc.npcSlots = 32f;
            }
            else if (npc.type == NPCID.WallofFlesh || npc.type == NPCID.WallofFleshEye)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.3);

                if (npc.type == NPCID.WallofFlesh)
                    npc.npcSlots = 20f;
            }
            else if (npc.type == NPCID.Deerclops)
            {
                npc.npcSlots = 16f;
            }
            else if (npc.type == NPCID.SkeletronHead)
            {
                /*if (CalamityWorld.death)
                    npc.lifeMax = (int)(npc.lifeMax * 0.65);
                else
                    npc.lifeMax = (int)(npc.lifeMax * 0.9);*/

                npc.npcSlots = 12f;
            }
            else if (npc.type == NPCID.SkeletronHand)
            {
                if (CalamityWorld.death)
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.5);
                else
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.75);
            }
            else if (npc.type == NPCID.QueenBee)
            {
                npc.defense = 14;
                npc.defDefense = npc.defense;
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.8);
                npc.npcSlots = 14f;
            }
            else if ((npc.type == NPCID.Bee || npc.type == NPCID.BeeSmall) && CalamityPlayer.areThereAnyDamnBosses)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.5);
                npc.scale *= 1.25f;
            }
            else if (npc.type == NPCID.BrainofCthulhu)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.65);
                npc.npcSlots = 12f;
            }
            else if (npc.type == NPCID.Creeper)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.4);
            }
            else if (CalamityNPCTypeSets.EaterOfWorlds.Contains(npc.type))
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);

                if (npc.type == NPCID.EaterofWorldsHead)
                    npc.npcSlots = 10f;

                if (CalamityWorld.death)
                    npc.scale *= 1.1f;
            }
            else if (npc.type == NPCID.EyeofCthulhu)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.4);
                npc.npcSlots = 10f;
            }
            else if (npc.type == NPCID.ServantofCthulhu)
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 4D);
            }
            else if (npc.type == NPCID.KingSlime)
            {
                if (CalamityWorld.death)
                    npc.scale = Main.getGoodWorld ? 6f : 2.5f;
                else
                    npc.scale = Main.getGoodWorld ? 3f : 1.5f;

                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.5);
            }
            else if ((npc.type == NPCID.Wraith || npc.type == NPCID.Mimic || npc.type == NPCID.Reaper || npc.type == NPCID.PresentMimic || npc.type == NPCID.SandElemental || npc.type == NPCID.Ghost) && Main.getGoodWorld)
            {
                npc.knockBackResist = 0f;
            }
        }
        #endregion

        #region Vulnerabilities and Resistances
        private void VulnerabilitiesAndResistances(NPC npc)
        {
            // These enemies are categorized in such a way to make them easy to understand
            // Regroup these if necessary, reminder to keep it comprehensive
            switch (npc.type)
            {
                // Regular organic desert enemies.
                case NPCID.Antlion:
                case NPCID.GiantWalkingAntlion:
                case NPCID.FlyingAntlion:
                case NPCID.GiantFlyingAntlion:
                case NPCID.LarvaeAntlion:
                case NPCID.WalkingAntlion:
                case NPCID.TombCrawlerHead:
                case NPCID.TombCrawlerBody:
                case NPCID.TombCrawlerTail:
                case NPCID.DesertBeast:
                case NPCID.DuneSplicerHead:
                case NPCID.DuneSplicerBody:
                case NPCID.DuneSplicerTail:
                case NPCID.DesertLamiaDark:
                case NPCID.DesertLamiaLight:
                case NPCID.DesertGhoul:
                case NPCID.DesertGhoulCorruption:
                case NPCID.DesertGhoulCrimson:
                case NPCID.DesertGhoulHallow:
                case NPCID.Mummy:
                case NPCID.DarkMummy:
                case NPCID.LightMummy:
                case NPCID.BloodMummy:
                case NPCID.Tumbleweed:
                case NPCID.SandShark:
                case NPCID.SandsharkCorrupt:
                case NPCID.SandsharkCrimson:
                case NPCID.SandsharkHallow:
                    VulnerableToCold = true;
                    VulnerableToSickness = true;
                    VulnerableToWater = true;
                    break;

                // Scorpions and sand elemental.
                case NPCID.DesertScorpionWalk:
                case NPCID.DesertScorpionWall:
                case NPCID.SandElemental:
                    VulnerableToCold = true;
                    VulnerableToSickness = false;
                    VulnerableToWater = true;
                    break;

                // Desert slimes.
                case NPCID.SandSlime:
                    VulnerableToCold = true;
                    VulnerableToSickness = false;
                    VulnerableToWater = true;
                    VulnerableToHeat = true;
                    break;

                // Organic undead or other enemies that are covered in slime.
                case NPCID.ArmedZombieSlimed:
                case NPCID.BigSlimedZombie:
                case NPCID.SlimedZombie:
                case NPCID.SmallSlimedZombie:
                    VulnerableToCold = true;
                    VulnerableToHeat = true;
                    break;

                // Slimes that use heat-related attacks.
                case NPCID.LavaSlime:
                    VulnerableToCold = true;
                    VulnerableToSickness = false;
                    VulnerableToHeat = false;
                    VulnerableToWater = true;
                    break;

                // Regular slimes.
                case NPCID.QueenSlimeBoss:
                case NPCID.QueenSlimeMinionBlue:
                case NPCID.QueenSlimeMinionPink:
                case NPCID.QueenSlimeMinionPurple:
                case NPCID.DungeonSlime:
                case NPCID.BabySlime:
                case NPCID.BlackSlime:
                case NPCID.BlueSlime:
                case NPCID.CorruptSlime:
                case NPCID.GoldenSlime:
                case NPCID.GreenSlime:
                case NPCID.IlluminantSlime:
                case NPCID.JungleSlime:
                case NPCID.KingSlime:
                case NPCID.MotherSlime:
                case NPCID.PurpleSlime:
                case NPCID.RainbowSlime:
                case NPCID.RedSlime:
                case NPCID.ShimmerSlime:
                case NPCID.Slimeling:
                case NPCID.SlimeMasked:
                case NPCID.Slimer:
                case NPCID.Slimer2:
                case NPCID.SlimeRibbonGreen:
                case NPCID.SlimeRibbonRed:
                case NPCID.SlimeRibbonWhite:
                case NPCID.SlimeRibbonYellow:
                case NPCID.SlimeSpiked:
                case NPCID.SpikedJungleSlime:
                case NPCID.UmbrellaSlime:
                case NPCID.YellowSlime:
                case NPCID.ToxicSludge:
                case NPCID.Crimslime:
                case NPCID.BigCrimslime:
                case NPCID.LittleCrimslime:
                case NPCID.Gastropod:
                case NPCID.Pinky:
                    VulnerableToSickness = false;
                    VulnerableToHeat = true;
                    break;

                // Skeletons and other armored/bone enemies that use heat-related attacks.
                case NPCID.HellArmoredBones:
                case NPCID.HellArmoredBonesMace:
                case NPCID.HellArmoredBonesSpikeShield:
                case NPCID.HellArmoredBonesSword:
                case NPCID.DiabolistRed:
                case NPCID.DiabolistWhite:
                    VulnerableToHeat = false;
                    VulnerableToCold = true;
                    VulnerableToSickness = false;
                    VulnerableToWater = true;
                    break;

                // Spore skeleton.
                case NPCID.SporeSkeleton:
                    VulnerableToHeat = true;
                    VulnerableToSickness = false;
                    VulnerableToWater = true;
                    break;

                // Skeletons and other armored/bone enemies that are dead or undead.
                case NPCID.SkeletronHand:
                case NPCID.SkeletronHead:
                case NPCID.AngryBones:
                case NPCID.AngryBonesBig:
                case NPCID.AngryBonesBigHelmet:
                case NPCID.AngryBonesBigMuscle:
                case NPCID.DarkCaster:
                case NPCID.CursedSkull:
                case NPCID.GiantCursedSkull:
                case NPCID.DungeonGuardian:
                case NPCID.BigBoned:
                case NPCID.BlueArmoredBones:
                case NPCID.BlueArmoredBonesMace:
                case NPCID.BlueArmoredBonesNoPants:
                case NPCID.BlueArmoredBonesSword:
                case NPCID.BoneLee:
                case NPCID.BoneSerpentBody:
                case NPCID.BoneSerpentHead:
                case NPCID.BoneSerpentTail:
                case NPCID.BoneThrowingSkeleton:
                case NPCID.BoneThrowingSkeleton2:
                case NPCID.BoneThrowingSkeleton3:
                case NPCID.BoneThrowingSkeleton4:
                case NPCID.RustyArmoredBonesAxe:
                case NPCID.RustyArmoredBonesFlail:
                case NPCID.RustyArmoredBonesSword:
                case NPCID.RustyArmoredBonesSwordNoArmor:
                case NPCID.ShortBones:
                case NPCID.Necromancer:
                case NPCID.NecromancerArmored:
                case NPCID.RaggedCaster:
                case NPCID.RaggedCasterOpenCoat:
                case NPCID.SkeletonCommando:
                case NPCID.ArmoredSkeleton:
                case NPCID.BigHeadacheSkeleton:
                case NPCID.BigMisassembledSkeleton:
                case NPCID.BigPantlessSkeleton:
                case NPCID.BigSkeleton:
                case NPCID.DD2SkeletonT1:
                case NPCID.DD2SkeletonT3:
                case NPCID.GreekSkeleton:
                case NPCID.HeadacheSkeleton:
                case NPCID.HeavySkeleton:
                case NPCID.MisassembledSkeleton:
                case NPCID.PantlessSkeleton:
                case NPCID.Skeleton:
                case NPCID.SkeletonAlien:
                case NPCID.SkeletonArcher:
                case NPCID.SkeletonAstonaut:
                case NPCID.SkeletonSniper:
                case NPCID.SkeletonTopHat:
                case NPCID.SmallHeadacheSkeleton:
                case NPCID.SmallMisassembledSkeleton:
                case NPCID.SmallPantlessSkeleton:
                case NPCID.SmallSkeleton:
                case NPCID.TacticalSkeleton:
                case NPCID.Tim:
                case NPCID.UndeadMiner:
                case NPCID.UndeadViking:
                case NPCID.ArmoredViking:
                case NPCID.GraniteFlyer:
                case NPCID.GraniteGolem:
                case NPCID.RuneWizard:
                case NPCID.Golem:
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                case NPCID.GolemHead:
                case NPCID.GolemHeadFree:
                case NPCID.RockGolem:
                    VulnerableToSickness = false;
                    VulnerableToWater = true;
                    break;

                // Metal non-robotic enemies
                case NPCID.BigMimicCorruption:
                case NPCID.BigMimicCrimson:
                case NPCID.BigMimicHallow:
                case NPCID.BigMimicJungle:
                case NPCID.Paladin:
                case NPCID.Mimic:
                case NPCID.PresentMimic:
                case NPCID.PirateShipCannon:
                case NPCID.PossessedArmor:
                    VulnerableToSickness = false;
                    break;

                // Robotic enemies.
                case NPCID.Probe:
                case NPCID.MartianProbe:
                case NPCID.DeadlySphere:
                case NPCID.MartianDrone:
                case NPCID.MartianWalker:
                case NPCID.MartianTurret:
                case NPCID.ElfCopter:
                case NPCID.SkeletronPrime:
                case NPCID.PrimeCannon:
                case NPCID.PrimeLaser:
                case NPCID.PrimeSaw:
                case NPCID.PrimeVice:
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                case NPCID.SantaNK1:
                case NPCID.MartianSaucer:
                case NPCID.MartianSaucerCannon:
                case NPCID.MartianSaucerCore:
                case NPCID.MartianSaucerTurret:
                case NPCID.ChatteringTeethBomb:
                    VulnerableToElectricity = true;
                    VulnerableToSickness = false;
                    break;

                // Ghostly or ethereal enemies.
                case NPCID.DungeonSpirit:
                case NPCID.AncientCultistSquidhead:
                case NPCID.CultistDragonBody1:
                case NPCID.CultistDragonBody2:
                case NPCID.CultistDragonBody3:
                case NPCID.CultistDragonBody4:
                case NPCID.CultistDragonHead:
                case NPCID.CultistDragonTail:
                case NPCID.Ghost:
                case NPCID.ChaosElemental:
                case NPCID.CrimsonAxe:
                case NPCID.EnchantedSword:
                case NPCID.CursedHammer:
                case NPCID.DesertDjinn:
                case NPCID.Wraith:
                case NPCID.ShadowFlameApparition:
                case NPCID.Reaper:
                case NPCID.Poltergeist:
                case NPCID.Pixie:
                case NPCID.PirateGhost:
                    VulnerableToSickness = false;
                    break;

                // Organic enemies.
                case NPCID.HallowBoss:
                case NPCID.Dandelion:
                case NPCID.Gnome:
                case NPCID.BloodEelHead:
                case NPCID.BloodEelBody:
                case NPCID.BloodEelTail:
                case NPCID.BloodSquid:
                case NPCID.BloodNautilus:
                case NPCID.GoblinShark:
                case NPCID.EyeballFlyingFish:
                case NPCID.ZombieMerman:
                case NPCID.CultistArcherBlue:
                case NPCID.CultistArcherWhite:
                case NPCID.CultistBoss:
                case NPCID.CultistDevote:
                case NPCID.BloodCrawler:
                case NPCID.BloodCrawlerWall:
                case NPCID.CaveBat:
                case NPCID.GiantBat:
                case NPCID.CochinealBeetle:
                case NPCID.CyanBeetle:
                case NPCID.LacBeetle:
                case NPCID.AnomuraFungus:
                case NPCID.GiantFungiBulb:
                case NPCID.FungiBulb:
                case NPCID.MushiLadybug:
                case NPCID.SporeBat:
                case NPCID.ZombieMushroom:
                case NPCID.ZombieMushroomHat:
                case NPCID.ManEater:
                case NPCID.Snatcher:
                case NPCID.AngryTrapper:
                case NPCID.HoppinJack:
                case NPCID.Splinterling:
                case NPCID.MourningWood:
                case NPCID.Pumpking:
                case NPCID.Everscream:
                case NPCID.Crimera:
                case NPCID.BigCrimera:
                case NPCID.LittleCrimera:
                case NPCID.DemonEye:
                case NPCID.DemonEye2:
                case NPCID.DemonEyeOwl:
                case NPCID.DemonEyeSpaceship:
                case NPCID.DevourerBody:
                case NPCID.DevourerHead:
                case NPCID.DevourerTail:
                case NPCID.DoctorBones:
                case NPCID.EaterofSouls:
                case NPCID.BigEater:
                case NPCID.LittleEater:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsTail:
                case NPCID.FaceMonster:
                case NPCID.GiantShelly:
                case NPCID.GiantShelly2:
                case NPCID.GiantWormBody:
                case NPCID.GiantWormHead:
                case NPCID.GiantWormTail:
                case NPCID.GoblinScout:
                case NPCID.Harpy:
                case NPCID.JungleBat:
                case NPCID.Nymph:
                case NPCID.Raven:
                case NPCID.Salamander:
                case NPCID.Salamander2:
                case NPCID.Salamander3:
                case NPCID.Salamander4:
                case NPCID.Salamander5:
                case NPCID.Salamander6:
                case NPCID.Salamander7:
                case NPCID.Salamander8:
                case NPCID.Salamander9:
                case NPCID.Vulture:
                case NPCID.WallCreeper:
                case NPCID.WallCreeperWall:
                case NPCID.ArmedZombie:
                case NPCID.ArmedZombieCenx:
                case NPCID.ArmedZombiePincussion:
                case NPCID.ArmedZombieSwamp:
                case NPCID.ArmedZombieTwiggy:
                case NPCID.ArmedTorchZombie:
                case NPCID.BaldZombie:
                case NPCID.BigBaldZombie:
                case NPCID.BigFemaleZombie:
                case NPCID.BigPincushionZombie:
                case NPCID.BigRainZombie:
                case NPCID.BigSwampZombie:
                case NPCID.BigTwiggyZombie:
                case NPCID.BigZombie:
                case NPCID.MaggotZombie:
                case NPCID.BloodZombie:
                case NPCID.FemaleZombie:
                case NPCID.PincushionZombie:
                case NPCID.SmallBaldZombie:
                case NPCID.SmallFemaleZombie:
                case NPCID.SmallPincushionZombie:
                case NPCID.SmallRainZombie:
                case NPCID.SmallSwampZombie:
                case NPCID.SmallTwiggyZombie:
                case NPCID.SmallZombie:
                case NPCID.SwampZombie:
                case NPCID.TorchZombie:
                case NPCID.TwiggyZombie:
                case NPCID.Zombie:
                case NPCID.ZombieDoctor:
                case NPCID.ZombiePixie:
                case NPCID.ZombieRaincoat:
                case NPCID.ZombieSuperman:
                case NPCID.ZombieSweater:
                case NPCID.ZombieXmas:
                case NPCID.Clinger:
                case NPCID.Corruptor:
                case NPCID.Derpling:
                case NPCID.Herpling:
                case NPCID.DiggerBody:
                case NPCID.DiggerHead:
                case NPCID.DiggerTail:
                case NPCID.FloatyGross:
                case NPCID.FlyingSnake:
                case NPCID.Lihzahrd:
                case NPCID.LihzahrdCrawler:
                case NPCID.GiantFlyingFox:
                case NPCID.GiantTortoise:
                case NPCID.IchorSticker:
                case NPCID.IlluminantBat:
                case NPCID.Medusa:
                case NPCID.Moth:
                case NPCID.Unicorn:
                case NPCID.WanderingEye:
                case NPCID.Werewolf:
                case NPCID.SeekerBody:
                case NPCID.SeekerHead:
                case NPCID.SeekerTail:
                case NPCID.WyvernBody:
                case NPCID.WyvernBody2:
                case NPCID.WyvernBody3:
                case NPCID.WyvernHead:
                case NPCID.WyvernLegs:
                case NPCID.WyvernTail:
                case NPCID.Clown:
                case NPCID.CorruptBunny:
                case NPCID.CrimsonBunny:
                case NPCID.Drippler:
                case NPCID.TheGroom:
                case NPCID.TheBride:
                case NPCID.GoblinArcher:
                case NPCID.GoblinPeon:
                case NPCID.GoblinSorcerer:
                case NPCID.GoblinSummoner:
                case NPCID.GoblinThief:
                case NPCID.GoblinWarrior:
                case NPCID.DD2DarkMageT1:
                case NPCID.DD2DarkMageT3:
                case NPCID.DD2DrakinT2:
                case NPCID.DD2DrakinT3:
                case NPCID.DD2GoblinBomberT1:
                case NPCID.DD2GoblinBomberT2:
                case NPCID.DD2GoblinBomberT3:
                case NPCID.DD2GoblinT1:
                case NPCID.DD2GoblinT2:
                case NPCID.DD2GoblinT3:
                case NPCID.DD2JavelinstT1:
                case NPCID.DD2JavelinstT2:
                case NPCID.DD2JavelinstT3:
                case NPCID.DD2KoboldFlyerT2:
                case NPCID.DD2KoboldFlyerT3:
                case NPCID.DD2KoboldWalkerT2:
                case NPCID.DD2KoboldWalkerT3:
                case NPCID.DD2OgreT2:
                case NPCID.DD2OgreT3:
                case NPCID.DD2WitherBeastT2:
                case NPCID.DD2WitherBeastT3:
                case NPCID.DD2WyvernT1:
                case NPCID.DD2WyvernT2:
                case NPCID.DD2WyvernT3:
                case NPCID.Parrot:
                case NPCID.PirateCaptain:
                case NPCID.PirateCorsair:
                case NPCID.PirateCrossbower:
                case NPCID.PirateDeadeye:
                case NPCID.PirateDeckhand:
                case NPCID.Mothron:
                case NPCID.MothronEgg:
                case NPCID.MothronSpawn:
                case NPCID.Butcher:
                case NPCID.DrManFly:
                case NPCID.Eyezor:
                case NPCID.Frankenstein:
                case NPCID.Fritz:
                case NPCID.Nailhead:
                case NPCID.Psycho:
                case NPCID.SwampThing:
                case NPCID.ThePossessed:
                case NPCID.Vampire:
                case NPCID.VampireBat:
                case NPCID.BrainScrambler:
                case NPCID.GigaZapper:
                case NPCID.GrayGrunt:
                case NPCID.MartianEngineer:
                case NPCID.MartianOfficer:
                case NPCID.RayGunner:
                case NPCID.Scutlix:
                case NPCID.ScutlixRider:
                case NPCID.HeadlessHorseman:
                case NPCID.Hellhound:
                case NPCID.Scarecrow1:
                case NPCID.Scarecrow2:
                case NPCID.Scarecrow3:
                case NPCID.Scarecrow4:
                case NPCID.Scarecrow5:
                case NPCID.Scarecrow6:
                case NPCID.Scarecrow7:
                case NPCID.Scarecrow8:
                case NPCID.Scarecrow9:
                case NPCID.Scarecrow10:
                case NPCID.NebulaHeadcrab:
                case NPCID.LunarTowerNebula:
                case NPCID.NebulaBeast:
                case NPCID.NebulaBrain:
                case NPCID.NebulaSoldier:
                case NPCID.LunarTowerSolar:
                case NPCID.SolarCorite:
                case NPCID.SolarCrawltipedeTail:
                case NPCID.SolarDrakomire:
                case NPCID.SolarDrakomireRider:
                case NPCID.SolarSolenian:
                case NPCID.SolarSpearman:
                case NPCID.SolarSroller:
                case NPCID.LunarTowerStardust:
                case NPCID.StardustCellBig:
                case NPCID.StardustCellSmall:
                case NPCID.StardustJellyfishBig:
                case NPCID.StardustSoldier:
                case NPCID.StardustSpiderBig:
                case NPCID.StardustSpiderSmall:
                case NPCID.StardustWormHead:
                case NPCID.LunarTowerVortex:
                case NPCID.VortexHornet:
                case NPCID.VortexHornetQueen:
                case NPCID.VortexLarva:
                case NPCID.VortexRifleman:
                case NPCID.VortexSoldier:
                case NPCID.BrainofCthulhu:
                case NPCID.Creeper:
                case NPCID.EyeofCthulhu:
                case NPCID.ServantofCthulhu:
                case NPCID.MoonLordCore:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordHead:
                case NPCID.Spazmatism: // Changes to robotic in phase 2
                case NPCID.Retinazer: // Changes to robotic in phase 2
                    VulnerableToCold = true;
                    VulnerableToHeat = true;
                    VulnerableToSickness = true;
                    break;

                // Demons and shit.
                case NPCID.WallofFlesh:
                case NPCID.WallofFleshEye:
                case NPCID.TheHungry:
                case NPCID.TheHungryII:
                case NPCID.LeechBody:
                case NPCID.LeechHead:
                case NPCID.LeechTail:
                case NPCID.Demon:
                case NPCID.VoodooDemon:
                case NPCID.RedDevil:
                case NPCID.DemonTaxCollector:
                    VulnerableToCold = true;
                    VulnerableToHeat = false;
                    VulnerableToSickness = true;
                    break;

                // Fire enemies that are also organic.
                case NPCID.FireImp:
                case NPCID.Hellbat:
                case NPCID.Lavabat:
                    VulnerableToCold = true;
                    VulnerableToHeat = false;
                    VulnerableToSickness = true;
                    VulnerableToWater = true;
                    break;

                // Fire enemies that aren't organic.
                case NPCID.MeteorHead:
                    VulnerableToCold = true;
                    VulnerableToHeat = false;
                    VulnerableToSickness = false;
                    VulnerableToWater = true;
                    break;

                // Lightning bug thing.
                case NPCID.DD2LightningBugT3:
                    VulnerableToElectricity = false;
                    VulnerableToCold = true;
                    VulnerableToHeat = true;
                    VulnerableToSickness = true;
                    break;

                // Betsy.
                case NPCID.DD2Betsy:
                    VulnerableToCold = true;
                    VulnerableToHeat = false;
                    VulnerableToSickness = true;
                    break;

                // Nimbus
                case NPCID.AngryNimbus:
                    VulnerableToCold = true;
                    VulnerableToElectricity = false;
                    VulnerableToWater = false;
                    VulnerableToHeat = false;
                    VulnerableToSickness = false;
                    break;

                // Cold-themed enemies
                case NPCID.ArmedZombieEskimo:
                case NPCID.ZombieEskimo:
                case NPCID.IceBat:
                case NPCID.SnowFlinx:
                case NPCID.IceTortoise:
                case NPCID.IcyMerman:
                case NPCID.PigronCorruption:
                case NPCID.PigronCrimson:
                case NPCID.PigronHallow:
                case NPCID.Wolf:
                case NPCID.CorruptPenguin:
                case NPCID.CrimsonPenguin:
                case NPCID.ElfArcher:
                case NPCID.Krampus:
                case NPCID.Yeti:
                case NPCID.Nutcracker:
                case NPCID.NutcrackerSpinning:
                case NPCID.ZombieElf:
                case NPCID.ZombieElfBeard:
                case NPCID.ZombieElfGirl:
                case NPCID.Deerclops:
                    VulnerableToHeat = true;
                    VulnerableToCold = false;
                    VulnerableToSickness = true;
                    break;

                // Cold-themed enemies that aren't organic.
                case NPCID.IceElemental:
                case NPCID.IceSlime:
                case NPCID.SpikedIceSlime:
                case NPCID.IceGolem:
                case NPCID.IceMimic:
                case NPCID.MisterStabby:
                case NPCID.SnowBalla:
                case NPCID.SnowmanGangsta:
                case NPCID.Flocko:
                case NPCID.IceQueen:
                case NPCID.GingerbreadMan:
                    VulnerableToCold = false;
                    VulnerableToHeat = true;
                    VulnerableToSickness = false;
                    break;

                // Water-themed enemies.
                case NPCID.Crawdad:
                case NPCID.Crawdad2:
                case NPCID.BlueJellyfish:
                case NPCID.GreenJellyfish:
                case NPCID.PinkJellyfish:
                case NPCID.BloodJelly:
                case NPCID.FungoFish:
                case NPCID.Crab:
                case NPCID.Piranha:
                case NPCID.SeaSnail:
                case NPCID.Squid:
                case NPCID.Shark:
                case NPCID.AnglerFish:
                case NPCID.Arapaima:
                case NPCID.BloodFeeder:
                case NPCID.CorruptGoldfish:
                case NPCID.CrimsonGoldfish:
                case NPCID.FlyingFish:
                case NPCID.CreatureFromTheDeep:
                case NPCID.DukeFishron:
                case NPCID.Sharkron:
                case NPCID.Sharkron2:
                    VulnerableToHeat = false;
                    VulnerableToSickness = true;
                    VulnerableToElectricity = true;
                    VulnerableToWater = false;
                    break;

                // Fucking bees, hornets and poisonous/toxic stuff.
                case NPCID.Bee:
                case NPCID.BeeSmall:
                case NPCID.QueenBee:
                case NPCID.BigHornetFatty:
                case NPCID.BigHornetHoney:
                case NPCID.BigHornetLeafy:
                case NPCID.BigHornetSpikey:
                case NPCID.BigHornetStingy:
                case NPCID.BigMossHornet:
                case NPCID.GiantMossHornet:
                case NPCID.Hornet:
                case NPCID.HornetFatty:
                case NPCID.HornetHoney:
                case NPCID.HornetLeafy:
                case NPCID.HornetSpikey:
                case NPCID.HornetStingy:
                case NPCID.LittleHornetFatty:
                case NPCID.LittleHornetHoney:
                case NPCID.LittleHornetLeafy:
                case NPCID.LittleHornetSpikey:
                case NPCID.LittleHornetStingy:
                case NPCID.LittleMossHornet:
                case NPCID.MossHornet:
                case NPCID.TinyMossHornet:
                case NPCID.JungleCreeper:
                case NPCID.JungleCreeperWall:
                case NPCID.BlackRecluse:
                case NPCID.BlackRecluseWall:
                case NPCID.Plantera:
                case NPCID.PlanterasTentacle:
                    VulnerableToCold = true;
                    VulnerableToHeat = true;
                    VulnerableToSickness = false;
                    break;

                // Town NPCs. Mostly irrelevant, but it displays in the Bestiary
                case NPCID.Merchant:
                case NPCID.Nurse:
                case NPCID.ArmsDealer:
                case NPCID.Dryad:
                case NPCID.Guide:
                case NPCID.OldMan:
                case NPCID.Demolitionist:
                case NPCID.Clothier:
                case NPCID.BoundGoblin:
                case NPCID.BoundWizard:
                case NPCID.GoblinTinkerer:
                case NPCID.Wizard:
                case NPCID.BoundMechanic:
                case NPCID.Mechanic:
                case NPCID.Truffle:
                case NPCID.Steampunker:
                case NPCID.DyeTrader:
                case NPCID.PartyGirl:
                case NPCID.Painter:
                case NPCID.WitchDoctor:
                case NPCID.Pirate:
                case NPCID.Stylist:
                case NPCID.WebbedStylist:
                case NPCID.TravellingMerchant:
                case NPCID.Angler:
                case NPCID.SleepingAngler:
                case NPCID.DD2Bartender:
                case NPCID.BartenderUnconscious:
                case NPCID.Golfer:
                case NPCID.GolferRescue:
                case NPCID.BestiaryGirl:
                case NPCID.Princess:
                case NPCID.TownCat:
                case NPCID.TownDog:
                case NPCID.TownBunny:
                    VulnerableToCold = true;
                    VulnerableToHeat = true;
                    VulnerableToSickness = true;
                    break;
                case NPCID.SantaClaus:
                    VulnerableToCold = false;
                    VulnerableToHeat = true;
                    VulnerableToSickness = true;
                    break;
                case NPCID.TaxCollector:
                    VulnerableToCold = true;
                    VulnerableToHeat = false;
                    VulnerableToSickness = true;
                    break;
                // Non-organic Town NPCs.
                case NPCID.Cyborg:
                case NPCID.BoundTownSlimeOld:
                    VulnerableToSickness = false;
                    break;
                // Town Slimes.
                case NPCID.TownSlimeBlue:
                case NPCID.TownSlimeGreen:
                case NPCID.TownSlimeOld:
                case NPCID.TownSlimePurple:
                case NPCID.TownSlimeRainbow:
                case NPCID.TownSlimeRed:
                case NPCID.TownSlimeYellow:
                case NPCID.TownSlimeCopper:
                case NPCID.BoundTownSlimePurple:
                    VulnerableToSickness = false;
                    VulnerableToHeat = true;
                    break;

                // Critters
                case NPCID.Bunny:
                case NPCID.Bird:
                case NPCID.BirdBlue:
                case NPCID.BirdRed:
                case NPCID.Squirrel:
                case NPCID.Mouse:
                case NPCID.BunnySlimed:
                case NPCID.BunnyXmas:
                case NPCID.Firefly:
                case NPCID.Butterfly:
                case NPCID.Worm:
                case NPCID.LightningBug:
                case NPCID.Snail:
                case NPCID.GlowingSnail:
                case NPCID.Frog:
                case NPCID.Duck:
                case NPCID.Duck2:
                case NPCID.DuckWhite:
                case NPCID.DuckWhite2:
                case NPCID.ScorpionBlack:
                case NPCID.Scorpion:
                case NPCID.TruffleWorm:
                case NPCID.TruffleWormDigger:
                case NPCID.Grasshopper:
                case NPCID.GoldBird:
                case NPCID.GoldBunny:
                case NPCID.GoldButterfly:
                case NPCID.GoldFrog:
                case NPCID.GoldGrasshopper:
                case NPCID.GoldMouse:
                case NPCID.GoldWorm:
                case NPCID.EnchantedNightcrawler:
                case NPCID.Grubby:
                case NPCID.Sluggy:
                case NPCID.Buggy:
                case NPCID.SquirrelRed:
                case NPCID.SquirrelGold:
                case NPCID.PartyBunny:
                case NPCID.BlackDragonfly:
                case NPCID.BlueDragonfly:
                case NPCID.GreenDragonfly:
                case NPCID.OrangeDragonfly:
                case NPCID.RedDragonfly:
                case NPCID.YellowDragonfly:
                case NPCID.GoldDragonfly:
                case NPCID.Seagull:
                case NPCID.Seagull2:
                case NPCID.LadyBug:
                case NPCID.GoldLadyBug:
                case NPCID.Maggot:
                case NPCID.Grebe:
                case NPCID.Grebe2:
                case NPCID.Rat:
                case NPCID.Owl:
                case NPCID.WaterStrider:
                case NPCID.GoldWaterStrider:
                case NPCID.ExplosiveBunny:
                case NPCID.EmpressButterfly:
                case NPCID.Stinkbug:
                case NPCID.ScarletMacaw:
                case NPCID.BlueMacaw:
                case NPCID.Toucan:
                case NPCID.YellowCockatiel:
                case NPCID.GrayCockatiel:
                case NPCID.Shimmerfly:
                case NPCID.BoundTownSlimeYellow:
                    VulnerableToCold = true;
                    VulnerableToHeat = true;
                    VulnerableToSickness = true;
                    break;
                // Water Critters
                case NPCID.Goldfish:
                case NPCID.GoldfishWalker:
                case NPCID.GoldGoldfish:
                case NPCID.GoldGoldfishWalker:
                case NPCID.Pupfish:
                case NPCID.Dolphin:
                case NPCID.Turtle:
                case NPCID.TurtleJungle:
                case NPCID.SeaTurtle:
                case NPCID.Seahorse:
                case NPCID.GoldSeahorse:
                    VulnerableToHeat = false;
                    VulnerableToSickness = true;
                    VulnerableToElectricity = true;
                    VulnerableToWater = false;
                    break;
                // Penguins
                case NPCID.Penguin:
                case NPCID.PenguinBlack:
                    VulnerableToCold = false;
                    VulnerableToHeat = true;
                    VulnerableToSickness = true;
                    break;
                // Fairies
                case NPCID.FairyCritterPink:
                case NPCID.FairyCritterGreen:
                case NPCID.FairyCritterBlue:
                    VulnerableToSickness = false;
                    break;
                // Gem Critters
                case NPCID.GemSquirrelAmethyst:
                case NPCID.GemSquirrelTopaz:
                case NPCID.GemSquirrelSapphire:
                case NPCID.GemSquirrelEmerald:
                case NPCID.GemSquirrelRuby:
                case NPCID.GemSquirrelDiamond:
                case NPCID.GemSquirrelAmber:
                case NPCID.GemBunnyAmethyst:
                case NPCID.GemBunnyTopaz:
                case NPCID.GemBunnySapphire:
                case NPCID.GemBunnyEmerald:
                case NPCID.GemBunnyDiamond:
                case NPCID.GemBunnyAmber:
                    VulnerableToCold = true;
                    VulnerableToSickness = true;
                    VulnerableToWater = true;
                    break;
                // Underworld Critters
                case NPCID.HellButterfly:
                case NPCID.Lavafly:
                case NPCID.MagmaSnail:
                    VulnerableToCold = true;
                    VulnerableToHeat = false;
                    VulnerableToSickness = true;
                    VulnerableToWater = true;
                    break;
            }
        }
        #endregion

        #region Other Stat Changes
        private void OtherStatChanges(NPC npc)
        {
            EditGlobalCoinDrops(npc);

            if ((npc.boss && npc.type != NPCID.MartianSaucerCore) || CalamityNPCSets.ScalesHealthLikeBoss[npc.type])
            {
                double HPBoost = CalamityServerConfig.Instance.BossHealthBoost * 0.01;
                npc.lifeMax += (int)Math.Round(npc.lifeMax * HPBoost);
            }

            switch (npc.type)
            {
                case NPCID.KingSlime:
                case NPCID.EyeofCthulhu:
                case NPCID.BrainofCthulhu:
                case NPCID.QueenBee:
                case NPCID.Paladin:
                case NPCID.BigMimicCorruption:
                case NPCID.BigMimicCrimson:
                case NPCID.BigMimicHallow:
                case NPCID.Mothron:
                case NPCID.EaterofWorldsHead:
                case NPCID.SkeletronHead:
                case NPCID.WallofFlesh:
                case NPCID.Spazmatism:
                case NPCID.Retinazer:
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                case NPCID.SkeletronPrime:
                case NPCID.PrimeVice:
                case NPCID.PrimeSaw:
                case NPCID.Plantera:
                case NPCID.PlanterasTentacle:
                case NPCID.Golem:
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                case NPCID.CultistDragonHead:
                case NPCID.DD2OgreT2:
                case NPCID.DD2OgreT3:
                case NPCID.DD2Betsy:
                case NPCID.PumpkingBlade:
                case NPCID.SantaNK1:
                case NPCID.DukeFishron:
                case NPCID.BloodNautilus:
                case NPCID.HallowBoss:
                case NPCID.QueenSlimeBoss:
                case NPCID.Deerclops:
                    canBreakPlayerDefense = true;
                    break;

                // These go through walls and are very annoying with the new tombstone breaking spawning them mechanic in 1.4
                case NPCID.Ghost:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.5);
                    break;

                case NPCID.PirateGhost:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.33);
                    break;

                case NPCID.BloodSquid:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.25);
                    break;

                case NPCID.LarvaeAntlion:
                    npc.lifeMax = 15;
                    break;

                // Reduce prehardmode desert enemy stats
                case NPCID.WalkingAntlion:
                case NPCID.GiantWalkingAntlion:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * DesertEnemyStatMultiplier);
                    npc.damage = (int)Math.Round(npc.damage * DesertEnemyStatMultiplier);
                    npc.defDamage = npc.damage;
                    npc.defense /= 2;
                    npc.defDefense = npc.defense;
                    break;

                case NPCID.Antlion:
                case NPCID.FlyingAntlion:
                case NPCID.GiantFlyingAntlion:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * DesertEnemyStatMultiplier);
                    npc.damage = (int)Math.Round(npc.damage * DesertEnemyStatMultiplier);
                    npc.defDamage = npc.damage;
                    npc.defense /= 2;
                    npc.defDefense = npc.defense;
                    break;

                // Reduce Dungeon Guardian HP
                case NPCID.DungeonGuardian:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.1);
                    canBreakPlayerDefense = true;
                    break;

                // Reduce Tomb Crawler stats
                case NPCID.TombCrawlerHead:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.5);
                    npc.damage = (int)Math.Round(npc.damage * DesertEnemyStatMultiplier);
                    npc.defDamage = npc.damage;
                    break;

                case NPCID.TombCrawlerBody:
                case NPCID.TombCrawlerTail:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.5);
                    npc.damage = (int)Math.Round(npc.damage * DesertEnemyStatMultiplier);
                    npc.defDamage = npc.damage;
                    npc.defense /= 2;
                    npc.defDefense = npc.defense;
                    break;

                // Fix Sharkron hitboxes
                case NPCID.Sharkron:
                case NPCID.Sharkron2:
                    npc.width = npc.height = 36;
                    npc.chaseable = false;
                    break;

                // Fix drawing issues with Golem's Free Head
                case NPCID.GolemHeadFree:
                    npc.width = 88;
                    npc.height = 90;
                    break;

                // Make Core hitbox bigger and reduce HP
                case NPCID.MartianSaucerCore:
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.6);
                    npc.width *= 2;
                    npc.height *= 2;
                    break;

                // Nerf Green Jellyfish because they spawn in prehardmode
                case NPCID.GreenJellyfish:
                    npc.damage = 40;
                    npc.defDamage = npc.damage;
                    npc.defense = 4;
                    npc.defDefense = npc.defense;
                    break;

                // Make Plantera's Spores immune to damage because otherwise they're pointless
                case NPCID.Spore:
                    npc.dontTakeDamage = true;
                    break;

                // Make Fishron and Anahita Bubbles have actual health in Death Mode
                case NPCID.DetonatingBubble:
                    if (CalamityWorld.death)
                        npc.lifeMax = 300;
                    break;

                default:
                    break;
            }

            // Reduce mech boss HP and damage depending on the new ore progression changes
            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
            {
                if (!NPC.downedMechBossAny)
                {
                    if (CalamityNPCTypeSets.Destroyer.Contains(npc.type) || npc.type == NPCID.Probe || CalamityNPCTypeSets.SkeletronPrime.Contains(npc.type) || npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer)
                    {
                        double multiplier = Main.expertMode ? EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                        npc.lifeMax = (int)Math.Round(npc.lifeMax * multiplier);
                        npc.damage = (int)Math.Round(npc.damage * multiplier);
                        npc.defDamage = npc.damage;
                    }
                }
                else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                {
                    if (CalamityNPCTypeSets.Destroyer.Contains(npc.type) || npc.type == NPCID.Probe || CalamityNPCTypeSets.SkeletronPrime.Contains(npc.type) || npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer)
                    {
                        double multiplier = Main.expertMode ? EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                        npc.lifeMax = (int)Math.Round(npc.lifeMax * multiplier);
                        npc.damage = (int)Math.Round(npc.damage * multiplier);
                        npc.defDamage = npc.damage;
                    }
                }
            }

            // Prehardmode mushroom enemy nerfs
            if (!Main.hardMode)
            {
                if (npc.type == NPCID.ZombieMushroom || npc.type == NPCID.ZombieMushroomHat || npc.type == NPCID.AnomuraFungus || npc.type == NPCID.FungiBulb || npc.type == NPCID.MushiLadybug)
                {
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.5);
                    npc.damage = (int)Math.Round(npc.damage * 0.5);
                    npc.defDamage = npc.damage;
                }

                if (npc.type == NPCID.FungiSpore)
                {
                    npc.damage = (int)Math.Round(npc.damage * 0.5);
                    npc.defDamage = npc.damage;
                }
            }

            if (Main.hardMode && CalamityNPCSets.NerfDamageInHardmode[npc.type])
            {
                npc.damage = (int)Math.Round(npc.damage * 0.75);
                npc.defDamage = npc.damage;
            }

            if (DownedBossSystem.downedDoG)
            {
                if (CalamityNPCSets.IsBuffedPumpkinMoonEnemy[npc.type])
                {
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 3.5);
                    npc.damage += 30;
                    npc.life = npc.lifeMax;
                    npc.defDamage = npc.damage;
                }
                else if (CalamityNPCSets.IsBuffedFrostMoonEnemy[npc.type])
                {
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 2.5);
                    npc.damage += 30;
                    npc.life = npc.lifeMax;
                    npc.defDamage = npc.damage;
                }
                else if (CalamityNPCSets.IsBuffedSolarEclipseEnemy[npc.type])
                {
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 5D);
                    npc.damage += 30;
                    npc.life = npc.lifeMax;
                    npc.defDamage = npc.damage;
                }
            }

            if (NPC.downedMoonlord)
            {
                if (CalamityNPCSets.IsBuffedDungeonEnemy[npc.type])
                {
                    npc.lifeMax = (int)Math.Round(npc.lifeMax * 2.5);
                    npc.damage += 30;
                    npc.life = npc.lifeMax;
                    npc.defDamage = npc.damage;
                }
            }
        }
        #endregion

        #region Edit Coin Drops
        private void EditGlobalCoinDrops(NPC npc)
        {
            // Old Rev coin drop math: Normal = 10 Gold, Expert = 25 Gold, Rev = 37 Gold 50 Silver.
            // New Rev coin drop math: Normal = 15 Gold, Expert AND Rev = 22 Gold 50 Silver.
            // Rebalance coin drops so that Normal Mode enemies and bosses drop an adequate amount of coins.

            // Increase Normal Mode coin drops by 1.5x.
            npc.value = (int)(npc.value * NPCValueMultiplier_ClassicCalamity);

            // Change the Expert Mode coin drop multiplier.
            if (Main.expertMode)
            {
                // Undo the Expert Mode coin drop multiplier.
                npc.value = (int)(npc.value / NPCValueMultiplier_ExpertVanilla);

                // Change the Expert Mode coin drop multiplier to the new Calamity amount.
                npc.value = (int)(npc.value * NPCValueMultiplier_ExpertCalamity);
            }
        }
        #endregion

        #region Special Drawing
        public static void DrawGlowmask(NPC npc, SpriteBatch spriteBatch, Texture2D texture = null, bool invertedDirection = false, Vector2 offset = default)
        {
            if (texture is null)
                texture = TextureAssets.Npc[npc.type].Value;
            SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (invertedDirection)
                effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 screenOffset = npc.IsABestiaryIconDummy ? Vector2.Zero : Main.screenPosition;
            spriteBatch.Draw(texture,
                             npc.Center - screenOffset + offset,
                             npc.frame,
                             npc.GetAlpha(Color.White),
                             npc.rotation,
                             npc.frame.Size() * 0.5f,
                             npc.scale,
                             effects,
                             0f);
        }

        public static void DrawAfterimage(NPC npc, SpriteBatch spriteBatch, Color startingColor, Color endingColor, Texture2D texture = null, Func<NPC, int, float> rotationCalculation = null, bool directioning = false, bool invertedDirection = false)
        {
            if (NPCID.Sets.TrailingMode[npc.type] != 1)
                return;

            SpriteEffects spriteEffects = SpriteEffects.None;

            if (npc.spriteDirection == -1 && directioning)
                spriteEffects = SpriteEffects.FlipHorizontally;

            if (invertedDirection)
                spriteEffects ^= SpriteEffects.FlipHorizontally; // Same as x XOR 1, or x XOR TRUE, which inverts the bit. In this case, this reverses the horizontal flip

            // Set the rotation calculation to a predefined value. The null default is solely so that
            if (rotationCalculation is null)
                rotationCalculation = (nPC, afterimageIndex) => nPC.rotation;

            endingColor.A = 0;

            Color drawColor = npc.GetAlpha(startingColor);
            Texture2D npcTexture = texture ?? TextureAssets.Npc[npc.type].Value;
            Vector2 screenOffset = npc.IsABestiaryIconDummy ? Vector2.Zero : Main.screenPosition;
            int afterimageCounter = 1;
            while (afterimageCounter < NPCID.Sets.TrailCacheLength[npc.type] && CalamityClientConfig.Instance.Afterimages)
            {
                Color colorToDraw = Color.Lerp(drawColor, endingColor, afterimageCounter / (float)NPCID.Sets.TrailCacheLength[npc.type]);
                colorToDraw *= afterimageCounter / (float)NPCID.Sets.TrailCacheLength[npc.type];
                spriteBatch.Draw(npcTexture,
                                 npc.oldPos[afterimageCounter] + npc.Size / 2f - screenOffset + Vector2.UnitY * npc.gfxOffY,
                                 npc.frame,
                                 colorToDraw,
                                 rotationCalculation.Invoke(npc, afterimageCounter),
                                 npc.frame.Size() * 0.5f,
                                 npc.scale,
                                 spriteEffects,
                                 0f);
                afterimageCounter++;
            }
        }
        #endregion

        #region Scale Expert Multiplayer Stats
        private const float VanillaScalingFactor_2Players = 1.35f;
        private const float VanillaScalingFactor_3Players = 1.9166666666666666f;

        /// <summary>
        /// Applies Calamity's adjustments to difficulty-based player count stat scaling for NPCs. Calamity only adjusts the health of NPCs and does not touch any other stats.
        /// </summary>
        /// <param name="npc">The NPC which is having its stats adjusted.</param>
        /// <param name="numPlayers">The number of players considered active for the purposes of stat scaling.</param>
        /// <param name="balance">The vanilla Expert+ multiplayer health scalar value.</param>
        /// <param name="bossAdjustment">An arbitrary float to make Master Mode easier. On Master Mode, it is 0.85, otherwise it is 1.0.</param>
        public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
        {
            // Do absolutely nothing in single player, or in multiplayer with only one player connected.
            if (Main.netMode == NetmodeID.SinglePlayer || numPlayers <= 1)
                return;

            bool countsAsBoss = npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
            bool scalesLikeBoss = countsAsBoss || CalamityNPCSets.ScalesHealthLikeBoss[npc.type];
            bool isCalamityNPC = npc.ModNPC != null && npc.ModNPC.Mod == CalamityMod.Instance;

            // 14APR2025: Ozzatron: Reworked how Calamity changes the health of Expert+ multiplayer bosses
            // Non-boss enemies that receive scaling in Expert+ are still reduced via the old formula
            //
            // TL;DR:
            // - 2 players goes from 135% health to 175% health
            // - 3 players goes from 191.6% health to 225% health
            // - 4 players and beyond are unedited (4 players is 262.8% for reference)

            // This case applies to all bosses: vanilla, Calamity, and other mods, and anything that is supposed to scale like a boss.
            if (countsAsBoss || scalesLikeBoss)
            {
                double adjustmentFactor = 1.0;

                // The 2-player boss case is too easy; 1.35x health does not even come close to justify being able to respawn.
                if (numPlayers == 2)
                    adjustmentFactor = BalancingConstants.ExpertHealthScalingOverride_2Players / VanillaScalingFactor_2Players;

                // Similarly, the 3-player boss case is too easy, given the considerably higher damage output available.
                else if (numPlayers == 3)
                    adjustmentFactor = BalancingConstants.ExpertHealthScalingOverride_3Players / VanillaScalingFactor_3Players;

                // Cases beyond 3 players are already sufficiently scaled by vanilla and continue to scale harder with more players.

                // Apply the adjustment factor, if any. No other changes are made to bosses or boss-like NPCs.
                npc.life = (int)Math.Round(npc.life * adjustmentFactor);
                return;
            }

            // Do not touch non-boss NPCs from vanilla or other mods.
            if (!isCalamityNPC)
                return;

            // Reduction to multiplayer HP scaling for non-boss Calamity enemies in Expert+
            double scalar;
            switch (numPlayers)
            {
                case 1:
                    scalar = 1.0;
                    break;

                case 2:
                    scalar = 0.9; // 1.8
                    break;

                case 3:
                    scalar = 0.82; // 2.46
                    break;

                case 4:
                    scalar = 0.76; // 3.04
                    break;

                case 5:
                    scalar = 0.71; // 3.55
                    break;

                case 6:
                    scalar = 0.67; // 4.02
                    break;

                default:
                    scalar = 0.64; // 4.48 + 0.64 per player beyond 7
                    break;
            }

            npc.lifeMax = (int)Math.Round(npc.lifeMax * scalar);
        }
        #endregion

        #region Can Hit Player
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (pacified)
                return false;

            if (target.Calamity().prismaticHelmet && !CalamityPlayer.areThereAnyDamnBosses)
            {
                if (npc.lifeMax < 500)
                    return false;
            }

            return true;
        }
        #endregion

        #region Strike NPC
        // Incoming defense to this function is already affected by the vanilla debuffs Ichor (-10) and Betsy's Curse (-40), and cannot be below zero.
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // Reduce ichor debuff defense reduction from -15 to -10.
            if (npc.ichor)
                modifiers.Defense.Flat += 5;

            // Apply armor penetration based on Calamity debuffs. The hit system manages the sequencing.
            // Ozzatron 05JAN2023: fixed doubled armor pen, this time for real
            int defenseReduction = (markedForDeath && DR <= 0f ? MarkedforDeath.DefenseReduction : 0) + (wither ? RemsRevenge.WitherDefenseReduction : 0) + miscDefenseLoss;
            modifiers.ArmorPenetration += defenseReduction;

            // DR applies after vanilla defense.
            ApplyDR(npc, ref modifiers);

            // Damage reduction on spawn for certain worm bosses.
            if (CalamityWorld.revenge)
            {
                if (CalamityNPCTypeSets.EaterOfWorlds.Contains(npc.type) && newAI[1] < EaterOfWorldsAI.DRIncreaseTime)
                    modifiers.FinalDamage *= 1f - (float)Math.Sqrt(MathHelper.Lerp(0f, 0.99f, MathHelper.Clamp(1f - newAI[1] / EaterOfWorldsAI.DRIncreaseTime, 0f, 1f)));
                if (CalamityNPCTypeSets.Destroyer.Contains(npc.type) && newAI[1] < DestroyerAI.DRIncreaseTime)
                    modifiers.FinalDamage *= 1f - (float)Math.Sqrt(MathHelper.Lerp(0f, 0.99f, MathHelper.Clamp(1f - newAI[1] / DestroyerAI.DRIncreaseTime, 0f, 1f)));
            }
            if (CalamityNPCTypeSets.AstrumDeus.Contains(npc.type))
            {
                float drTime = newAI[0] != 0f ? 300f : 600f;
                if (newAI[1] < drTime)
                    modifiers.FinalDamage *= 1f - (float)Math.Sqrt(MathHelper.Lerp(0f, 0.99f, MathHelper.Clamp(1f - newAI[1] / drTime, 0f, 1f)));
            }
        }

        // Directly modifies final damage incoming to an NPC based on their DR (damage reduction) stat added by Calamity.
        // This is entirely separate from vanilla's takenDamageMultiplier.
        private void ApplyDR(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (DR <= 0f && KillTime == 0)
                return;

            float finalMultiplier = 1f;

            // If the NPC currently has unbreakable DR, it cannot be reduced by any means.
            // If custom DR is enabled, use that instead of normal DR.
            float effectiveDR = unbreakableDR ? DR : (customDR ? CustomDRMath(npc, DR) : DefaultDRMath(npc, DR));

            // DR floor is 0%. Nothing can have negative DR.
            if (effectiveDR <= 0f)
                effectiveDR = 0f;

            // Calculate extra DR based on kill time, similar to the Hush boss from The Binding of Isaac
            bool enragedProvi = npc.type == NPCType<Providence.Providence>() && !ProvUtils.StandardAI();
            bool dayEmpress = npc.type == NPCID.HallowBoss && NPC.ShouldEmpressBeEnraged();
            if (KillTime > 0 && AITimer < KillTime && !BossRushEvent.BossRushActive && (enragedProvi || dayEmpress))
            {
                // Set the DR scaling factor
                float DRScalar = 10f;

                // The limit for how much extra DR the boss can have
                float extraDRLimit = (1f - DR) * DRScalar;

                // Ranges from 1 to 0
                float currentHPRatio = npc.life / (float)npc.lifeMax;

                // Ranges from 0 to 1
                float killTimeRatio = AITimer / (float)KillTime;

                // If the player is damaging the boss too quickly
                float extraDRScalar = currentHPRatio + killTimeRatio;
                if (extraDRScalar < 1f)
                {
                    // Ranges from 0 to (extraDRLimit / 2)
                    effectiveDR += extraDRLimit - (extraDRLimit / (1f + (1f - extraDRScalar)));
                }
            }

            // Final DR calculation
            finalMultiplier -= effectiveDR;

            modifiers.FinalDamage *= finalMultiplier;
        }

        //TODO:
        //This will need to be adjusted to use DebuffData in the future.
        //However, we still need to decide what to actually do with these due to the general flattening of DR amounts and removal from most enemies
        //This means that this will be handled in the future
        private float DefaultDRMath(NPC npc, float DR)
        {
            float calcDR = DR;
            if (markedForDeath)
                calcDR *= 0.5f;
            if (absorberAffliction)
                calcDR *= 0.8f;
            if (npc.Calamity().armorCrunch)
                calcDR *= ArmorCrunch.MultiplicativeDamageReductionEnemy;
            if (npc.Calamity().crumble)
                calcDR *= Crumbling.MultiplicativeDamageReductionEnemy;
            if (relicOfResilienceWeakness)
                calcDR *= 0.5f;


            return calcDR;
        }

        private float CustomDRMath(NPC npc, float DR)
        {
            void FlatEditDR(ref float theDR, bool npcHasDebuff, int buffID)
            {
                if (npcHasDebuff && flatDRReductions.TryGetValue(buffID, out float reduction))
                    theDR -= reduction;
            }
            void MultEditDR(ref float theDR, bool npcHasDebuff, int buffID)
            {
                if (npcHasDebuff && multDRReductions.TryGetValue(buffID, out float multiplier))
                    theDR *= multiplier;
            }

            float calcDR = DR;

            // Apply flat reductions first. All vanilla debuffs check their internal booleans.
            FlatEditDR(ref calcDR, npc.poisoned, BuffID.Poisoned);
            FlatEditDR(ref calcDR, npc.onFire, BuffID.OnFire);
            FlatEditDR(ref calcDR, npc.onFire3, BuffID.OnFire3);
            FlatEditDR(ref calcDR, npc.venom, BuffID.Venom);
            FlatEditDR(ref calcDR, npc.onFrostBurn, BuffID.Frostburn);
            FlatEditDR(ref calcDR, npc.shadowFlame, BuffID.ShadowFlame);
            FlatEditDR(ref calcDR, npc.daybreak, BuffID.Daybreak);
            FlatEditDR(ref calcDR, npc.onFire2, BuffID.CursedInferno);

            // Modded debuffs are handled modularly and use HasBuff.
            foreach (KeyValuePair<int, float> entry in flatDRReductions)
            {
                int buffID = entry.Key;
                if (buffID >= BuffID.Count && npc.HasBuff(buffID))
                    calcDR -= entry.Value;
            }

            // Apply multiplicative reductions second. All vanilla debuffs check their internal booleans.
            MultEditDR(ref calcDR, npc.poisoned, BuffID.Poisoned);
            MultEditDR(ref calcDR, npc.onFire, BuffID.OnFire);
            MultEditDR(ref calcDR, npc.onFire3, BuffID.OnFire3);
            MultEditDR(ref calcDR, npc.venom, BuffID.Venom);
            MultEditDR(ref calcDR, npc.onFrostBurn, BuffID.Frostburn);
            MultEditDR(ref calcDR, npc.shadowFlame, BuffID.ShadowFlame);
            MultEditDR(ref calcDR, npc.daybreak, BuffID.Daybreak);
            MultEditDR(ref calcDR, npc.onFire2, BuffID.CursedInferno);

            // Modded debuffs are handled modularly and use HasBuff.
            foreach (KeyValuePair<int, float> entry in multDRReductions)
            {
                int buffID = entry.Key;
                if (buffID >= BuffID.Count && npc.HasBuff(buffID))
                    calcDR *= entry.Value;
            }

            return calcDR;
        }

        public bool IsArmored()
        {
            return unbreakableDR && DR > 0.9f;
        }
        #endregion

        #region Pre AI
        public override bool PreAI(NPC npc)
        {
            // Change Spaz and Ret weaknesses and resistances when phase 2 starts.
            if (npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer)
            {
                if (npc.ai[0] >= 2f)
                {
                    VulnerableToCold = null;
                    VulnerableToHeat = null;
                    VulnerableToSickness = false;
                    VulnerableToElectricity = true;
                }
            }

            if (VulnerabilityHexFireDrawer != null)
                VulnerabilityHexFireDrawer.Update();

            if (ManaBurnFireDrawer != null)
            {
                ManaBurnFireDrawer.LocalTimer = 0;
                float power = npc.height / 100f;
                if (power > 2.75f)
                    power = 2.75f;
                ManaBurnFireDrawer.RelativePower = power * MathHelper.Lerp(0.5f, 1.5f, MathHelper.Clamp(manaBurn / manaBurnPeak, 0, 1)) * playerManaBurnIntensity;
                ManaBurnFireDrawer.Update();
            }

            SetPatreonTownNPCName(npc, Mod);

            // Decrement each immune timer if it's greater than 0.
            for (int i = 0; i < maxPlayerImmunities; i++)
            {
                if (dashImmunityTime[i] > 0)
                    dashImmunityTime[i]--;
            }

            if (KillTime > 0 || npc.type == NPCType<Draedon>())
            {
                // Apply Boss Effects while any boss NPC is active
                if (!Main.dedServ)
                {
                    if (!Main.LocalPlayer.dead && Main.LocalPlayer.active && Vector2.Distance(Main.LocalPlayer.Center, npc.Center) < BossZenDistance)
                        Main.LocalPlayer.AddBuff(BuffType<BossEffects>(), 2);
                }

                if (npc.type != NPCType<Draedon>())
                {
                    if (AITimer < KillTime)
                        AITimer++;
                }
            }

            if (npc.type == NPCID.TargetDummy || npc.type == NPCType<SuperDummyNPC>())
            {
                npc.dontTakeDamage = CalamityPlayer.areThereAnyDamnBosses;

                if (draedon != -1)
                {
                    if (Main.npc[draedon].active)
                        npc.dontTakeDamage = true;
                }
            }

            // Setting this in SetDefaults will disable expert mode scaling, so put it here instead
            if (CalamityNPCSets.DealsZeroContactDamage[npc.type] && !(npc.type == NPCID.RuneWizard && Main.zenithWorld))
                npc.damage = 0;

            // Don't do damage for 42 frames after spawning in
            if (npc.type == NPCID.Sharkron || npc.type == NPCID.Sharkron2)
                npc.damage = npc.alpha > 0 ? 0 : npc.defDamage;

            if (BossRushEvent.BossRushActive && !npc.friendly && !npc.townNPC && !npc.Calamity().DoesNotDisappearInBossRush)
                BossRushForceDespawnOtherNPCs(npc, Mod);

            if (NPC.LunarApocalypseIsUp)
                PillarEventProgressionEdit(npc);

            // Adult Wyrm Ancient Doom
            if (npc.type == NPCID.AncientDoom)
            {
                if (Main.npc[(int)npc.ai[0]].type == NPCType<PrimordialWyrmHead>())
                    return CultistAI.BuffedAncientDoomAI(npc, Mod);
            }

            // Completely override the shitty AI and replace it
            if (npc.type == NPCID.BloodNautilus)
                return DreadnautilusAI.BuffedDreadnautilusAI(npc, Mod);

            // Decrease the projectile velocities of several fighter enemies and make them better to fight in general
            // Also limit the amount of times Vortex Larvae and Hornets can evolve
            if (npc.type == NPCID.IceGolem || npc.type == NPCID.Eyezor || npc.type == NPCID.VortexRifleman ||
                npc.type == NPCID.TacticalSkeleton || npc.type == NPCID.Nailhead || npc.type == NPCID.WallCreeper ||
                npc.type == NPCID.BloodCrawler || npc.type == NPCID.BlackRecluse || npc.type == NPCID.JungleCreeper ||
                npc.type == NPCID.BoneLee || npc.type == NPCID.VortexLarva || npc.type == NPCID.VortexHornet ||
                npc.type == NPCID.VortexHornetQueen)
            {
                return RevengeanceAndDeathAI.BuffedFighterAI(npc, Mod);
            }

            // More telegraphs
            if (npc.type == NPCID.Harpy || npc.type == NPCID.Demon || npc.type == NPCID.VoodooDemon ||
                npc.type == NPCID.RedDevil)
            {
                return RevengeanceAndDeathAI.BuffedBatAI(npc, Mod);
            }

            // Casters hold their hands up for longer before firing in all modes
            if (npc.type == NPCID.FireImp || npc.type == NPCID.DarkCaster || npc.type == NPCID.Tim ||
                npc.type == NPCID.RuneWizard || (npc.type >= NPCID.RaggedCaster && npc.type <= NPCID.DiabolistWhite) ||
                npc.type == NPCID.DesertDjinn || npc.type == NPCID.GoblinSorcerer)
            {
                return RevengeanceAndDeathAI.BuffedCasterAI(npc, Mod);
            }

            // Antlion telegraph
            if (npc.type == NPCID.Antlion)
                return RevengeanceAndDeathAI.BuffedAntlionAI(npc, Mod);

            // Corruptor and Blood Squid telegraphs
            if (npc.type == NPCID.Corruptor || npc.type == NPCID.BloodSquid)
                return RevengeanceAndDeathAI.BuffedFlyingAI(npc, Mod);

            // Ichor Sticker and Ice Elemental telegraphs
            if (npc.type == NPCID.IchorSticker || npc.type == NPCID.IceElemental)
                return RevengeanceAndDeathAI.BuffedHoveringAI(npc, Mod);

            // Fungi Bulb telegraphs
            if (npc.type == NPCID.FungiBulb || npc.type == NPCID.GiantFungiBulb)
                return RevengeanceAndDeathAI.BuffedPlantAI(npc, Mod);

            // Spider web spit telegraph
            if (npc.type == NPCID.WallCreeperWall || npc.type == NPCID.BloodCrawlerWall || npc.type == NPCID.BlackRecluseWall ||
                npc.type == NPCID.JungleCreeperWall)
            {
                return RevengeanceAndDeathAI.BuffedSpiderAI(npc, Mod);
            }

            // Servant of Cthulhu light
            if (npc.type == NPCID.ServantofCthulhu)
                Lighting.AddLight(npc.Center, 0.2f, 0.2f, 0.2f);

            if (npc.type == NPCID.CultistBoss || npc.type == NPCID.CultistBossClone)
            {
                if (npc.type == NPCID.CultistBossClone)
                {
                    if (Main.npc[(int)npc.ai[3]].active)
                    {
                        // Emit light
                        float lifeRatio = Main.npc[(int)npc.ai[3]].life / (float)Main.npc[(int)npc.ai[3]].lifeMax;
                        float colorTransitionAmt = (float)Math.Pow((double)(1f - lifeRatio), 2D);
                        Color lightColor = Color.Lerp(Color.Cyan, Color.Blue, colorTransitionAmt);
                        Lighting.AddLight(npc.Center, lightColor.R / 255f, lightColor.G / 255f, lightColor.B / 255f);
                    }
                }
                else
                {
                    // Emit light
                    float lifeRatio = npc.life / (float)npc.lifeMax;
                    float colorTransitionAmt = (float)Math.Pow((double)(1f - lifeRatio), 2D);
                    Color lightColor = Color.Lerp(Color.Cyan, Color.Blue, colorTransitionAmt);
                    Lighting.AddLight(npc.Center, lightColor.R / 255f, lightColor.G / 255f, lightColor.B / 255f);

                    // Decrement the hit counter for the shield flicker
                    if (newAI[1] > 0f)
                        newAI[1] -= 1f;

                    // Cultist shield hitbox
                    Vector2 hitboxSize = new Vector2(216f / 1.4142f);
                    if (npc.Size != hitboxSize)
                        npc.Size = hitboxSize;
                }
            }

            if (Main.zenithWorld)
            {
                if (npc.type == NPCID.QueenBee)
                    return QueenBeeAI.BuffedQueenBeeAI(npc, Mod);
            }

            if (CalamityWorld.death)
            {
                if (npc.type == NPCID.DetonatingBubble)
                    return DukeFishronAI.BuffedDetonatingBubbleAI(npc, Mod);
            }

            if (CalamityWorld.revenge || BossRushEvent.BossRushActive)
            {
                switch (npc.type)
                {
                    case NPCID.KingSlime:
                        return KingSlimeAI.BuffedKingSlimeAI(npc, Mod);

                    case NPCID.EyeofCthulhu:
                        return EyeOfCthulhuAI.BuffedEyeofCthulhuAI(npc, Mod);

                    case NPCID.EaterofWorldsHead:
                    case NPCID.EaterofWorldsBody:
                    case NPCID.EaterofWorldsTail:
                        return EaterOfWorldsAI.BuffedEaterofWorldsAI(npc, Mod);

                    case NPCID.BrainofCthulhu:
                        return BrainOfCthulhuAI.BuffedBrainofCthulhuAI(npc, Mod);
                    case NPCID.Creeper:
                        return BrainOfCthulhuAI.BuffedCreeperAI(npc, Mod);

                    case NPCID.QueenBee:
                        return QueenBeeAI.BuffedQueenBeeAI(npc, Mod);

                    case NPCID.SkeletronHand:
                        return SkeletronAI.BuffedSkeletronHandAI(npc, Mod);
                    case NPCID.SkeletronHead:
                        return SkeletronAI.BuffedSkeletronAI(npc, Mod);

                    case NPCID.Deerclops:
                        return DeerclopsAI.BuffedDeerclopsAI(npc, Mod);

                    case NPCID.WallofFlesh:
                        return WallOfFleshAI.BuffedWallofFleshAI(npc, Mod);
                    case NPCID.WallofFleshEye:
                        return WallOfFleshAI.BuffedWallofFleshEyeAI(npc, Mod);

                    case NPCID.QueenSlimeBoss:
                        return QueenSlimeAI.BuffedQueenSlimeAI(npc, Mod);
                    case NPCID.QueenSlimeMinionBlue:
                        return QueenSlimeAI.BuffedQueenSlimeCrystalSlimeAI(npc, Mod);
                    case NPCID.QueenSlimeMinionPink:
                        return QueenSlimeAI.BuffedQueenSlimeBouncySlimeAI(npc, Mod);

                    case NPCID.TheDestroyer:
                    case NPCID.TheDestroyerBody:
                    case NPCID.TheDestroyerTail:
                        return DestroyerAI.BuffedDestroyerAI(npc, Mod);
                    case NPCID.Probe:
                        return DestroyerAI.BuffedProbeAI(npc, Mod);

                    case NPCID.Retinazer:
                        return TwinsAI.BuffedRetinazerAI(npc, Mod);
                    case NPCID.Spazmatism:
                        return TwinsAI.BuffedSpazmatismAI(npc, Mod);

                    case NPCID.SkeletronPrime:
                        return SkeletronPrimeAI.BuffedSkeletronPrimeAI(npc, Mod);
                    case NPCID.PrimeLaser:
                        return SkeletronPrimeAI.BuffedPrimeLaserAI(npc, Mod);
                    case NPCID.PrimeCannon:
                        return SkeletronPrimeAI.BuffedPrimeCannonAI(npc, Mod);
                    case NPCID.PrimeVice:
                        return SkeletronPrimeAI.BuffedPrimeViceAI(npc, Mod);
                    case NPCID.PrimeSaw:
                        return SkeletronPrimeAI.BuffedPrimeSawAI(npc, Mod);

                    case NPCID.Plantera:
                        return PlanteraAI.BuffedPlanteraAI(npc, Mod);
                    case NPCID.PlanterasHook:
                        return PlanteraAI.BuffedPlanterasHookAI(npc, Mod);
                    case NPCID.PlanterasTentacle:
                        return PlanteraAI.BuffedPlanterasTentacleAI(npc, Mod);

                    case NPCID.HallowBoss:
                        return EmpressofLightAI.BuffedEmpressofLightAI(npc, Mod);

                    case NPCID.Golem:
                        return GolemAI.BuffedGolemAI(npc, Mod);
                    case NPCID.GolemFistLeft:
                    case NPCID.GolemFistRight:
                        return GolemAI.BuffedGolemFistAI(npc, Mod);
                    case NPCID.GolemHead:
                        return GolemAI.BuffedGolemHeadAI(npc, Mod);
                    case NPCID.GolemHeadFree:
                        return GolemAI.BuffedGolemHeadFreeAI(npc, Mod);

                    case NPCID.DukeFishron:
                        return DukeFishronAI.BuffedDukeFishronAI(npc, Mod);

                    case NPCID.Pumpking:
                        if (DownedBossSystem.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedPumpkingAI(npc);
                        }

                        break;

                    case NPCID.PumpkingBlade:
                        if (DownedBossSystem.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedPumpkingBladeAI(npc);
                        }

                        break;

                    case NPCID.IceQueen:
                        if (DownedBossSystem.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedIceQueenAI(npc);
                        }

                        break;

                    case NPCID.Mothron:
                        if (DownedBossSystem.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedMothronAI(npc);
                        }

                        break;

                    case NPCID.CultistBoss:
                    case NPCID.CultistBossClone:
                        return CultistAI.BuffedCultistAI(npc, Mod);
                    case NPCID.AncientLight:
                        return CultistAI.BuffedAncientLightAI(npc, Mod);
                    case NPCID.AncientDoom:
                        return CultistAI.BuffedAncientDoomAI(npc, Mod);

                    case NPCID.MoonLordCore:
                    case NPCID.MoonLordHand:
                    case NPCID.MoonLordHead:
                    case NPCID.MoonLordFreeEye:
                    case NPCID.MoonLordLeechBlob:
                        return MoonLordAI.BuffedMoonLordAI(npc, Mod);

                    default:
                        break;
                }
            }

            if (CalamityWorld.revenge)
            {
                switch (npc.aiStyle)
                {
                    case NPCAIStyleID.Slime:
                        if (npc.type == NPCType<BloomSlime>() || npc.type == NPCType<InfernalCongealment>() ||
                            npc.type == NPCType<CrimulanBlightSlime>() || npc.type == NPCType<CryoSlime>() ||
                            npc.type == NPCType<EbonianBlightSlime>() || npc.type == NPCType<PerennialSlime>() ||
                            npc.type == NPCType<IrradiatedSlime>() || npc.type == NPCType<AstralSlime>())
                        {
                            return SlimeAI.BuffedSlimeAI(npc, Mod);
                        }
                        else
                        {
                            switch (npc.type)
                            {
                                case NPCID.BlueSlime:
                                case NPCID.MotherSlime:
                                case NPCID.LavaSlime:
                                case NPCID.DungeonSlime:
                                case NPCID.CorruptSlime:
                                case NPCID.IlluminantSlime:
                                case NPCID.ToxicSludge:
                                case NPCID.IceSlime:
                                case NPCID.Crimslime:
                                case NPCID.SpikedIceSlime:
                                case NPCID.SpikedJungleSlime:
                                case NPCID.UmbrellaSlime:
                                case NPCID.RainbowSlime:
                                case NPCID.SlimeMasked:
                                case NPCID.HoppinJack:
                                case NPCID.SlimeRibbonWhite:
                                case NPCID.SlimeRibbonYellow:
                                case NPCID.SlimeRibbonGreen:
                                case NPCID.SlimeRibbonRed:
                                case NPCID.SandSlime:
                                case NPCID.SlimeSpiked:
                                case NPCID.GoldenSlime:
                                case NPCID.ShimmerSlime:
                                    return SlimeAI.BuffedSlimeAI(npc, Mod);
                            }
                        }
                        break;

                    case NPCAIStyleID.DemonEye:
                        if (npc.type == NPCType<CalamityEye>())
                        {
                            return DemonEyeAI.BuffedDemonEyeAI(npc, Mod);
                        }
                        else
                        {
                            switch (npc.type)
                            {
                                case NPCID.DemonEye:
                                case NPCID.TheHungryII:
                                case NPCID.WanderingEye:
                                case NPCID.PigronCorruption:
                                case NPCID.PigronHallow:
                                case NPCID.PigronCrimson:
                                case NPCID.CataractEye:
                                case NPCID.SleepyEye:
                                case NPCID.DialatedEye:
                                case NPCID.GreenEye:
                                case NPCID.PurpleEye:
                                case NPCID.DemonEyeOwl:
                                case NPCID.DemonEyeSpaceship:
                                    return DemonEyeAI.BuffedDemonEyeAI(npc, Mod);
                            }
                        }
                        break;

                    case NPCAIStyleID.Fighter:
                        if (npc.type == NPCType<Stormlion>() ||
                            npc.type == NPCType<AstralachneaGround>() || npc.type == NPCType<RenegadeWarlock>())
                        {
                            return RevengeanceAndDeathAI.BuffedFighterAI(npc, Mod);
                        }
                        else
                        {
                            switch (npc.type)
                            {
                                case NPCID.Zombie:
                                case NPCID.ArmedZombie:
                                case NPCID.ArmedZombieEskimo:
                                case NPCID.ArmedZombiePincussion:
                                case NPCID.ArmedZombieSlimed:
                                case NPCID.ArmedZombieSwamp:
                                case NPCID.ArmedZombieTwiggy:
                                case NPCID.ArmedZombieCenx:
                                case NPCID.Skeleton:
                                case NPCID.SporeSkeleton:
                                case NPCID.AngryBones:
                                case NPCID.UndeadMiner:
                                case NPCID.CorruptBunny:
                                case NPCID.DoctorBones:
                                case NPCID.TheGroom:
                                case NPCID.Crab:
                                case NPCID.GoblinScout:
                                case NPCID.ArmoredSkeleton:
                                case NPCID.Mummy:
                                case NPCID.DarkMummy:
                                case NPCID.LightMummy:
                                case NPCID.Werewolf:
                                case NPCID.Clown:
                                case NPCID.SkeletonArcher:
                                case NPCID.ChaosElemental:
                                case NPCID.BaldZombie:
                                case NPCID.PossessedArmor:
                                case NPCID.ZombieEskimo:
                                case NPCID.UndeadViking:
                                case NPCID.CorruptPenguin:
                                case NPCID.FaceMonster:
                                case NPCID.SnowFlinx:
                                case NPCID.PincushionZombie:
                                case NPCID.SlimedZombie:
                                case NPCID.SwampZombie:
                                case NPCID.TwiggyZombie:
                                case NPCID.Nymph:
                                case NPCID.ArmoredViking:
                                case NPCID.Lihzahrd:
                                case NPCID.LihzahrdCrawler:
                                case NPCID.FemaleZombie:
                                case NPCID.HeadacheSkeleton:
                                case NPCID.MisassembledSkeleton:
                                case NPCID.PantlessSkeleton:
                                case NPCID.IcyMerman:
                                case NPCID.PirateDeckhand:
                                case NPCID.PirateCorsair:
                                case NPCID.PirateDeadeye:
                                case NPCID.PirateCrossbower:
                                case NPCID.PirateCaptain:
                                case NPCID.CochinealBeetle:
                                case NPCID.CyanBeetle:
                                case NPCID.LacBeetle:
                                case NPCID.SeaSnail:
                                case NPCID.ZombieRaincoat:
                                case NPCID.ZombieMushroom:
                                case NPCID.ZombieMushroomHat:
                                case NPCID.AnomuraFungus:
                                case NPCID.MushiLadybug:
                                case NPCID.RustyArmoredBonesAxe:
                                case NPCID.RustyArmoredBonesFlail:
                                case NPCID.RustyArmoredBonesSword:
                                case NPCID.RustyArmoredBonesSwordNoArmor:
                                case NPCID.BlueArmoredBones:
                                case NPCID.BlueArmoredBonesMace:
                                case NPCID.BlueArmoredBonesNoPants:
                                case NPCID.BlueArmoredBonesSword:
                                case NPCID.HellArmoredBones:
                                case NPCID.HellArmoredBonesSpikeShield:
                                case NPCID.HellArmoredBonesMace:
                                case NPCID.HellArmoredBonesSword:
                                case NPCID.Paladin:
                                case NPCID.SkeletonSniper:
                                case NPCID.SkeletonCommando:
                                case NPCID.AngryBonesBig:
                                case NPCID.AngryBonesBigMuscle:
                                case NPCID.AngryBonesBigHelmet:
                                case NPCID.Scarecrow1:
                                case NPCID.Scarecrow2:
                                case NPCID.Scarecrow3:
                                case NPCID.Scarecrow4:
                                case NPCID.Scarecrow5:
                                case NPCID.Scarecrow6:
                                case NPCID.Scarecrow7:
                                case NPCID.Scarecrow8:
                                case NPCID.Scarecrow9:
                                case NPCID.Scarecrow10:
                                case NPCID.ZombieDoctor:
                                case NPCID.ZombieSuperman:
                                case NPCID.ZombiePixie:
                                case NPCID.SkeletonTopHat:
                                case NPCID.SkeletonAstonaut:
                                case NPCID.SkeletonAlien:
                                case NPCID.Splinterling:
                                case NPCID.ZombieXmas:
                                case NPCID.ZombieSweater:
                                case NPCID.ZombieElf:
                                case NPCID.ZombieElfBeard:
                                case NPCID.ZombieElfGirl:
                                case NPCID.GingerbreadMan:
                                case NPCID.Yeti:
                                case NPCID.Nutcracker:
                                case NPCID.NutcrackerSpinning:
                                case NPCID.ElfArcher:
                                case NPCID.Krampus:
                                case NPCID.CultistArcherBlue:
                                case NPCID.CultistArcherWhite:
                                case NPCID.BrainScrambler:
                                case NPCID.RayGunner:
                                case NPCID.MartianOfficer:
                                case NPCID.GrayGrunt:
                                case NPCID.MartianEngineer:
                                case NPCID.GigaZapper:
                                case NPCID.Scutlix:
                                case NPCID.BoneThrowingSkeleton:
                                case NPCID.BoneThrowingSkeleton2:
                                case NPCID.BoneThrowingSkeleton3:
                                case NPCID.BoneThrowingSkeleton4:
                                case NPCID.CrimsonBunny:
                                case NPCID.CrimsonPenguin:
                                case NPCID.Medusa:
                                case NPCID.GreekSkeleton:
                                case NPCID.GraniteGolem:
                                case NPCID.BloodZombie:
                                case NPCID.Crawdad:
                                case NPCID.Crawdad2:
                                case NPCID.Salamander:
                                case NPCID.Salamander2:
                                case NPCID.Salamander3:
                                case NPCID.Salamander4:
                                case NPCID.Salamander5:
                                case NPCID.Salamander6:
                                case NPCID.Salamander7:
                                case NPCID.Salamander8:
                                case NPCID.Salamander9:
                                case NPCID.GiantWalkingAntlion:
                                case NPCID.WalkingAntlion:
                                case NPCID.LarvaeAntlion:
                                case NPCID.DesertGhoul:
                                case NPCID.DesertGhoulCorruption:
                                case NPCID.DesertGhoulCrimson:
                                case NPCID.DesertGhoulHallow:
                                case NPCID.DesertLamiaLight:
                                case NPCID.DesertLamiaDark:
                                case NPCID.DesertScorpionWalk:
                                case NPCID.DesertBeast:
                                case NPCID.StardustSoldier:
                                case NPCID.StardustSpiderBig:
                                case NPCID.NebulaSoldier:
                                case NPCID.VortexSoldier:
                                case NPCID.SolarDrakomire:
                                case NPCID.SolarSpearman:
                                case NPCID.SolarSolenian:
                                case NPCID.Frankenstein:
                                case NPCID.SwampThing:
                                case NPCID.Vampire:
                                case NPCID.Butcher:
                                case NPCID.CreatureFromTheDeep:
                                case NPCID.Fritz:
                                case NPCID.Psycho:
                                case NPCID.ThePossessed:
                                case NPCID.DrManFly:
                                case NPCID.GoblinPeon:
                                case NPCID.GoblinThief:
                                case NPCID.GoblinWarrior:
                                case NPCID.GoblinArcher:
                                case NPCID.GoblinSummoner:
                                case NPCID.MartianWalker:
                                case NPCID.DemonTaxCollector:
                                case NPCID.TheBride:
                                    return RevengeanceAndDeathAI.BuffedFighterAI(npc, Mod);
                            }
                        }
                        break;

                    case NPCAIStyleID.Flying:
                        switch (npc.type)
                        {
                            case NPCID.ServantofCthulhu:
                            case NPCID.EaterofSouls:
                            case NPCID.MeteorHead:
                            case NPCID.Crimera:
                            case NPCID.Moth:
                            case NPCID.Parrot:
                            case NPCID.Bee:
                            case NPCID.BeeSmall:
                            case NPCID.Hornet:
                            case NPCID.HornetFatty:
                            case NPCID.HornetHoney:
                            case NPCID.HornetLeafy:
                            case NPCID.HornetSpikey:
                            case NPCID.HornetStingy:
                            case NPCID.MossHornet:
                                return RevengeanceAndDeathAI.BuffedFlyingAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.Worm:
                        switch (npc.type)
                        {
                            case NPCID.DevourerHead:
                            case NPCID.DevourerBody:
                            case NPCID.DevourerTail:
                            case NPCID.GiantWormHead:
                            case NPCID.GiantWormBody:
                            case NPCID.GiantWormTail:
                            case NPCID.BoneSerpentHead:
                            case NPCID.BoneSerpentBody:
                            case NPCID.BoneSerpentTail:
                            case NPCID.WyvernHead:
                            case NPCID.WyvernLegs:
                            case NPCID.WyvernBody:
                            case NPCID.WyvernBody2:
                            case NPCID.WyvernBody3:
                            case NPCID.WyvernTail:
                            case NPCID.LeechHead:
                            case NPCID.LeechBody:
                            case NPCID.LeechTail:
                            case NPCID.TombCrawlerHead:
                            case NPCID.TombCrawlerBody:
                            case NPCID.TombCrawlerTail:
                            case NPCID.StardustWormHead:
                            case NPCID.SolarCrawltipedeHead:
                            case NPCID.SolarCrawltipedeBody:
                            case NPCID.SolarCrawltipedeTail:
                            case NPCID.BloodEelHead:
                            case NPCID.BloodEelBody:
                            case NPCID.BloodEelTail:
                                return RevengeanceAndDeathAI.BuffedWormAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.ManEater:
                        switch (npc.type)
                        {
                            case NPCID.ManEater:
                            case NPCID.Snatcher:
                            case NPCID.Clinger:
                            case NPCID.AngryTrapper:
                                return RevengeanceAndDeathAI.BuffedPlantAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.Bat:
                        if (npc.type == NPCType<StellarCulex>() || npc.type == NPCType<Melter>() || npc.type == NPCType<AeroSlime>())
                        {
                            return RevengeanceAndDeathAI.BuffedBatAI(npc, Mod);
                        }
                        else
                        {
                            switch (npc.type)
                            {
                                case NPCID.CaveBat:
                                case NPCID.JungleBat:
                                case NPCID.Hellbat:
                                case NPCID.GiantBat:
                                case NPCID.Slimer:
                                case NPCID.IlluminantBat:
                                case NPCID.IceBat:
                                case NPCID.Lavabat:
                                case NPCID.GiantFlyingFox:
                                case NPCID.FlyingSnake:
                                case NPCID.VampireBat:
                                case NPCID.SporeBat:
                                    return RevengeanceAndDeathAI.BuffedBatAI(npc, Mod);
                            }
                        }
                        break;

                    case NPCAIStyleID.Piranha:
                        switch (npc.type)
                        {
                            case NPCID.CorruptGoldfish:
                            case NPCID.Piranha:
                            case NPCID.Shark:
                            case NPCID.AnglerFish:
                            case NPCID.Arapaima:
                            case NPCID.BloodFeeder:
                            case NPCID.CrimsonGoldfish:
                                return RevengeanceAndDeathAI.BuffedSwimmingAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.Jellyfish:
                        switch (npc.type)
                        {
                            case NPCID.BlueJellyfish:
                            case NPCID.PinkJellyfish:
                            case NPCID.GreenJellyfish:
                            case NPCID.Squid:
                            case NPCID.BloodJelly:
                            case NPCID.FungoFish:
                                return RevengeanceAndDeathAI.BuffedJellyfishAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.SpikeBall:
                        switch (npc.type)
                        {
                            case NPCID.SpikeBall:
                                return RevengeanceAndDeathAI.BuffedSpikeBallAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.BlazingWheel:
                        switch (npc.type)
                        {
                            case NPCID.BlazingWheel:
                                return RevengeanceAndDeathAI.BuffedBlazingWheelAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.HoveringFighter:
                        switch (npc.type)
                        {
                            case NPCID.Pixie:
                            case NPCID.Wraith:
                            case NPCID.Gastropod:
                            case NPCID.FloatyGross:
                            case NPCID.Ghost:
                            case NPCID.Poltergeist:
                            case NPCID.Drippler:
                            case NPCID.Reaper:
                                return RevengeanceAndDeathAI.BuffedHoveringAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.EnchantedSword:
                        switch (npc.type)
                        {
                            case NPCID.CursedHammer:
                            case NPCID.EnchantedSword:
                            case NPCID.CrimsonAxe:
                                return RevengeanceAndDeathAI.BuffedFlyingWeaponAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.Mimic:
                        switch (npc.type)
                        {
                            case NPCID.Mimic:
                            case NPCID.PresentMimic:
                            case NPCID.IceMimic:
                                return RevengeanceAndDeathAI.BuffedMimicAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.Unicorn:
                        if (npc.type == NPCType<Rotdog>())
                        {
                            return RevengeanceAndDeathAI.BuffedUnicornAI(npc, Mod);
                        }
                        else
                        {
                            switch (npc.type)
                            {
                                case NPCID.Unicorn:
                                case NPCID.Wolf:
                                case NPCID.HeadlessHorseman:
                                case NPCID.Hellhound:
                                case NPCID.StardustSpiderSmall:
                                case NPCID.NebulaBeast:
                                case NPCID.Tumbleweed:
                                    return RevengeanceAndDeathAI.BuffedUnicornAI(npc, Mod);
                            }
                        }
                        break;

                    case NPCAIStyleID.TheHungry:
                        switch (npc.type)
                        {
                            case NPCID.TheHungry:
                                return WallOfFleshAI.BuffedHungryAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.GiantTortoise:
                        if (npc.type == NPCType<Plagueshell>())
                        {
                            return RevengeanceAndDeathAI.BuffedTortoiseAI(npc, Mod);
                        }
                        else
                        {
                            switch (npc.type)
                            {
                                case NPCID.GiantTortoise:
                                case NPCID.IceTortoise:
                                case NPCID.GiantShelly:
                                case NPCID.GiantShelly2:
                                case NPCID.SolarSroller:
                                    return RevengeanceAndDeathAI.BuffedTortoiseAI(npc, Mod);
                            }
                        }
                        break;

                    case NPCAIStyleID.Spider:
                        switch (npc.type)
                        {
                            case NPCID.DesertScorpionWall:
                                return RevengeanceAndDeathAI.BuffedSpiderAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.Herpling:
                        if (npc.type == NPCType<Aries>())
                        {
                            return RevengeanceAndDeathAI.BuffedHerplingAI(npc, Mod);
                        }
                        else
                        {
                            switch (npc.type)
                            {
                                case NPCID.Herpling:
                                case NPCID.Derpling:
                                case NPCID.ChatteringTeethBomb:
                                    return RevengeanceAndDeathAI.BuffedHerplingAI(npc, Mod);
                            }
                        }
                        break;

                    case NPCAIStyleID.FlyingFish:
                        switch (npc.type)
                        {
                            case NPCID.FlyingFish:
                            case NPCID.GiantFlyingAntlion:
                            case NPCID.FlyingAntlion:
                            case NPCID.EyeballFlyingFish:
                                return RevengeanceAndDeathAI.BuffedFlyingFishAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.AngryNimbus:
                        switch (npc.type)
                        {
                            case NPCID.AngryNimbus:
                                return RevengeanceAndDeathAI.BuffedAngryNimbusAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.TeslaTurret:
                        switch (npc.type)
                        {
                            case NPCID.MartianTurret:
                                return RevengeanceAndDeathAI.BuffedTeslaTurretAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.Corite:
                        switch (npc.type)
                        {
                            case NPCID.MartianDrone:
                            case NPCID.SolarCorite:
                                return RevengeanceAndDeathAI.BuffedCoriteAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.MartianProbe:
                        switch (npc.type)
                        {
                            case NPCID.MartianProbe:
                                return RevengeanceAndDeathAI.BuffedMartianProbeAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.StarCell:
                        switch (npc.type)
                        {
                            case NPCID.StardustCellBig:
                            case NPCID.NebulaHeadcrab:
                            case NPCID.DeadlySphere:
                                return RevengeanceAndDeathAI.BuffedStarCellAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.AncientVision:
                        switch (npc.type)
                        {
                            case NPCID.ShadowFlameApparition:
                            case NPCID.AncientCultistSquidhead:
                                return RevengeanceAndDeathAI.BuffedAncientVisionAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.BiomeMimic:
                        switch (npc.type)
                        {
                            case NPCID.BigMimicCorruption:
                            case NPCID.BigMimicCrimson:
                            case NPCID.BigMimicHallow:
                            case NPCID.BigMimicJungle:
                                return RevengeanceAndDeathAI.BuffedBigMimicAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.MothronEgg:
                        switch (npc.type)
                        {
                            case NPCID.MothronEgg:
                                return RevengeanceAndDeathAI.BuffedMothronEggAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.GraniteElemental:
                        switch (npc.type)
                        {
                            case NPCID.GraniteFlyer:
                                return RevengeanceAndDeathAI.BuffedGraniteElementalAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.SmallStarCell:
                        switch (npc.type)
                        {
                            case NPCID.StardustCellSmall:
                                return RevengeanceAndDeathAI.BuffedSmallStarCellAI(npc, Mod);
                        }
                        break;

                    case NPCAIStyleID.FlowInvader:
                        switch (npc.type)
                        {
                            case NPCID.StardustJellyfishBig:
                                return RevengeanceAndDeathAI.BuffedFlowInvaderAI(npc, Mod);
                        }
                        break;

                    default:
                        break;
                }
            }

            if (npc.type == NPCID.FungiBulb)
                return RevengeanceAndDeathAI.BuffedPlantAI(npc, Mod);

            if (npc.type == NPCID.FungiSpore || npc.type == NPCID.Spore)
                return RevengeanceAndDeathAI.BuffedSporeAI(npc, Mod);

            // Fairies don't run away and are immune to damage while wearing Fairy Boots.
            if (npc.type >= NPCID.FairyCritterPink && npc.type <= NPCID.FairyCritterBlue && (npc.ai[2] < 2f || npc.ai[2] == 7f))
            {
                npc.TargetClosest();
                if (Main.player[npc.target].Calamity().fairyBoots)
                {
                    NPCAimedTarget targetData = npc.GetTargetData();
                    if (targetData.Type == NPCTargetType.Player)
                    {
                        if (Main.player[npc.target].dead)
                            return true;
                    }

                    // Set this to 7 so that they run away when the player takes off their Fairy Boots.
                    npc.ai[2] = 7f;

                    npc.lavaImmune = true;
                    npc.dontTakeDamage = true;
                    npc.noTileCollide = true;
                    npc.rarity = 0;

                    // Teleport to the player if far enough away.
                    if (Vector2.Distance(npc.Center, targetData.Center) > 1000f)
                    {
                        npc.Center = targetData.Center;
                    }

                    // Move towards the player if far enough away.
                    else if (Vector2.Distance(npc.Center, targetData.Center) > 80f)
                    {
                        Rectangle r = Utils.CenteredRectangle(targetData.Center, new Vector2(targetData.Width + 60, targetData.Height / 2));
                        Vector2 closestTargetPoint = r.ClosestPointInRect(npc.Center);
                        Vector2 targetPointDir = npc.DirectionTo(closestTargetPoint) * ((targetData.Velocity.Length() * 0.5f) + 2f);
                        float targetPointDist = npc.Distance(closestTargetPoint);
                        if (targetPointDist > 225f)
                            targetPointDir *= 2f;
                        else if (targetPointDist > 120f)
                            targetPointDir *= 1.5f;

                        npc.velocity = Vector2.Lerp(npc.velocity, targetPointDir, 0.07f);
                    }

                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        if (k != npc.whoAmI && Main.npc[k].active && Main.npc[k].aiStyle == NPCAIStyleID.Fairy && Math.Abs(npc.position.X - Main.npc[k].position.X) + Math.Abs(npc.position.Y - Main.npc[k].position.Y) < (float)npc.width * 1.5f)
                        {
                            if (npc.position.Y < Main.npc[k].position.Y)
                                npc.velocity.Y -= 0.05f;
                            else
                                npc.velocity.Y += 0.05f;
                        }
                    }

                    npc.direction = (npc.velocity.X >= 0f) ? 1 : (-1);
                    npc.spriteDirection = -npc.direction;

                    Color dustLerpColor1 = Color.HotPink;
                    Color dustLerpColor2 = Color.LightPink;
                    int dustPosition = 4;
                    if (npc.type == NPCID.FairyCritterGreen)
                    {
                        dustLerpColor1 = Color.LimeGreen;
                        dustLerpColor2 = Color.LightSeaGreen;
                    }

                    if (npc.type == NPCID.FairyCritterBlue)
                    {
                        dustLerpColor1 = Color.RoyalBlue;
                        dustLerpColor2 = Color.LightBlue;
                    }

                    if ((int)Main.timeForVisualEffects % 2 == 0)
                    {
                        npc.position += npc.netOffset;
                        Dust dust = Dust.NewDustDirect(npc.Center - new Vector2(dustPosition) * 0.5f, dustPosition + 4, dustPosition + 4, DustID.FireworksRGB, 0f, 0f, 200, Color.Lerp(dustLerpColor1, dustLerpColor2, Main.rand.NextFloat()), 0.65f);
                        dust.velocity *= 0f;
                        dust.velocity += npc.velocity * 0.3f;
                        dust.noGravity = true;
                        dust.noLight = true;
                        npc.position -= npc.netOffset;
                    }

                    Lighting.AddLight(npc.Center, dustLerpColor1.ToVector3() * 0.7f);
                    if (!Main.dedServ)
                    {
                        Player localPlayer = Main.LocalPlayer;
                        if (!localPlayer.dead && localPlayer.HitboxForBestiaryNearbyCheck.Intersects(npc.Hitbox))
                            AchievementsHelper.HandleSpecialEvent(localPlayer, 22);
                    }

                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Boss Rush Force Despawn Other NPCs
        private void BossRushForceDespawnOtherNPCs(NPC npc, Mod mod)
        {
            if (BossRushEvent.BossRushStage >= BossRushEvent.Bosses.Count)
                return;

            if (!BossRushEvent.Bosses[BossRushEvent.BossRushStage].HostileNPCsToNotDelete.Contains(npc.type))
            {
                npc.active = false;
                npc.netUpdate = true;
            }
        }
        #endregion

        #region Pillar Event Progression Edit
        private void PillarEventProgressionEdit(NPC npc)
        {
            // Make pillars a bit more fun by forcing more difficult enemies based on progression.
            int solarTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerSolar / 25D);
            switch (solarTowerShieldStrength)
            {
                case 4:
                    // Possible spawns: Drakanian, Drakomire, Drakomire Rider, Sroller
                    switch (npc.type)
                    {
                        case NPCID.SolarCrawltipedeHead:
                        case NPCID.SolarCrawltipedeBody:
                        case NPCID.SolarCrawltipedeTail:
                        case NPCID.SolarSolenian:
                        case NPCID.SolarCorite:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    // Possible spawns: Drakanian, Drakomire Rider, Sroller
                    switch (npc.type)
                    {
                        case NPCID.SolarCrawltipedeHead:
                        case NPCID.SolarCrawltipedeBody:
                        case NPCID.SolarCrawltipedeTail:
                        case NPCID.SolarDrakomire:
                        case NPCID.SolarSolenian:
                        case NPCID.SolarCorite:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    // Possible spawns: Drakanian, Selenian, Sroller
                    switch (npc.type)
                    {
                        case NPCID.SolarDrakomire:
                        case NPCID.SolarCrawltipedeHead:
                        case NPCID.SolarCrawltipedeBody:
                        case NPCID.SolarCrawltipedeTail:
                        case NPCID.SolarCorite:
                        case NPCID.SolarDrakomireRider:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 1:
                    // Possible spawns: Corite, Selenian, Sroller, Crawltipede
                    switch (npc.type)
                    {
                        case NPCID.SolarDrakomire:
                        case NPCID.SolarSpearman:
                        case NPCID.SolarDrakomireRider:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 0:
                    // Possible spawns: Corite, Crawltipede, Selenian
                    switch (npc.type)
                    {
                        case NPCID.SolarDrakomire:
                        case NPCID.SolarSpearman:
                        case NPCID.SolarDrakomireRider:
                        case NPCID.SolarSroller:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
            }

            int vortexTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerVortex / 25D);
            switch (vortexTowerShieldStrength)
            {
                case 4:
                    // Possible spawns: Alien Larva, Alien Hornet, Alien Queen
                    switch (npc.type)
                    {
                        case NPCID.VortexSoldier:
                        case NPCID.VortexRifleman:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    // Possible spawns: Alien Larva, Alien Hornet, Alien Queen, Vortexian
                    if (npc.type == NPCID.VortexRifleman)
                    {
                        npc.active = false;
                        npc.netUpdate = true;
                    }
                    break;
                case 2:
                    // Possible spawns: Alien Larva, Alien Hornet, Alien Queen, Storm Diver
                    if (npc.type == NPCID.VortexSoldier)
                    {
                        npc.active = false;
                        npc.netUpdate = true;
                    }
                    break;
                case 1:
                case 0:
                    // Possible spawns: Alien Larva, Alien Hornet, Alien Queen, Vortexian, Storm Diver
                    break;
            }

            int nebulaTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerNebula / 25D);
            switch (nebulaTowerShieldStrength)
            {
                case 4:
                    // Possible spawns: Brain Suckler
                    switch (npc.type)
                    {
                        case NPCID.NebulaBeast:
                        case NPCID.NebulaBrain:
                        case NPCID.NebulaSoldier:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    // Possible spawns: Brain Suckler, Predictor
                    switch (npc.type)
                    {
                        case NPCID.NebulaBeast:
                        case NPCID.NebulaBrain:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    // Possible spawns: Brain Suckler, Predictor, Evolution Beast
                    if (npc.type == NPCID.NebulaBrain)
                    {
                        npc.active = false;
                        npc.netUpdate = true;
                    }
                    break;
                case 1:
                case 0:
                    // Possible spawns: Predictor, Evolution Beast, Nebula Floater
                    if (npc.type == NPCID.NebulaHeadcrab)
                    {
                        npc.active = false;
                        npc.netUpdate = true;
                    }
                    break;
            }

            int stardustTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerStardust / 25D);
            switch (stardustTowerShieldStrength)
            {
                case 4:
                    // Possible spawns: Milkyway Weaver, Star Cell
                    switch (npc.type)
                    {
                        case NPCID.StardustSpiderBig:
                        case NPCID.StardustSoldier:
                        case NPCID.StardustJellyfishBig:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    // Possible spawns: Milkyway Weaver, Stargazer, Twinkle Popper
                    switch (npc.type)
                    {
                        case NPCID.StardustCellBig:
                        case NPCID.StardustJellyfishBig:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    // Possible spawns: Stargazer, Twinkle Popper, Flow Invader
                    switch (npc.type)
                    {
                        case NPCID.StardustCellBig:
                        case NPCID.StardustWormHead:
                        case NPCID.StardustWormBody:
                        case NPCID.StardustWormTail:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 1:
                case 0:
                    // Possible spawns: Twinkle Popper, Flow Invader
                    switch (npc.type)
                    {
                        case NPCID.StardustCellBig:
                        case NPCID.StardustWormHead:
                        case NPCID.StardustWormBody:
                        case NPCID.StardustWormTail:
                        case NPCID.StardustSoldier:
                            npc.active = false;
                            npc.netUpdate = true;
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        #endregion

        #region AI
        public override void AI(NPC npc)
        {
            // Fair contact damage
            switch (npc.type)
            {
                case NPCID.DD2Betsy:
                    npc.damage = npc.ai[0] == 2f ? npc.defDamage : 0;
                    break;

                case NPCID.DD2WyvernT1:
                case NPCID.DD2WyvernT2:
                case NPCID.DD2WyvernT3:
                    npc.damage = npc.ai[0] == 2f ? npc.defDamage : 0;
                    break;

                case NPCID.Mothron:
                    npc.damage = npc.ai[0] == 3.2f ? (int)Math.Round(npc.defDamage * 1.3) : npc.ai[0] == 2f ? (int)Math.Round(npc.defDamage * 0.5) : 0;
                    break;

                case NPCID.MothronSpawn:
                    npc.damage = npc.ai[0] == 2.1f ? npc.defDamage : 0;
                    break;

                case NPCID.Mimic:
                case NPCID.IceMimic:
                case NPCID.PresentMimic:
                    npc.damage = (npc.ai[0] == 0f || npc.velocity.Y == 0f) ? 0 : npc.defDamage;
                    break;

                case NPCID.BigMimicCorruption:
                case NPCID.BigMimicCrimson:
                case NPCID.BigMimicHallow:
                case NPCID.BigMimicJungle:
                    npc.damage = npc.ai[0] == 3f ? 0 : npc.defDamage;

                    // Spend less time in closed state
                    if (npc.ai[0] == 3f)
                        npc.ai[1] += 0.5f;

                    break;

                case NPCID.MartianDrone:
                case NPCID.SolarCorite:
                    npc.damage = (npc.ai[0] == 2f || npc.ai[0] == 3f) ? npc.defDamage : 0;
                    break;

                case NPCID.GraniteFlyer:
                    npc.damage = npc.ai[0] == -1f ? 0 : npc.defDamage;
                    break;

                case NPCID.GraniteGolem:
                    npc.damage = npc.ai[2] < 0f ? 0 : npc.defDamage;
                    break;

                case NPCID.BlueSlime:
                case NPCID.MotherSlime:
                case NPCID.LavaSlime:
                case NPCID.DungeonSlime:
                case NPCID.CorruptSlime:
                case NPCID.IlluminantSlime:
                case NPCID.ToxicSludge:
                case NPCID.IceSlime:
                case NPCID.Crimslime:
                case NPCID.UmbrellaSlime:
                case NPCID.RainbowSlime:
                case NPCID.SlimeMasked:
                case NPCID.HoppinJack:
                case NPCID.SlimeRibbonWhite:
                case NPCID.SlimeRibbonYellow:
                case NPCID.SlimeRibbonGreen:
                case NPCID.SlimeRibbonRed:
                case NPCID.SandSlime:
                case NPCID.GoldenSlime:
                case NPCID.ShimmerSlime:
                case NPCID.GreenSlime:
                case NPCID.RedSlime:
                case NPCID.PurpleSlime:
                case NPCID.YellowSlime:
                case NPCID.BlackSlime:
                case NPCID.JungleSlime:
                case NPCID.BabySlime:
                case NPCID.Pinky:
                case NPCID.Slimeling:
                case NPCID.Slimer2:
                    npc.damage = (npc.velocity.Y == 0f || npc.velocity.Length() < 3f) ? 0 : npc.defDamage;
                    break;

                case NPCID.GiantShelly:
                case NPCID.GiantShelly2:
                    npc.damage = npc.ai[0] == 3f ? (int)Math.Round(npc.defDamage * 1.2) : 0;
                    break;

                case NPCID.GiantTortoise:
                case NPCID.IceTortoise:
                    npc.damage = npc.ai[0] == 3f ? (int)Math.Round(npc.defDamage * 1.4) : 0;
                    break;

                case NPCID.SolarSroller:
                    npc.damage = npc.ai[0] == 6f ? (int)Math.Round(npc.defDamage * 1.2) : 0;
                    break;

                default:
                    break;
            }

            if (CalamityWorld.revenge && npc.type == NPCID.DungeonGuardian)
                SkeletronAI.RevengeanceDungeonGuardianAI(npc);
        }
        #endregion

        #region Post AI
        public override void PostAI(NPC npc)
        {
            // Worm heads emit dust when close enough to the player and digging through tiles
            if (npc.type == NPCID.GiantWormHead || npc.type == NPCID.DiggerHead || npc.type == NPCID.DevourerHead ||
                npc.type == NPCID.SeekerHead || npc.type == NPCID.TombCrawlerHead || npc.type == NPCID.BoneSerpentHead ||
                npc.type == NPCID.DuneSplicerHead || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.TheDestroyer)
            {
                Point point = npc.Center.ToTileCoordinates();
                Tile tileSafely = Framing.GetTileSafely(point);
                bool createDust = tileSafely.HasUnactuatedTile && npc.Distance(Main.player[npc.target].Center) < 800f;
                if (createDust)
                {
                    if (Main.rand.NextBool())
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.TreasureSparkle, 0f, 0f, 150, default, 0.3f);
                        dust.fadeIn = 0.75f;
                        dust.velocity *= 0.1f;
                        dust.noLight = true;
                    }
                }
            }

            // Plants that go through tiles emit spores while inside tiles
            else if (npc.type == NPCID.ManEater || npc.type == NPCID.Snatcher || npc.type == NPCID.AngryTrapper)
            {
                Point point = npc.Center.ToTileCoordinates();
                Tile tileSafely = Framing.GetTileSafely(point);
                bool createDust = tileSafely.HasUnactuatedTile && npc.Distance(Main.player[npc.target].Center) < 800f;
                if (createDust)
                {
                    if (Main.rand.NextBool(10))
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.JungleSpore, 0f, 0f, 250, default, 0.4f);
                        dust.fadeIn = 0.7f;
                    }
                }
            }

            // Clingers emit cursed fire while inside tiles
            else if (npc.type == NPCID.Clinger)
            {
                Point point = npc.Center.ToTileCoordinates();
                Tile tileSafely = Framing.GetTileSafely(point);
                bool createDust = tileSafely.HasUnactuatedTile && npc.Distance(Main.player[npc.target].Center) < 800f;
                if (createDust)
                {
                    if (Main.rand.NextBool(5))
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.CursedTorch, 0f, 0f, 100, default, 1.5f);
                        dust.noGravity = true;
                    }
                }

                // Emit cursed flame dust from mouth when about to fire
                else if (npc.localAI[0] > (CalamityWorld.revenge ? RevengeanceAndDeathAI.ClingerShootGateValue_Rev : RevengeanceAndDeathAI.ClingerShootGateValue) - RevengeanceAndDeathAI.ClingerTelegraphTime)
                {
                    Vector2 dustCenter = npc.Center + npc.SafeDirectionTo(Main.player[npc.target].Center, -Vector2.UnitY) * 20f + Main.rand.NextVector2CircularEdge(5f, 5f);
                    Dust dust = Dust.NewDustDirect(dustCenter, 1, 1, DustID.CursedTorch, 0f, 0f, 100, default, 3f);
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }

                // Reset shoot counter if inside tiles or cannot see the target
                if (Collision.SolidCollision(npc.position, npc.width, npc.height) || !Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                    npc.localAI[0] = 0f;
            }

            else if (npc.type == NPCID.IchorSticker)
            {
                // Emit ichor dust from mouth when about to fire
                if (npc.ai[3] > (CalamityWorld.death ? RevengeanceAndDeathAI.IchorStickerShootGateValue_Death : CalamityWorld.revenge ? RevengeanceAndDeathAI.IchorStickerShootGateValue_Rev : RevengeanceAndDeathAI.IchorStickerShootGateValue) - RevengeanceAndDeathAI.IchorStickerTelegraphTime)
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(npc.Center.X - 4f, npc.position.Y + npc.height * 0.7f) + Main.rand.NextVector2CircularEdge(2f, 2f), 1, 1, DustID.Ichor, 0f, 0f, 100, default, 1.5f);
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }

                // Reset shoot counter if cannot see the target
                if (!Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].head))
                    npc.ai[3] = 0f;
            }

            

            if (warbannerBurnTimer > 0)
                warbannerBurnTimer--;
            if (warbannerBurnTimer == 0 && warbannerBurnMarked)
            {
                warbannerBurnTime = 0;
                warbannerBurnDamage = 0;
                warbannerBurnMarked = false;
                warbannerBurnStacks = 0;
            }
            if (warbannerBurnTimer <= 60)
            {
                warbannerBurnStacks = (int)(warbannerBurnStacks * 0.9f);
            }
            if (warbannerBurnMarked)
            {
                int maxStacks = 300; // Time in frames needed to reach max power
                int fastestBurnRate = 2;
                int slowestBurnRate = 15;
                float burnPower = Utils.Remap(warbannerBurnStacks, 0, maxStacks, slowestBurnRate, fastestBurnRate, true);

                float sizeBonus = (1 + Utils.GetLerpValue(0, 170, Math.Max(npc.Hitbox.Width / 2f, npc.Hitbox.Height / 2f)));

                if (!warbannerBurnHideEffects)
                {
                    Lighting.AddLight(npc.Center, Color.Gold.ToVector3() * 0.3f * warbannerBurnIntensity);
                }
                if (warbannerBurnStacks == maxStacks && !warbannerBurnHideEffects)
                {
                    // Sound and visual for hitting max stacks
                    for (int i = 0; i < 15; i++)
                    {
                        Particle spark = new SparkParticle(npc.Center, new Vector2(13, 13).RotatedByRandom(100) * Main.rand.NextFloat(0.4f, 1f), true, 45, 0.85f, Main.rand.NextBool() ? Color.Goldenrod : Color.Orange);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    SoundStyle fullPower = new("CalamityMod/Sounds/Custom/Providence/ProvidenceBurn");
                    SoundEngine.PlaySound(fullPower with { Volume = 0.7f, Pitch = 0.7f }, npc.Center);
                    warbannerBurnStacks++;
                }
                if (warbannerBurnIntensity > 2.5f && npc.CanBeMoved(true))
                {
                    npc.velocity *= 1f - 0.25f * Utils.GetLerpValue(2.5f, 3, warbannerBurnIntensity);
                    if (npc.velocity.Length() > 5 && warbannerBurnIntensity > 2.85f) // Repel leaping enemies
                        npc.velocity = -npc.velocity * 0.7f;
                }
                if (warbannerBurnTime == 0)
                {
                    if (!warbannerBurnHideEffects)
                    {
                        int particleLevel = (int)(MathHelper.Clamp((slowestBurnRate - burnPower) * 0.15f, 1, 2) * warbannerBurnIntensity);
                        for (int d = 0; d < particleLevel; d++)
                        {
                            Color color = Main.rand.NextBool() ? Color.Goldenrod : Color.Lerp(Color.OrangeRed, Color.Orange, Main.rand.NextFloat(0, 1)); ;
                            Vector2 sparkPos = npc.Center - warbannerBurnDirection * 220 * Utils.GetLerpValue(0, 200, Math.Max(npc.Hitbox.Width / 2f, npc.Hitbox.Height / 2f));
                            float velAdjust = Main.rand.NextFloat(2, 7) * warbannerBurnIntensity * sizeBonus;
                            Vector2 endVel = warbannerBurnDirection * velAdjust;
                            Vector2 startVel = (warbannerBurnDirection * velAdjust).RotatedByRandom(0.6f * warbannerBurnIntensity);
                            Particle sparks = new VelChangingSpark(sparkPos, startVel, endVel, "CalamityMod/Particles/SmallBloom", Main.rand.Next(18, 22 + 1), Main.rand.NextFloat(0.1f, 0.25f) * sizeBonus, color * 0.75f, new Vector2(0.7f, 1), true, false, 0, false, 0.45f, 0.1f);
                            GeneralParticleHandler.SpawnParticle(sparks);
                            Dust lust2 = Dust.NewDustPerfect(sparkPos, DustType<LightDust>(), startVel, Scale: Main.rand.NextFloat(0.7f, 1.1f) * sizeBonus);
                            lust2.noGravity = true;
                            lust2.color = color;
                            lust2.noLightEmittence = true;
                        }
                    }
                    var player = Main.LocalPlayer;
                    Projectile burnHit = Projectile.NewProjectileDirect(player.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileType<WarbannerDamage>(), (int)(warbannerBurnDamage * warbannerBurnIntensity), 0, Main.myPlayer, npc.whoAmI);
                    burnHit.ArmorPenetration = 50;
                    warbannerBurnTime = (int)(burnPower + (3 - warbannerBurnIntensity) * 4);
                }
                warbannerBurnTime--;
            }

            if (veriumDoomTimer > 0)
                veriumDoomTimer--;
            if (laserBurnTimer > 0)
                laserBurnTimer--;

            if (veriumDoomTimer == 0 && veriumDoomMarked)
            {
                for (int d = 0; d < 14 + veriumDoomStacks; d++)
                {
                    Particle sparks = new LineParticle(npc.Center, new Vector2(Main.rand.NextFloat(-9f, 9f), Main.rand.NextFloat(-9f, 9f)), false, 45, 0.9f, Main.rand.NextBool() ? Color.Cyan : Color.SkyBlue);
                    GeneralParticleHandler.SpawnParticle(sparks);
                }

                SoundEngine.PlaySound(new("CalamityMod/Sounds/NPCHit/CryogenHit", 3) { Volume = 0.6f }, npc.Center);
                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileType<DirectStrike>(), 100 + (15 * veriumDoomStacks), 0, Main.myPlayer, npc.whoAmI);

                veriumDoomMarked = false;
                veriumDoomStacks = 0;
            }

            // Amidias' Spark spark spawning
            if (shocked > 0)
            {
                var player = Main.LocalPlayer;

                int frequency = 15;

                // Spawn sparks from the enemy
                if (player.miscCounter % frequency == 0)
                {
                    int sDamage = 10;
                    Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 5f;
                    Projectile spark = Projectile.NewProjectileDirect(npc.GetSource_FromThis(), npc.Center, velocity, ProjectileType<GenericElectricSpark>(), sDamage, 0f, player.whoAmI, 0f, 1f);
                    spark.timeLeft = 120;
                    spark.penetrate = 3;
                }
            }
            if (hyperiusMarked)
            {
                if (hyperiusFxTimer < 20)
                    hyperiusFxTimer++;
                else if (hyperiusFxTimer > 20)
                    hyperiusFxTimer = (int)Utils.Lerp(hyperiusFxTimer, 20, 0.2f);

                float threshold = ((float)(hyperiusDamage) / (float)(npc.lifeMax));
                int overflowSpeed = (int)Utils.Remap(threshold, hyperiusLifePercentThreshold, 0.35f, 1, 34);
                if (threshold > hyperiusLifePercentThreshold) // If the stored damage is greater than the life % cap of the target's max health, rapily deal a % of the stored damage to the enemy to drain it
                {
                    bool enemyIsNotArmored = (npc.defense < 1000 && !unbreakableDR && DR <= 0.9f && !npc.dontTakeDamage && !npc.immortal);
                    if (enemyIsNotArmored)
                        hyperiusOverflowTimer -= overflowSpeed;
                    if (hyperiusOverflowTimer <= 0)
                    {
                        hyperiusOverflowTimer = hyperiusOverflowTime;

                        float damagePercent = 0.07f; // The % of stacks drained when you're over the cap
                        int damage = (int)Math.Max((int)((float)(hyperiusDamage) * damagePercent), 1);
                        hyperiusDamage -= damage;

                        // Spawn "bleed" hit
                        // Uses a seperate projectile so that the hit takes defense and DR into account
                        Projectile overflow = Projectile.NewProjectileDirect(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileType<HyperiusBleed>(), damage, 0, -1, npc.whoAmI);
                        overflow.DamageType = DamageClass.Ranged;
                        if (hyperiusFxTimer >= 20)
                            hyperiusFxTimer = 35;

                        if (hyperiusDamage <= 0)
                        {
                            hyperiusDamage = 0;
                            hyperiusMarked = false;
                        }
                    }
                }
            }
            else if (hyperiusFxTimer > 0)
                hyperiusFxTimer--;

            if ((laserBurnTimer <= 0 || laserBurnDamage >= npc.life * 1.5f) && laserBurnMarked && laserBurnType > 0)
            {
                if (laserBurnType == 1) // Applied damage
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileType<DirectStrike>(), laserBurnDamage, 0, Main.myPlayer, npc.whoAmI);
                if (laserBurnType == 2) // Flat damage + stacks
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileType<DirectStrike>(), 70 + (20 * laserBurnStacks), 0, Main.myPlayer, npc.whoAmI);

                for (int d = 0; d < (int)(7 + laserBurnStacks * 0.4f); d++)
                {
                    float partScale = Main.rand.NextFloat(0.7f, 1f);
                    Vector2 partVel = (new Vector2(10, 10) * (laserBurnStacks * 0.025f)).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f);

                    Particle spark2 = new CustomSpark(npc.Center, partVel, "CalamityMod/Particles/BloomLineSoftEdge", false, 12, Main.rand.NextFloat(0.02f, 0.03f), Effects.ArsenalEffects.ArsenalLaserColor * 0.8f, new Vector2(1, 1), true, false, 0, false, false, 1f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                    Dust dust = Dust.NewDustPerfect(npc.Center, Effects.ArsenalEffects.ArsenalLaserDust);
                    dust.velocity = (Vector2.UnitX * 5 * (laserBurnStacks * 0.05f)).RotatedByRandom(100) * Main.rand.NextFloat(0.85f, 1f);
                    dust.scale = Main.rand.NextFloat(0.65f, 0.8f);
                    dust.noGravity = true;
                    dust.color = Color.Red;
                }

                SoundEngine.PlaySound(new("CalamityMod/Sounds/Item/LaserBurn") { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, npc.Center);
                laserBurnMarked = false;
                laserBurnStacks = 0;
                laserBurnTimer = 0;
                laserBurnDamage = 0;
            }

            // Queen Bee is completely immune to having her movement impaired if not in a high difficulty mode.
            if (npc.type == NPCID.QueenBee && !CalamityWorld.revenge && !BossRushEvent.BossRushActive)
                return;

            // Pearl Aura shard spawning
            // Slowing is handled in the general slowing code below
            if (pearlAura)
            {
                pearlAuraCounter++;
                if (pearlAuraCounter >= 45)
                {
                    pearlAuraCounter = 0;
                    SoundEngine.PlaySound(SoundID.Item49, npc.Center);

                    // Prevent things from getting too crazy
                    // CIT 8MAR2025: It is assumed that pearlAuraOwner is always set to something other than -1 when this code is run.
                    if (CalamityUtils.CountOwnedProjectiles(ProjectileType<PearlAuraShard>(), pearlAuraOwner) <= 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 shardPos = npc.Center + new Vector2(Main.rand.NextFloat(-100f, 100f), Main.rand.NextFloat(-500f, -650f));
                            Vector2 shardVel = Vector2.Normalize(npc.Center - shardPos).RotatedByRandom(MathHelper.Pi / 55f) * 20f;
                            int damage = 20;
                            Projectile.NewProjectile(npc.GetSource_FromThis(), shardPos, shardVel, ProjectileType<PearlAuraShard>(), damage, 5f, pearlAuraOwner);
                        }
                    }
                }
            }
            else
            {
                pearlAuraCounter = 0;
                pearlAuraOwner = -1;
            }

            if (demonSwordImpales > 0 && npc.CanBeMoved(true))
            {
                npc.velocity *= Utils.Remap(demonSwordImpales, 1, 5, 0.95f, 0.3f, true);
                if (impalePacketTimer > 30) // There's probably a better solution, but this is the best for now
                {
                    npc.SyncMotionToServer();
                    impalePacketTimer = 0;
                }
            }
            impalePacketTimer++;

            // Queen Bee is completely immune to having her movement impaired if not in a high difficulty mode.
            if (npc.type == NPCID.QueenBee && !CalamityWorld.revenge && !BossRushEvent.BossRushActive)
                return;

            // Apply slowing debuff effects
            if (debuffResistanceTimer <= 0 || (debuffResistanceTimer > slowingDebuffResistanceMin))
            {
                // Slowing debuffs which set a velocity hard cap take priority first.
                if (vulnerabilityHex)
                    npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-Calamity.MaxNPCSpeed), new Vector2(Calamity.MaxNPCSpeed, 10f));

                // Then debuffs which apply a multiplier to velocity.
                // These multipliers can stack with each other, even if you'll rarely see this on a boss.
                float velocitySlownessFactor = 1f;

                if (temporalSadness)
                    velocitySlownessFactor += 0.2f;

                if (timeDistortion)
                    velocitySlownessFactor += 0.15f;

                if (webbed)
                    velocitySlownessFactor += 0.15f;

                if (glacialState)
                {
                    float baseSlownessFactor = 0.1f;
                    if (VulnerableToCold.HasValue)
                    {
                        if (VulnerableToCold.Value)
                            baseSlownessFactor = 0.4f;
                        else
                            baseSlownessFactor = 0.025f;
                    }
                    velocitySlownessFactor += baseSlownessFactor;
                }

                if (pearlAura)
                    velocitySlownessFactor += 0.1f;

                if (eutrophication)
                {
                    float baseSlownessFactor = 0.05f;
                    if (VulnerableToWater.HasValue)
                    {
                        if (VulnerableToWater.Value)
                            baseSlownessFactor = 0.2f;
                        else
                            baseSlownessFactor = 0.0125f;
                    }
                    velocitySlownessFactor += baseSlownessFactor;
                }

                if (galvanicCorrosion)
                {
                    float baseSlownessFactor = 0.05f;
                    if (VulnerableToElectricity.HasValue)
                    {
                        if (VulnerableToElectricity.Value)
                            baseSlownessFactor = 0.2f;
                        else
                            baseSlownessFactor = 0.0125f;
                    }
                    velocitySlownessFactor += baseSlownessFactor;
                }

                if (vaporfied)
                    velocitySlownessFactor += 0.05f;

                // Divide 1 by the slowness factor to get the amount to slow by.
                // This scales with diminishing returns, though getting slowed every frame means they quickly slow down either way.
                velocitySlownessFactor = 1f / velocitySlownessFactor;
                npc.velocity *= velocitySlownessFactor;
            }

            // Auric Ore/Repulsers reject Town NPCs and dummies (Auric Land Mines work on them too)
            if ((NPCID.Sets.ActsLikeTownNPC[npc.type] || npc.townNPC) && !npc.dontTakeDamage || npc.type == NPCType<SuperDummyNPC>())
            {
                int auricOreID = TileType<AuricOre>();
                int auricRepulserID = TileType<AuricRepulserPanelTile>();
                int auricLandMineID = TileType<AuricLandMineTile>();

                // Get a list of tiles near the npc
                // This is just Collision.GetEntityTiles but with a larger detection square because the sheer speed from auric boosts causes the detection to fail at higher speeds
                List<Point> EdgeTiles = new List<Point>();
                int extraDist = (int)(8 * npc.velocity.Length() / 6) + 1;
                int left = (int)npc.position.X - extraDist;
                int up = (int)npc.position.Y - extraDist;
                int right = (int)npc.Right.X + extraDist;
                int down = (int)npc.Bottom.Y + extraDist;
                if (left % 16 == 0)
                {
                    left--;
                }

                if (up % 16 == 0)
                {
                    up--;
                }

                if (right % 16 == 0)
                {
                    right++;
                }

                if (down % 16 == 0)
                {
                    down++;
                }

                int width = right / 16 - left / 16;
                int height = down / 16 - up / 16;
                left /= 16;
                up /= 16;
                for (int i = left; i <= left + width; i++)
                {
                    EdgeTiles.Add(new Point(i, up));
                    EdgeTiles.Add(new Point(i, up + height));
                }

                for (int j = up; j < up + height; j++)
                {
                    EdgeTiles.Add(new Point(left, j));
                    EdgeTiles.Add(new Point(left + width, j));
                }
                foreach (Point touchedTile in EdgeTiles)
                {
                    Tile tile = Framing.GetTileSafely(touchedTile);
                    if (!tile.HasTile || !tile.HasUnactuatedTile)
                        continue;

                    if (tile.TileType == auricLandMineID)
                    {
                        SoundStyle explode = new("CalamityMod/Sounds/Item/DudFire");
                        SoundEngine.PlaySound(explode with { Pitch = 0.8f }, touchedTile.ToWorldCoordinates());
                        GenericSparkle sparker = new GenericSparkle(touchedTile.ToWorldCoordinates(), Vector2.Zero, Color.Goldenrod, Color.Gold, 2.5f, 9, Main.rand.NextFloat(-0.01f, 0.01f), 2.68f);
                        GeneralParticleHandler.SpawnParticle(sparker);
                        Projectile.NewProjectile(new EntitySource_TileInteraction(npc, touchedTile.X, touchedTile.Y), touchedTile.ToWorldCoordinates(), Vector2.Zero, ProjectileType<AuricLandMineExplosion>(), 40000, 0f);
                        WorldGen.KillTile(touchedTile.X, touchedTile.Y, noItem: true);
                        continue;
                    }

                    if (tile.TileType != auricOreID && tile.TileType != auricRepulserID)
                        continue;

                    // Force Auric Ore to animate with its crackling electricity
                    if (tile.TileType == auricOreID)
                    {
                        AuricOre.Animate = true;
                    }

                    var yeetVec = Vector2.Normalize(npc.Center - touchedTile.ToWorldCoordinates());
                    npc.velocity += yeetVec * 20f;
                    // Speed must be clamped or they start clipping through tiles very easily
                    float clampedSpeed = MathHelper.Clamp(npc.velocity.Length(), -40, 40);
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * clampedSpeed;
                    if (tile.TileType == auricOreID)
                    {
                        npc.SimpleStrikeNPC((int)(npc.lifeMax * 0.2f), 0);
                        npc.AddBuff(BuffType<AuricRebuke>(), 120);
                    }
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/ExoMechs/TeslaShoot1"), npc.Center);
                    break;
                }
            }
        }
        #endregion

        #region On Hit Player
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage <= 0)
                return;

            if (target.Calamity().sulphurSet)
                npc.AddBuff(BuffID.Poisoned, 60);

            if (target.Calamity().ilSpark)
            {
                shocked = 120;
            }

            if (target.Transformation().Type == ModContent.ItemType<Popo>())
            {
                if (npc.type == NPCID.Demon || npc.type == NPCID.VoodooDemon || npc.type == NPCID.RedDevil)
                    target.AddBuff(BuffType<PopoNoselessBuff>(), 36000);
            }

            switch (npc.type)
            {
                case NPCID.DevourerHead:
                case NPCID.FaceMonster:
                    target.AddBuff(BuffID.Weak, 180);
                    break;

                case NPCID.Crawdad:
                case NPCID.Crawdad2:
                case NPCID.UndeadViking:
                    target.AddBuff(BuffID.BrokenArmor, 180);
                    break;

                case NPCID.ArmoredViking:
                    target.AddBuff(BuffID.BrokenArmor, 300);
                    break;

                case NPCID.IlluminantBat:
                    target.AddBuff(BuffID.Confused, 120);
                    break;

                case NPCID.Piranha:
                    target.AddBuff(BuffID.Bleeding, 180);
                    break;

                case NPCID.Arapaima:
                case NPCID.BloodFeeder:
                    target.AddBuff(BuffID.Bleeding, 300);
                    break;

                case NPCID.ToxicSludge:
                    target.AddBuff(BuffID.Slow, 300);
                    break;

                case NPCID.ShadowFlameApparition:
                    target.AddBuff(BuffType<Shadowflame>(), 120);
                    break;

                case NPCID.ChaosBall:
                    if (Main.hardMode || CalamityPlayer.areThereAnyDamnBosses)
                        target.AddBuff(BuffType<Shadowflame>(), 120);
                    break;

                case NPCID.Golem:
                    if (CalamityWorld.revenge)
                        target.AddBuff(BuffType<ArmorCrunch>(), 480);
                    break;

                case NPCID.GolemHead:
                case NPCID.GolemHeadFree:
                case NPCID.GolemFistRight:
                case NPCID.GolemFistLeft:
                    if (CalamityWorld.revenge)
                        target.AddBuff(BuffType<ArmorCrunch>(), 240);
                    break;

                case NPCID.BloodNautilus:
                    target.AddBuff(BuffType<BurningBlood>(), 300);
                    break;

                case NPCID.GoblinShark:
                case NPCID.BloodEelHead:
                    target.AddBuff(BuffType<BurningBlood>(), 180);
                    break;

                case NPCID.Lavabat:
                    target.AddBuff(BuffID.OnFire, 300);
                    break;

                case NPCID.RuneWizard:
                    if (Main.zenithWorld)
                        target.AddBuff(BuffType<MiracleBlight>(), 600);
                    break;

                default:
                    break;
            }

            if (Main.hardMode)
            {
                if (CalamityNPCTypeSets.AngryBones[npc.type])
                    target.AddBuff(BuffType<ArmorCrunch>(), 120);

                if (NPC.downedPlantBoss)
                {
                    switch (npc.type)
                    {
                        case NPCID.RustyArmoredBonesAxe:
                        case NPCID.RustyArmoredBonesFlail:
                        case NPCID.RustyArmoredBonesSword:
                        case NPCID.RustyArmoredBonesSwordNoArmor:
                        case NPCID.BlueArmoredBones:
                        case NPCID.BlueArmoredBonesMace:
                        case NPCID.BlueArmoredBonesNoPants:
                        case NPCID.BlueArmoredBonesSword:
                        case NPCID.HellArmoredBones:
                        case NPCID.HellArmoredBonesSpikeShield:
                        case NPCID.HellArmoredBonesMace:
                        case NPCID.HellArmoredBonesSword:
                            target.AddBuff(BuffType<ArmorCrunch>(), 240);
                            break;

                        default:
                            break;
                    }
                }
            }

            if (Main.expertMode)
            {
                switch (npc.type)
                {
                    case NPCID.Hellbat:
                        target.AddBuff(BuffID.OnFire, 120);
                        break;

                    default:
                        break;
                }
            }

            // GFB Brain and its Creepers can inflict literally any buff in the game
            // Yes this includes pets, light pets, mounts, whip tags, endgame debuffs, anything!
            if ((npc.type == NPCID.BrainofCthulhu || npc.type == NPCID.Creeper) && Main.zenithWorld)
            {
                int buffType = Main.rand.Next(BuffLoader.BuffCount);
                target.AddBuff(buffType, Main.rand.Next(300, 601));
            }
        }
        #endregion

        #region On Hit NPC

        public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
        {
            if (target.ModNPC is SunkenSeaNPC ssnpc)
                ssnpc.OnHitByNPC(npc);
        }

        #endregion

        #region Modify Hit
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            List<int> SharkIDs =
            [
                NPCID.Shark,
                NPCID.DukeFishron,
                NPCID.Sharkron,
                NPCID.Sharkron2,
                NPCID.SandShark,
                NPCID.SandsharkCorrupt,
                NPCID.SandsharkCrimson,
                NPCID.SandsharkHallow,
                NPCID.GoblinShark,
                NPCType<FusionFeeder>(),
                NPCType<GreatSandShark.GreatSandShark>(),
                NPCType<Mauler>(),
                NPCType<OldDuke.OldDuke>(),
                NPCType<SulphurousSharkron>(),
                NPCType<ReaperShark>()
            ];

            // Kaguya hair boom GIF
            if (SharkIDs.Contains(npc.type) && target.name == "Rebecca" && Main.zenithWorld)
            {
                SoundEngine.PlaySound(AresGaussNuke.NukeExplosionSound, target.Center);
                Main.LocalPlayer.SetScreenshake(12f);

                target.KillMe(PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Rebecca").ToNetworkText(target.name)), 1000.0, 0);
                modifiers.SourceDamage *= target.statLifeMax2 * Main.rand.NextFloat(3f, 6f);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile explosion = Projectile.NewProjectileDirect(npc.GetSource_FromThis(), target.Center, Vector2.Zero, ProjectileType<ScorpioLargeRocket>(), 9999, 0f, Main.myPlayer, ItemID.MiniNukeII, 0.01f);
                    explosion.friendly = false;
                    explosion.hostile = true;
                    explosion.timeLeft = 5;
                }
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.camper && !player.StandingStill())
                modifiers.SourceDamage *= 0.5f;

            if (IsArmored()) //Hide combat text so we can draw our own for armored NPCs
            {
                modifiers.HideCombatText();
            }

            // True melee resists
            if (item.CountsAsClass<MeleeDamageClass>() && item.type != ItemType<InfernaCutter>())
            {
                float damageMult = 1f;
                if (npc.type == NPCType<Crabulon.Crabulon>())
                    damageMult = 0.8f;
                else if (CalamityNPCTypeSets.EaterOfWorlds.Contains(npc.type) || npc.type == NPCID.Creeper || npc.type == NPCType<AstrumAureus.AstrumAureus>())
                    damageMult = 0.75f;
                else if (CalamityNPCTypeSets.Perforators.Contains(npc.type) || CalamityNPCTypeSets.AquaticScourge.Contains(npc.type) || CalamityNPCTypeSets.Destroyer.Contains(npc.type) ||
                    CalamityNPCTypeSets.Ravager.Contains(npc.type) || CalamityNPCTypeSets.AstrumDeus.Contains(npc.type) || CalamityNPCTypeSets.StormWeaver.Contains(npc.type) ||
                    npc.type == NPCType<ProfanedRocks>() || npc.type == NPCType<DarkEnergy>())
                    damageMult = 0.5f;
                else if (CalamityNPCTypeSets.Thanatos.Contains(npc.type))
                    damageMult = 0.35f;

                modifiers.SourceDamage *= damageMult;
            }
        }
        #endregion

        #region Modify Hit By Projectile
        public static bool DisableMultWhipTag = false;
        //this bool does nothing on the main branch, its just here so that CalTestHelpers doesn't crash searching for it
        //if you want to mess with this to test whips, please do so in the summoner branch - Shade

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();

            if (IsArmored()) //Hide combat text so we can draw our own for armored NPCs
            {
                modifiers.HideCombatText();
            }

            // Block natural falling stars from killing boss spawners randomly
            if ((projectile.type == ProjectileID.FallingStar && projectile.damage >= 1000) && (npc.type == NPCType<PerforatorCyst>() || npc.type == NPCType<HiveTumor>() || npc.type == NPCType<LeviathanStart>()))
                modifiers.SourceDamage *= 0f;

            // Supercrits
            var cgp = projectile.Calamity();
            if (cgp.supercritHits != 0)
            {
                cgp.supercritHits--;
                float critOver100 = (projectile.ContinuouslyUpdateDamageStats ? player.GetCritChance(projectile.DamageType) : projectile.CritChance) - 100f;

                // Supercrits can "supercrit" over and over for each extra 100% critical strike chance.
                // For example if you have 716% critical strike chance, you are guaranteed +700% damage and then have a 16% chance for +800% damage instead.
                if (critOver100 > 0f)
                {
                    int supercritLayers = (int)(critOver100 / 100f);
                    float lastLayerCritChance = critOver100 % 100f;
                    // Roll for the remaining crit chance
                    if (Main.rand.NextFloat(100f) <= lastLayerCritChance)
                        ++supercritLayers;

                    // 08MAR2025: Ozzatron: changed supercrit implementation to actually increase crit multiplier instead of multiplying source damage
                    // This means supercrits don't affect on-hits, just like regular crits don't
                    //
                    // Apply supercrit damage as a direct increase to the critical strike damage multiplier, which starts at 2.0 (aka 200%).
                    modifiers.CritDamage += supercritLayers;
                }
            }

            // 08MAR2025: Simplistic crit damage increase. Doesn't force a crit, though you can do that separately.
            modifiers.CritDamage += cgp.bonusCritDamage;

            //
            // DAAWNLIGHT SPIRIT ORIGIN AIM IMPLEMENTATION
            //
            if (modPlayer.spiritOrigin && projectile.CountsAsClass<RangedDamageClass>())
            {
                int bullseyeType = ProjectileType<SpiritOriginBullseye>();
                Projectile bullseye = null;
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.type != bullseyeType || p.owner != player.whoAmI)
                        continue;

                    // Only choose a bullseye if it is attached to the NPC that is being hit.
                    if (npc.whoAmI == (int)p.ai[0])
                    {
                        bullseye = p;
                        break;
                    }
                }

                // Don't allow large hitbox projectiles or explosions to "snipe" enemies.
                // Hitbox criteria were changed to allow long one dimensional projectiles so that Condemnation would work.
                bool acceptableVelocity = projectile.velocity != Vector2.Zero;
                bool acceptableHitbox = (projectile.width <= 36) || (projectile.height <= 36);
                if (bullseye != null && acceptableVelocity && acceptableHitbox)
                {
                    // Bullseyes are visually different on bosses and thus have larger hitboxes.
                    float bullseyeRadius = npc.IsABoss() ? DaawnlightSpiritOrigin.BossBullseyeRadius : DaawnlightSpiritOrigin.RegularEnemyBullseyeRadius;

                    // Do some geometry + trig to determine if the projectile WOULD hit the bullseye, even if it's about to be deleted on-hit.
                    // This is the equivalent of drawing a laser sight from the projectile along its velocity vector and seeing if it crosses the bullseye's hitbox.
                    // To do this more reliably, we back the projectile up quite a distance.
                    Vector2 normVelocity = projectile.velocity.SafeNormalize(Vector2.UnitY);
                    Vector2 backedUpPosition = projectile.Center - 160f * normVelocity;
                    Vector2 directionToBullseyeCenter = (bullseye.Center - backedUpPosition).SafeNormalize(Vector2.UnitY);
                    Vector2 perp = directionToBullseyeCenter.RotatedBy(MathHelper.PiOver2);
                    // Double the radius is given so that the cosine break-even point is right at the edge of the hitbox.
                    Vector2 comparisonPointOne = bullseye.Center + perp * 2f * bullseyeRadius;
                    Vector2 comparisonPointTwo = bullseye.Center - perp * 2f * bullseyeRadius;
                    Vector2 dirToPointOne = (comparisonPointOne - backedUpPosition).SafeNormalize(-Vector2.UnitX);
                    Vector2 dirToPointTwo = (comparisonPointTwo - backedUpPosition).SafeNormalize(Vector2.UnitX);

                    // Law of cosines: (A dot B) = |A| * |B| * cos(theta)
                    // where theta is the angle between the two vectors A and B.
                    // cos(theta) approaches one as the angle approaches zero, so an angle is smaller if the cos is bigger.
                    // If the angle to the bullseye's center is smaller than the angle to both the comparison points, it's a hit.
                    float dotCenter = Vector2.Dot(normVelocity, directionToBullseyeCenter);
                    float dotOne = Vector2.Dot(normVelocity, dirToPointOne);
                    float dotTwo = Vector2.Dot(normVelocity, dirToPointTwo);
                    bool willStrikeBullseye = dotCenter > dotOne && dotCenter > dotTwo;

                    // If a bullseye is triggered, set it as hit.
                    if (willStrikeBullseye)
                    {
                        // 08OCT2024: Ozzatron: this can be abused by firing a ton of shots then hotswapping to AMR while they are in flight
                        // we will need IEntitySource item use time provenance to fix this, and even that is unreliable with holdouts
                        modPlayer.spiritOriginCritBoost += player.HeldItem.useTime;

                        if (bullseye.ai[2] == 0f)
                        {
                            bullseye.timeLeft = DaawnlightSpiritOrigin.BullseyeHitLifetime;
                            bullseye.ai[2] = 1f;
                        }

                        if (Main.rand.NextBool(5))
                        {
                            int randomStarAmount = Main.rand.Next(3, 6);
                            float randomCircleRotation = Main.rand.NextFloat(MathHelper.TwoPi);
                            for (int i = 0; i < randomStarAmount; i++)
                            {
                                Particle fancyStars = new FancyStars(
                                bullseye.Center,
                                Main.rand.NextFloat(MathHelper.TwoPi) * Main.rand.NextBool().ToDirectionInt(),
                                Main.rand.NextFloat(0.42f, 0.63f),
                                (MathHelper.TwoPi / randomStarAmount * i).ToRotationVector2().RotatedBy(randomCircleRotation).RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(7f, 12f),
                                Main.rand.NextFloat(0.1f, 0.5f),
                                55,
                                new Color(Main.rand.Next(256), Main.rand.Next(256), Main.rand.Next(256)) * 1.2f);
                                GeneralParticleHandler.SpawnParticle(fancyStars);
                            }
                        }

                        bullseye.netUpdate = true;
                    }
                }
            }

            if (!projectile.npcProj && !projectile.trap)
            {
                // Plague Reaper deals extra damage to Plagued enemies
                if (projectile.CountsAsClass<RangedDamageClass>() && modPlayer.plagueReaper && plague)
                    modifiers.SourceDamage *= PlagueReaperMask.SetBonusPlaguedRangedDamageMult;

                // True Vulnerability Hex causes enemies to take 1.15x damage, 2.5x from Calamity itself
                if (trueVulnerabilityHex)
                    modifiers.SourceDamage *= (projectile.type == ProjectileType<DirectStrike>() && projectile.ai[1] == 255f) ? 2.5f : 1.15f;
            }

            // Apply balancing resists/vulnerabilities.
            BalancingChangesManager.ApplyFromProjectile(npc, ref modifiers, projectile);

            if (CalamityProjectileSets.ResistedExplosiveProjectile[projectile.type])
            {
                // Eater of Worlds has a vanilla resist in Expert+, this gives it to him in Normal mode
                // Note that Calamity reduces the vanilla resist from 80% to 60%
                bool hasResist = CalamityNPCTypeSets.EaterOfWorlds.Contains(npc.type) && !Main.expertMode;
                // Add a resist for BoC's creepers and Prehardmode worm bosses
                if (npc.type == NPCID.Creeper || CalamityNPCTypeSets.DesertScourge.Contains(npc.type) || CalamityNPCTypeSets.Perforators.Contains(npc.type))
                    hasResist = true;
                if (hasResist)
                    modifiers.SourceDamage *= 0.33f;
            }

            if (modPlayer.camper && !player.StandingStill())
                modifiers.SourceDamage *= 0.5f;

            if ((projectile.minion || ProjectileID.Sets.MinionShot[projectile.type] || projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type]) && (player.ownedProjectileCounts[ProjectileType<RelicOfDeliveranceSpear>()] > 0 || player.ownedProjectileCounts[ProjectileType<RelicOfConvergenceCrystal>()] > 0 || (player.Calamity().rOfResilienceCooldown == 0 && player.HeldItem.type == ItemType<RelicOfResilience>())))
                modifiers.SourceDamage *= 0.1f;

            //Doze apr-6-2025: with the summon tag system we now have this is unnececcessary and very likely causes issues on MP, so i'm commenting it out for the time being. Once further testing is done, delete it entirely.
            //Delete ardor blososm sparks and buff if hit by something that isnt a minion or sentry while not having Ardor Blossom Star in hand
            /*if (npc.HasBuff<ArdorBlossomSpark>() && player.HeldItem.type != ModContent.ItemType<ArdorBlossomStar>() && !projectile.minion && !ProjectileID.Sets.MinionShot[projectile.type] && !projectile.sentry)
            {
                npc.RequestBuffRemoval(ModContent.BuffType<ArdorBlossomSpark>());
                //Remove all embers from this enemy
                for (int k = 0; k < Main.maxProjectiles; k++)
                {
                    if (Main.projectile[k].active && Main.projectile[k].type == ModContent.ProjectileType<ArdorBlossomStarSpark>() && Main.projectile[k].ai[0] == 1f && Main.projectile[k].ai[1] == npc.whoAmI && Main.projectile[k].owner == player.whoAmI)
                        Main.projectile[k].Kill();
                }
            }*/
            //Handle summon tag effects
            if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type] || projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type])
            {
                EditSummonTagDamage(projectile, npc, ref modifiers);
            }


        }
        #endregion

        #region OnHitBy overrides
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damagedone)
        {
            if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type] || projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type])
            {
                SummonTagOnHitEffects(npc, projectile, hit, damagedone);
            }

            if (IsArmored())
            {
                CombatText.NewText(npc.Hitbox, Color.Gray, damagedone, hit.Crit);
            }
            if (projectile.type == ProjectileType<HyperiusDamage>() || projectile.type == ProjectileType<HyperiusBleed>())
            {
                float rate = (Main.GlobalTimeWrappedHourly * 3f);
                List<Color> eColors = new List<Color>()
                {
                    Color.Yellow,
                    Color.Magenta,
                    Color.Red,
                    Color.Cyan,
                    Color.Lime
                };
                int colorIndex = (int)(rate / 2 % eColors.Count);
                Color currentColor = eColors[colorIndex];
                Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
                Color usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

                CombatText.NewText(npc.Hitbox, usedColor, damagedone, hit.Crit, true);
            }

        }

        #endregion

        #region Summon Tag 
        //doze 03-15-2025: A full refactor of the summon tag system to make it easier to use and more flexible. Ping me with any questions.
        private void EditSummonTagDamage(Projectile proj, NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (proj.npcProj || proj.trap || proj.owner == -1) // don't run on non-player-owned projectiles.
                return;

            var player = Main.player[proj.owner];
            var modPlayer = player.Calamity();

            float critChance = modPlayer.bonusCritTag;
            float TagDamageMult = ProjectileID.Sets.SummonTagDamageMultiplier[proj.type];

            TagDamageMult += modPlayer.bonusMultTag;
            modifiers.FlatBonusDamage += modPlayer.bonusFlatTag;

            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                if (npc.buffTime[i] >= 1)
                {
                    int type = npc.buffType[i];
                    if (CalamityBuffSets.SummonTagDebuff.TryGetValue(type, out SummonTag tag))
                    {
                        tag.TagModifyHitEffects(proj, npc, ref modifiers, ref TagDamageMult, ref critChance);
                    }
                }
            }

            //For the vanilla Monk/Shinobi armor critting with Lightning Auras. In vanilla it doesn't stack additively but frankly I do not care. It's not like aura is that good anyway.
            if (proj.type == ProjectileID.DD2LightningAuraT1 || proj.type == ProjectileID.DD2LightningAuraT2 || proj.type == ProjectileID.DD2LightningAuraT3)
            {
                if (player.setMonkT3)
                {
                    critChance += 0.25f; // 1/4 chance to crit with Shinobi
                }
                else if (player.setMonkT2)
                {
                    critChance += 0.166f; // 1/6 chance to crit with Monk
                }
            }

            //Used to convert all multiplicative tag into crit chance and vice-versa. If both force tag crit and multiplicative are applied, chooses one at random.
            if (modPlayer.forceSummonTagCrit && !(modPlayer.forceSummonTagMultiplicative && Main.rand.NextBool()))
            {
                critChance += modifiers.ScalingBonusDamage.Value;
                modifiers.ScalingBonusDamage += -modifiers.ScalingBonusDamage.Value;

            }
            else if (modPlayer.forceSummonTagMultiplicative)
            {

                modifiers.ScalingBonusDamage += critChance;
                critChance = 0;
            }

            //currently doesn't support more than 100% crit chance, todo if something does more than +100% tag damage
            if (Main.rand.NextFloat() < critChance)
                modifiers.SetCrit();
            else
                modifiers.DisableCrit(); //This is to prevent Morning Star and Kalei from critting with their vanilla tag effect. If you want a minion/sentry to crit, you *must* make sure to change critChance in this function.
        }

        //This is for whip tag effects that run on hit and don't modify the damage of the hit.
        private void SummonTagOnHitEffects(NPC npc, Projectile projectile, NPC.HitInfo hit, int damagedone)
        {
            if (projectile.npcProj || projectile.trap || projectile.owner == -1) // don't run on non-player-owned projectiles.
                return;

            Player player = Main.player[projectile.owner];

            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                if (npc.buffTime[i] >= 1)
                {
                    int type = npc.buffType[i];
                    if (CalamityBuffSets.SummonTagDebuff.TryGetValue(type, out SummonTag tag))
                    {
                        tag.TagOnHit(npc, projectile, hit, damagedone);
                    }
                }
            }
        }
        #endregion

        #region Check Dead
        public override bool CheckDead(NPC npc)
        {
            if (npc.lifeMax > 1000 && npc.type != NPCID.DungeonSpirit &&
                npc.type != NPCType<PhantomSpirit>() &&
                npc.type != NPCType<PhantomSpiritS>() &&
                npc.type != NPCType<PhantomSpiritM>() &&
                npc.type != NPCType<PhantomSpiritL>() &&
                npc.value > 0f && !npc.boss && npc.HasPlayerTarget &&
                NPC.downedMoonlord &&
                Main.player[npc.target].ZoneDungeon)
            {
                // This value can change by (on average) 0.75x or 1.25x depending on your having positive or negative luck
                int baseValue = Main.expertMode ? 4 : 6;

                if (Main.player[npc.target].RollLuck(baseValue) == 0 && Main.wallDungeon[Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16].WallType])
                {
                    int randomType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        NPCType<PhantomSpirit>(),
                        NPCType<PhantomSpiritS>(),
                        NPCType<PhantomSpiritM>(),
                        NPCType<PhantomSpiritL>()
                    });

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, randomType);
                }
            }

            return true;
        }
        #endregion

        #region Hit Effect
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (npc.life <= 0 && npc.Organic() && ashesOnDeath > 0)
                DeathAshParticle.CreateAshesFromNPC(npc, Vector2.Zero);

            // Cultist shield flicker
            if (npc.type == NPCID.CultistBoss)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    newAI[1] = 35f;
                    npc.netUpdate = true;
                }
            }

            if (CalamityWorld.revenge)
            {
                switch (npc.type)
                {
                    case NPCID.PlanterasTentacle:
                        if (npc.life <= 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.position.X + (float)(npc.width / 2)), (int)(npc.position.Y + (float)npc.height), NPCType<PlanterasFreeTentacle>());
                        }
                        break;

                    case NPCID.MotherSlime:
                        if (npc.life <= 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int slimeAmt = Main.rand.Next(2) + 2; // 2 to 3 extra
                                for (int s = 0; s < slimeAmt; s++)
                                {
                                    int slime = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)(npc.position.Y + npc.height), NPCID.BlueSlime, 0, 0f, 0f, 0f, 0f, 255);
                                    NPC npc2 = Main.npc[slime];
                                    npc2.SetDefaults(NPCID.BabySlime);
                                    npc2.velocity.X = npc.velocity.X * 2f;
                                    npc2.velocity.Y = npc.velocity.Y;
                                    npc2.velocity.X += Main.rand.Next(-20, 20) * 0.1f + s * npc.direction * 0.3f;
                                    npc2.velocity.Y -= Main.rand.Next(10) * 0.1f + s;
                                    npc2.ai[0] = -1000 * Main.rand.Next(3);

                                    if (Main.dedServ && slime < Main.maxNPCs)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slime, 0f, 0f, 0f, 0, 0, 0);
                                }
                            }
                        }
                        break;

                    case NPCID.CursedHammer:
                    case NPCID.EnchantedSword:
                    case NPCID.CrimsonAxe:
                        if (Main.getGoodWorld)
                            npc.justHit = false;

                        break;

                    case NPCID.Clinger:
                    case NPCID.Gastropod:
                    case NPCID.GiantTortoise:
                    case NPCID.IceTortoise:
                    case NPCID.BlackRecluse:
                    case NPCID.BlackRecluseWall:
                        if (Main.getGoodWorld)
                            npc.justHit = false;

                        break;

                    case NPCID.Paladin:
                        if (Main.getGoodWorld)
                            npc.justHit = false;

                        break;
                }

                if (npc.type == NPCType<Plagueshell>())
                {
                    if (Main.getGoodWorld)
                        npc.justHit = false;
                }
            }

            // Plague debuff on kill effect
            if (plague && npc.life <= 0 && npc.realLife == -1)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        int DustID = 220;
                        Dust dust2 = Dust.NewDustDirect(npc.Center, npc.width, npc.height, DustID);
                        dust2.scale = Main.rand.NextFloat(0.6f, 0.75f);
                        dust2.velocity = new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 0.8f);
                        dust2.noGravity = true;
                    }

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC target = Main.npc[i];

                        if (target != null && target.IsAnEnemy(true, true) && !target.buffImmune[BuffType<Plague>()] && Vector2.Distance(target.Center, npc.Center) < 400)
                        {
                            if (target.HasBuff<Plague>() || target.life <= 0)
                            {
                                target.AddBuff(BuffType<Plague>(), 300);
                            }
                            else
                            {
                                target.AddBuff(BuffType<Plague>(), 300);
                                DirectionalPulseRing pulse = new DirectionalPulseRing(target.Center, Vector2.Zero, Main.rand.NextBool(3) ? Color.LimeGreen : Color.Green, new Vector2(1, 1), 0, Main.rand.NextFloat(0.07f, 0.18f) * 3, 0f, 15);
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                        }
                    }
                }
            }

            if (scionsCurioEffected && npc.life <= 0 && npc.realLife == -1)
            {
                for (int g = 0; g < 17; g++)
                {
                    int DustID = ModContent.DustType<SquashDust>();
                    Dust dust = Dust.NewDustPerfect(npc.Center, DustID);
                    dust.scale = Main.rand.NextFloat(1.1f, 1.35f);
                    dust.velocity = new Vector2(9, 9).RotatedByRandom(100) * Main.rand.NextFloat(0.4f, 0.9f) + Vector2.UnitY * -10;
                    dust.noGravity = false;
                    dust.color = Main.rand.NextBool() ? Color.Green : Color.Chartreuse;
                    dust.fadeIn = Main.rand.NextFloat(0.2f, 2f);
                }
                Particle blastvfx = new CustomPulse(npc.Center, Vector2.Zero, Color.Chartreuse * 0.9f, "CalamityMod/Particles/ShineExplosion1", Vector2.One, Main.rand.NextFloat(-10, 10), 0.05f, 0.15f, 10, true);
                GeneralParticleHandler.SpawnParticle(blastvfx);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.5f, 0.7f), MaxInstances = 6 }, npc.Center);

                int explosionDamage = 12;
                float highestDamage = 0;
                Player Owner = null;
                for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
                {
                    Player player = Main.player[playerIndex];

                    float playerRangedDamage = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(explosionDamage);
                    if (playerRangedDamage > highestDamage && player.Calamity().scionsCurio)
                    {
                        highestDamage = playerRangedDamage;
                        Owner = player;
                    }
                }

                // Create Blast
                float blastSize = 115;
                float minMultiplier = 0.5f;
                int hitsToMinMult = 5;
                int debuff = ModContent.BuffType<Irradiated>();
                int debuffTime = 300;
                Projectile blast = Projectile.NewProjectileDirect((Owner != null ? Owner.GetSource_FromThis() : npc.GetSource_FromThis()), npc.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(highestDamage), 7, (Owner != null ? Owner.whoAmI : -1), blastSize, minMultiplier, hitsToMinMult);
                blast.localAI[0] = debuff;
                blast.localAI[1] = debuffTime;
                blast.timeLeft = 15;
                blast.DamageType = DamageClass.Ranged;
            }

            bool fakeAbbadon = false; // Will be used for abbadon revamp
            if (fakeAbbadon && npc.life <= 0 && npc.realLife == -1)
            {
                float areaOfEffect = 500;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Color color1 = Color.Crimson;
                    Color color2 = Color.OrangeRed;
                    for (int r = 0; r < Main.maxNPCs; r++)
                    {
                        NPC target = Main.npc[r];

                        if (target != null && target.IsAnEnemy(true, true) && Vector2.Distance(target.Center, npc.Center) <= areaOfEffect)
                        {
                            if (target.life <= 0)
                            {
                                target.Calamity().scionsCurioEffected = true;
                            }
                            else
                            {
                                for (int g = 0; g < 7; g++)
                                {
                                    int DustID = ModContent.DustType<LightDust>();
                                    Dust dust2 = Dust.NewDustDirect(target.Center, target.width, target.height, DustID);
                                    dust2.scale = Main.rand.NextFloat(0.6f, 0.75f);
                                    dust2.velocity = new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 0.8f);
                                    dust2.noGravity = true;
                                    dust2.color = Main.rand.NextBool() ? color2 : color1;
                                }

                                Vector2 start = npc.Center;
                                Vector2 end = target.Center;
                                Color color = Main.rand.NextBool() ? color2 : color1;

                                Vector2 lerpVel = Vector2.Lerp(start, end, 0.5f);
                                float scale = 0.015f;
                                Particle spark = new CustomSpark(lerpVel, npc.SafeDirectionTo(target.Center), "CalamityMod/Particles/BloomLineThick", false, 18, scale, color, new Vector2(1.2f, (Utils.Distance(start, end) * 0.034f)), true, true, shrinkSpeed: 0.25f, glowOpacity: 0.75f);
                                GeneralParticleHandler.SpawnParticle(spark);
                                for (int u = 0; u < 2; u++)
                                {
                                    Vector2 pos = start;
                                    if (u == 0) pos = end;
                                    Particle spark2 = new CustomSpark(pos, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 18, 0.55f, color, Vector2.One, true, true, glowOpacity: 0.85f);
                                    GeneralParticleHandler.SpawnParticle(spark2);
                                }

                                float distance = Vector2.Distance(target.Center, npc.Center);
                                int maxDusts = (int)distance;
                                int dustCaper = 60;
                                int dustDivisor = maxDusts / dustCaper;
                                if (dustDivisor < 2)
                                    dustDivisor = 2;

                                Vector2 dustLineStart = target.Center;
                                Vector2 dustLineEnd = npc.Center;
                                Vector2 currentDustPos = default;
                                Vector2 dustVel = npc.Center.DirectionTo(target.Center);
                                int startingPoint = Main.rand.Next(0, 400 + 1);
                                Vector2 lastDustPos = default;
                                for (int i = 0; i < maxDusts; i++)
                                {
                                    float sine = (float)Math.Sin((i + startingPoint) * 0.425f / MathHelper.Pi);
                                    float endStartFade = Math.Min(Utils.GetLerpValue(maxDusts * 0.8f, 0, i), Utils.GetLerpValue(0 + maxDusts * 0.2f, maxDusts, i));
                                    currentDustPos = Vector2.Lerp(dustLineStart, dustLineEnd, i / (float)maxDusts) + dustVel.RotatedBy(MathHelper.PiOver2) * 6 * sine * endStartFade;
                                    if (i == 0)
                                        lastDustPos = currentDustPos;

                                    /*Dust dustLine = Dust.NewDustPerfect(currentDustPos, ModContent.DustType<SquashDust>());
                                    dustLine.position = currentDustPos;
                                    dustLine.velocity = Vector2.Zero;
                                    dustLine.noGravity = true;
                                    dustLine.scale = 1.5f * Math.Max(endStartFade, 0.7f);
                                    dustLine.fadeIn = Main.rand.NextFloat() * 2f;
                                    dustLine.color = Color.Lerp(color1, color2, Utils.GetLerpValue(0, maxDusts, i));*/

                                    currentDustPos = Vector2.Lerp(dustLineStart, dustLineEnd, i / (float)maxDusts) + dustVel.RotatedBy(MathHelper.PiOver2) * 55 * sine * endStartFade;
                                    Dust dustLinger = Dust.NewDustPerfect(currentDustPos, ModContent.DustType<SquashDust>());
                                    dustLinger.position = currentDustPos;
                                    dustLinger.velocity = currentDustPos.DirectionTo(lastDustPos) * (Main.rand.NextBool(5) ? 4f : Main.rand.NextFloat(0.2f, 0.8f));
                                    dustLinger.noGravity = true;
                                    dustLinger.scale = Main.rand.NextFloat(0.8f, 1f) * 1.5f;
                                    dustLinger.fadeIn = Main.rand.NextFloat(0.6f, 1f) * 4;
                                    dustLinger.color = Color.Lerp(color1, color2, Utils.GetLerpValue(0, maxDusts, i));
                                    
                                    lastDustPos = currentDustPos;
                                }

                                target.Calamity().scionsCurioEffected = true;
                                if (target.Calamity().irradiated)
                                    target.buffTime[target.FindBuffIndex(ModContent.BuffType<Irradiated>())] += 90;
                                else
                                    target.AddBuff(BuffType<Irradiated>(), 180);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Edit Spawn Rate
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            // Biomes
            if (player.Calamity().ZoneSulphur)
            {
                spawnRate = (int)(spawnRate * 1.1);
                maxSpawns = (int)(maxSpawns * 0.8f);
                if (Main.raining)
                {
                    spawnRate = (int)(spawnRate * 0.7);
                    maxSpawns = (int)(maxSpawns * 1.2f);

                    if (!player.Calamity().ZoneAbyss && AcidRainEvent.AcidRainEventIsOngoing)
                    {
                        if (AcidRainEvent.AnyRainMinibosses)
                        {
                            maxSpawns = 5;
                            spawnRate *= 2;
                        }
                        else
                        {
                            spawnRate = Main.hardMode ? 36 : 33;
                            maxSpawns = Main.hardMode ? 15 : 12;
                        }
                    }
                }
            }
            else if (player.Calamity().ZoneAbyss)
            {
                spawnRate = (int)(spawnRate * 0.7);
                maxSpawns = (int)(maxSpawns * 1.1f);
            }
            else if (player.Calamity().ZoneCalamity)
            {
                spawnRate = (int)(spawnRate * 0.9);
                maxSpawns = (int)(maxSpawns * 1.1f);
            }
            else if (player.Calamity().ZoneAstral)
            {
                spawnRate = (int)(spawnRate * 0.6);
                maxSpawns = (int)(maxSpawns * 1.2f);
            }
            else if (player.Calamity().ZoneSunkenSea)
            {
                spawnRate = (int)(spawnRate * 0.9);
                maxSpawns = (int)(maxSpawns * 1.1f);
            }

            // Boosts
            if (DownedBossSystem.downedDoG && (Main.pumpkinMoon || Main.snowMoon || Main.eclipse))
            {
                spawnRate = (int)(spawnRate * 0.75);
                maxSpawns = (int)(maxSpawns * 3f);
            }

            if (player.Calamity().clamity)
            {
                spawnRate = (int)(spawnRate * 0.02);
                maxSpawns = (int)(maxSpawns * 1.5f);
            }

            if (CalamityWorld.death && Main.bloodMoon && player.position.Y < Main.worldSurface * 16.0)
            {
                spawnRate = (int)(spawnRate * 0.25);
                maxSpawns = (int)(maxSpawns * 5f);
            }

            if (CalamityWorld.death && player.ZoneGraveyard)
            {
                spawnRate = (int)(spawnRate * 0.6667);
                maxSpawns = (int)(maxSpawns * 1.5f);
            }

            if (NPC.LunarApocalypseIsUp)
            {
                if ((player.ZoneTowerNebula && NPC.ShieldStrengthTowerNebula == 0) || (player.ZoneTowerStardust && NPC.ShieldStrengthTowerStardust == 0) ||
                    (player.ZoneTowerVortex && NPC.ShieldStrengthTowerVortex == 0) || (player.ZoneTowerSolar && NPC.ShieldStrengthTowerSolar == 0))
                {
                    spawnRate = (int)(spawnRate * 0.85);
                    maxSpawns = (int)(maxSpawns * 1.25f);
                }
            }

            if (CalamityWorld.revenge)
                spawnRate = (int)(spawnRate * 0.85);

            if (player.Calamity().chaosCandle)
            {
                spawnRate = (int)(spawnRate * 0.5); // 2x spawn rate
                maxSpawns = (int)(maxSpawns * 2f);
            }
            if (player.Calamity().zerg)
            {
                spawnRate = (int)(spawnRate * 0.25); // 4x spawn rate
                maxSpawns = (int)(maxSpawns * 4f);
            }

            // Reductions
            if (player.Calamity().tranquilityCandle)
            {
                spawnRate = (int)(spawnRate * 1.6666); // 0.6x spawn rate
                maxSpawns = (int)(maxSpawns * 0.6f);
            }
            if (player.Calamity().zen || (CalamityServerConfig.Instance.ForceTownSafety && player.townNPCs > 1f && Main.expertMode))
            {
                spawnRate = (int)(spawnRate * 2.5); // 0.4x spawn rate
                maxSpawns = (int)(maxSpawns * 0.4f);
            }
            if (player.Calamity().isNearbyBoss && CalamityServerConfig.Instance.BossZen)
            {
                spawnRate *= 5;
                maxSpawns = (int)(maxSpawns * 0.001f);
            }
        }
        #endregion

        #region Edit Spawn Range
        public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY)
        {
            if (player.Calamity().ZoneAbyss)
            {
                spawnRangeX = (int)(1920 / 16 * 0.5); //0.7
                safeRangeX = (int)(1920 / 16 * 0.32); //0.52
            }
        }
        #endregion

        #region Edit Spawn Pool

        internal static readonly FieldInfo MaxSpawnsField = typeof(NPC).GetField("maxSpawns", BindingFlags.NonPublic | BindingFlags.Static);

        public static void AttemptToSpawnLabCritters(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int spawnRate = 400;
            int maxSpawnCount = (int)MaxSpawnsField.GetValue(null);
            NPCLoader.EditSpawnRate(player, ref spawnRate, ref maxSpawnCount);

            // Enforce a limit on the amount of enemies that can appear.
            if (player.nearbyActiveNPCs >= maxSpawnCount)
                return;

            float playerCenterX = player.Center.X / 16f;
            float playerCenterY = player.Center.Y / 16f;
            Vector2 sunkenSeaLabCenter = CalamityWorld.SunkenSeaLabCenter / 16f;
            Vector2 planetoidLabCenter = CalamityWorld.PlanetoidLabCenter / 16f;
            Vector2 jungleLabCenter = CalamityWorld.JungleLabCenter / 16f;
            Vector2 hellLabCenter = CalamityWorld.HellLabCenter / 16f;
            Vector2 iceLabCenter = CalamityWorld.IceLabCenter / 16f;
            for (int i = 0; i < 8; i++)
            {
                int checkPositionX = (int)(playerCenterX + Main.rand.Next(30, 54) * Main.rand.NextBool().ToDirectionInt());
                int checkPositionY = (int)(playerCenterY + Main.rand.Next(24, 45) * Main.rand.NextBool().ToDirectionInt());
                Vector2 checkPosition = new Vector2(checkPositionX, checkPositionY);

                Tile aboveSpawnTile = CalamityUtils.ParanoidTileRetrieval(checkPositionX, checkPositionY - 1);
                bool nearLab = CalamityUtils.ManhattanDistance(checkPosition, sunkenSeaLabCenter) < 180f;
                nearLab |= CalamityUtils.ManhattanDistance(checkPosition, planetoidLabCenter) < 180f;
                nearLab |= CalamityUtils.ManhattanDistance(checkPosition, jungleLabCenter) < 180f;
                nearLab |= CalamityUtils.ManhattanDistance(checkPosition, hellLabCenter) < 180f;
                nearLab |= CalamityUtils.ManhattanDistance(checkPosition, iceLabCenter) < 180f;
                bool nearPlagueLab = CalamityUtils.ManhattanDistance(checkPosition, jungleLabCenter) < 180f;

                bool isLabWall = aboveSpawnTile.WallType == WallType<HazardChevronWall>() || aboveSpawnTile.WallType == WallType<LaboratoryPanelWall>() || aboveSpawnTile.WallType == WallType<LaboratoryPlateBeam>();
                isLabWall |= aboveSpawnTile.WallType == WallType<LaboratoryPlatePillar>() || aboveSpawnTile.WallType == WallType<LaboratoryPlatingWall>() || aboveSpawnTile.WallType == WallType<RustedPlateBeam>();
                if (!isLabWall || !nearLab || Collision.SolidCollision((checkPosition - new Vector2(2f, 2f)).ToWorldCoordinates(), 4, 4) || player.nearbyActiveNPCs >= maxSpawnCount || !Main.rand.NextBool(spawnRate))
                    continue;

                WeightedRandom<int> pool = new WeightedRandom<int>();
                pool.Add(NPCID.None, 0f);
                pool.Add(NPCType<RepairUnitCritter>(), 0.025f);
                pool.Add(NPCType<Androomba>(), 0.01f);
                // Normal droids are replaced with plague droids in the Jungle Lab.
                if (nearPlagueLab)
                {
                    pool.Add(NPCType<NanodroidPlagueGreen>(), 0.025f);
                    pool.Add(NPCType<NanodroidPlagueRed>(), 0.025f);
                    pool.Add(NPCType<NanodroidDysfunctional>(), 0.02f);
                }
                else
                {
                    pool.Add(NPCType<Nanodroid>(), 0.05f);
                    pool.Add(NPCType<NanodroidDysfunctional>(), 0.05f);
                }

                int typeToSpawn = pool.Get();
                if (typeToSpawn != NPCID.None)
                {
                    int spawnedNPC = NPCLoader.SpawnNPC(typeToSpawn, checkPositionX, checkPositionY - 1);
                    if (Main.dedServ && spawnedNPC < Main.maxNPCs)
                    {
                        Main.npc[spawnedNPC].position.Y -= 8f;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spawnedNPC);
                        return;
                    }
                }
            }
        }

        public static void AttemptToSpawnLavaNPCs(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // For now, we only need this for the Basalt Gully, but this may be used for the crags in the future
            if (!player.Calamity().ZoneBasaltGully)
                return;

            int spawnRate = 400;
            int maxSpawnCount = (int)MaxSpawnsField.GetValue(null);
            NPCLoader.EditSpawnRate(player, ref spawnRate, ref maxSpawnCount);

            // Enforce a limit on the amount of enemies that can appear.
            if (player.nearbyActiveNPCs >= maxSpawnCount)
                return;

            float playerCenterX = player.Center.X / 16f;
            float playerCenterY = player.Center.Y / 16f;
            for (int i = 0; i < 8; i++)
            {
                int checkPositionX = (int)(playerCenterX + Main.rand.Next(30, 54) * Main.rand.NextBool().ToDirectionInt());
                int checkPositionY = (int)(playerCenterY + Main.rand.Next(24, 45) * Main.rand.NextBool().ToDirectionInt());
                Vector2 checkPosition = new Vector2(checkPositionX, checkPositionY);

                Tile aboveSpawnTile = CalamityUtils.ParanoidTileRetrieval(checkPositionX, checkPositionY - 1);

               if (aboveSpawnTile.LiquidAmount < 255 || aboveSpawnTile.LiquidType != LiquidID.Lava || Collision.SolidCollision((checkPosition - new Vector2(2f, 2f)).ToWorldCoordinates(), 4, 4) || player.nearbyActiveNPCs >= maxSpawnCount || !Main.rand.NextBool(spawnRate))
                    continue;

                WeightedRandom<int> pool = new WeightedRandom<int>();
                pool.Add(NPCID.None, 1f);
                pool.Add(NPCType<PodobooKoi>(), 0.05f);

                int typeToSpawn = pool.Get();
                if (typeToSpawn != NPCID.None)
                {
                    int spawnedNPC = NPCLoader.SpawnNPC(typeToSpawn, checkPositionX, checkPositionY - 1);
                    if (Main.dedServ && spawnedNPC < Main.maxNPCs)
                    {
                        Main.npc[spawnedNPC].position.Y -= 8f;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spawnedNPC);
                        return;
                    }
                }
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            bool calamityBiomeZone = spawnInfo.Player.Calamity().ZoneAbyss ||
                spawnInfo.Player.Calamity().ZoneCalamity ||
                spawnInfo.Player.Calamity().ZoneSulphur ||
                spawnInfo.Player.Calamity().ZoneSunkenSea ||
                (spawnInfo.Player.Calamity().ZoneAstral && !spawnInfo.Player.PillarZone());

            // Fuck the Goblin and the Wizard
            if (!spawnInfo.Water && spawnInfo.Player.ZoneRockLayerHeight)
            {
                if (NPC.downedGoblins && !NPC.savedGoblin)
                {
                    if (!NPC.AnyNPCs(NPCID.BoundGoblin))
                        pool[NPCID.BoundGoblin] = SpawnCondition.BoundCaveNPC.Chance * 5f;
                }

                if (Main.hardMode && !NPC.savedWizard)
                {
                    if (!NPC.AnyNPCs(NPCID.BoundWizard))
                        pool[NPCID.BoundWizard] = SpawnCondition.BoundCaveNPC.Chance * 5f;
                }
            }

            // Fuck Chaos Elementals, overrides the vanilla spawn so they can be afk farmed once more
            if (Main.hardMode && spawnInfo.Player.ZoneRockLayerHeight && !calamityBiomeZone)
            {
                // Added more tiles for them to spawn on
                bool isChaosElementalSpawnTile =
                    spawnInfo.SpawnTileType == TileID.Pearlstone ||
                    spawnInfo.SpawnTileType == TileID.Pearlsand ||
                    spawnInfo.SpawnTileType == TileID.HallowedIce ||
                    spawnInfo.SpawnTileType == TileID.HallowedGrass ||
                    spawnInfo.SpawnTileType == TileID.HallowHardenedSand ||
                    spawnInfo.SpawnTileType == TileID.HallowSandstone;

                if (isChaosElementalSpawnTile)
                    pool[NPCID.ChaosElemental] = SpawnCondition.Cavern.Chance * 0.125f;
            }

            // Spawn Green Jellyfish in prehm and Blue Jellyfish in hardmode
            if (spawnInfo.Player.ZoneRockLayerHeight && spawnInfo.Water && !calamityBiomeZone)
            {
                if (!Main.hardMode)
                    pool[NPCID.GreenJellyfish] = SpawnCondition.CaveJellyfish.Chance * 0.5f;
                else
                    pool[NPCID.BlueJellyfish] = SpawnCondition.CaveJellyfish.Chance;
            }

            // Add Truffle Worm spawns to surface mushroom biome
            if (spawnInfo.Player.ZoneGlowshroom && Main.hardMode && (spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneSkyHeight))
            {
                if (NPC.CountNPCS(NPCID.TruffleWorm) < 2)
                    pool[NPCID.TruffleWorm] = SpawnCondition.OverworldMushroom.Chance * 0.5f;
            }

            // Add Prismatic Lacewing spawns to surface hallow from dusk to midnight
            if (!Main.dayTime && Main.time < 16200D && Main.hardMode && (spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneSkyHeight))
            {
                if (!NPC.AnyNPCs(NPCID.EmpressButterfly))
                    pool[NPCID.EmpressButterfly] = SpawnCondition.OverworldHallow.Chance * 0.1f;
            }

            // Increase fairy spawn rates while wearing Fairy Boots
            if (spawnInfo.Player.Calamity().fairyBoots)
            {
                int maxFairies = 5;
                if ((NPC.CountNPCS(NPCID.FairyCritterBlue) + NPC.CountNPCS(NPCID.FairyCritterGreen) + NPC.CountNPCS(NPCID.FairyCritterPink)) < maxFairies)
                {
                    if (!NPC.AnyNPCs(NPCID.FairyCritterBlue))
                        pool[NPCID.FairyCritterBlue] = SpawnCondition.Overworld.Chance * 5f;
                    if (!NPC.AnyNPCs(NPCID.FairyCritterGreen))
                        pool[NPCID.FairyCritterGreen] = SpawnCondition.Overworld.Chance * 5f;
                    if (!NPC.AnyNPCs(NPCID.FairyCritterPink))
                        pool[NPCID.FairyCritterPink] = SpawnCondition.Overworld.Chance * 5f;
                }
            }

            // Increased Maggot Zombie,the Groom, and the Bride spawn rates in a Graveyard
            if (spawnInfo.Player.ZoneGraveyard)
            {
                pool[NPCID.MaggotZombie] = SpawnCondition.OverworldNightMonster.Chance * 0.2f;
                pool[NPCID.TheGroom] = SpawnCondition.OverworldNightMonster.Chance * 0.035f;
                pool[NPCID.TheBride] = SpawnCondition.OverworldNightMonster.Chance * 0.035f;

            }

            // Disable vanilla spawns while in the Brimstone Crag
            if (calamityBiomeZone)
            {
                pool[0] = 0f;
            }

            // Add Enchanted Nightcrawlers as a critter to the Astral Infection
            if (!AnyEvents(spawnInfo.Player) && spawnInfo.Player.InAstral())
            {
                pool[NPCID.EnchantedNightcrawler] = SpawnCondition.TownCritter.Chance;
            }

            if (spawnInfo.Player.Calamity().ZoneSulphur && !spawnInfo.Player.Calamity().ZoneAbyss && AcidRainEvent.AcidRainEventIsOngoing)
            {
                pool.Clear();

                if (!(DownedBossSystem.downedPolterghast && AcidRainEvent.AccumulatedKillPoints == 1))
                {
                    Dictionary<int, AcidRainSpawnData> PossibleEnemies = AcidRainEvent.PossibleEnemiesPreHM;
                    Dictionary<int, AcidRainSpawnData> PossibleMinibosses = new Dictionary<int, AcidRainSpawnData>();
                    if (DownedBossSystem.downedAquaticScourge)
                    {
                        PossibleEnemies = AcidRainEvent.PossibleEnemiesAS;
                        PossibleMinibosses = AcidRainEvent.PossibleMinibossesAS;
                        if (!PossibleEnemies.ContainsKey(NPCType<IrradiatedSlime>()))
                        {
                            PossibleEnemies.Add(NPCType<IrradiatedSlime>(), new AcidRainSpawnData(1, 0f, AcidRainSpawnRequirement.Anywhere));
                        }
                    }
                    if (DownedBossSystem.downedPolterghast)
                    {
                        PossibleEnemies = AcidRainEvent.PossibleEnemiesPolter;
                        PossibleMinibosses = AcidRainEvent.PossibleMinibossesPolter;
                    }
                    foreach (int enemy in PossibleEnemies.Select(enemyType => enemyType.Key))
                    {
                        bool canSpawn = true;
                        switch (PossibleEnemies[enemy].SpawnRequirement)
                        {
                            case AcidRainSpawnRequirement.Anywhere:
                                break;
                            case AcidRainSpawnRequirement.Land:
                                canSpawn = !spawnInfo.Water;
                                break;
                            case AcidRainSpawnRequirement.Water:
                                canSpawn = spawnInfo.Water;
                                break;
                        }
                        if (canSpawn)
                        {
                            if (!pool.ContainsKey(enemy))
                            {
                                pool.Add(enemy, PossibleEnemies[enemy].SpawnRate);
                            }
                        }
                    }
                    if (PossibleMinibosses.Count > 0)
                    {
                        foreach (int miniboss in PossibleMinibosses.Select(miniboss => miniboss.Key).ToList())
                        {
                            bool canSpawn = true;
                            switch (PossibleMinibosses[miniboss].SpawnRequirement)
                            {
                                case AcidRainSpawnRequirement.Anywhere:
                                    break;
                                case AcidRainSpawnRequirement.Land:
                                    canSpawn = !spawnInfo.Water;
                                    break;
                                case AcidRainSpawnRequirement.Water:
                                    canSpawn = spawnInfo.Water;
                                    break;
                            }
                            if (canSpawn)
                            {
                                pool.Add(miniboss, PossibleMinibosses[miniboss].SpawnRate);
                            }
                        }
                    }
                    if (NPC.CountNPCS(NPCType<NuclearToad>()) >= AcidRainEvent.MaxNuclearToadCount)
                    {
                        pool.Remove(NPCType<NuclearToad>());
                    }
                }
            }

            if (spawnInfo.PlayerSafe)
                return;

            // Voodoo Demon changes (including partial Voodoo Demon Voodoo Doll implementation)
            bool voodooDemonDollActive = spawnInfo.Player.Calamity().disableVoodooSpawns;

            // If the doll is active, Voodoo Demons cannot spawn (via modded means).
            if (voodooDemonDollActive)
                pool.Remove(NPCID.VoodooDemon);
        }
        #endregion

        #region On Spawn
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type == NPCID.Deerclops)
            {
                DeerclopsAI.hasTargetBeenInRange = false;
                DeerclopsAI.borderDelay = 7f * 60f;
                DeerclopsAI.borderScalar = 0f;
                DeerclopsAI.innerBorder = DeerclopsAI.MaxDRIncreaseDistance * 5f;
                DeerclopsAI.outerBorder = DeerclopsAI.MaxDRIncreaseDistance * 5f;
            }

            // Despawn Blazing Wheels and Spike Balls when a boss spawns so they're not annoying and stay in the arena
            if (npc.boss)
            {
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.type == NPCID.BlazingWheel || n.type == NPCID.SpikeBall)
                    {
                        n.active = false;
                        n.netUpdate = true;
                    }
                }
            }

            if (npc.type != NPCID.VoodooDemon)
                return;

            // This entity source does not provide a player. So we have to find out if anyone close enough has a doll.
            if (source is EntitySource_SpawnNPC)
            {
                bool voodooDemonDollActive = false;
                Vector2 v = npc.Center;
                for (int i = 0; i < Main.maxPlayers; ++i)
                {
                    Player p = Main.player[i];
                    if (p is null || !p.active)
                        continue;
                    if (p.DistanceSQ(v) < 4000000f && p.Calamity().disableVoodooSpawns) // 2000 pixel radius
                    {
                        voodooDemonDollActive = true;
                        break;
                    }
                }
                if (!voodooDemonDollActive)
                    return;

                npc.Transform(NPCID.Demon);
                npc.netUpdate = true;
            }
        }
        #endregion

        #region Drawing
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (CalamityWorld.revenge || BossRushEvent.BossRushActive)
            {
                // Used to force the head to look to the sides for the laser spread attack
                if (npc.type == NPCID.GolemHead && (npc.ai[0] == 2f || npc.ai[0] == 3f))
                {
                    if (npc.localAI[1] == 1f)
                        npc.frame.Y = frameHeight * 2;
                    else
                        npc.frame.Y = frameHeight * 4;
                }
            }
            // Increment the bestiary worm timer when hovering over the NPC or having their entry open. Pauses otherwise
            if (npc.IsABestiaryIconDummy)
            {
                bestiaryWormTimer += 0.02f;
                // Resets after an hour. No sane human being is looking at a bestiary entry for an hour straight
                if (bestiaryWormTimer > 4320)
                {
                    bestiaryWormTimer = 0;
                }
            }


        }

        // Debuff visuals. Alphabetical order as per usual, please
        // TODO - Merge these into DebuffData
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!npc.canDisplayBuffs)
                return;

            if (absorberAffliction)
                AbsorberAffliction.DrawEffects(npc, ref drawColor);

            // Rancor's burn effect
            if (ashesOnDeath > 0)
            {
                if (Main.rand.NextBool(4))
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextVector2Circular(2.75f, 6.5f), ProjectileType<RancorFog>(), 0, 0f, Main.myPlayer, 0f, 0.475f);
                }
                if (Main.rand.NextBool(6))
                {
                    Vector2 randomPosition = new(npc.position.X + Main.rand.NextFloat(-10f, npc.width + 10f), npc.position.Y + Main.rand.NextFloat(-10f, npc.height + 10f));
                    RancorLavaMetaball.SpawnParticle(randomPosition, Main.rand.NextFloat(30f, 37f));
                }
            }

            if (astralInfection)
                AstralInfectionDebuff.DrawEffects(npc, ref drawColor);

            // Brimstone Flames and Demonshade Enrage set bonus share the same visual effects
            // TODO -- change this when Demonshade is reworked
            if (brimstoneFlames || npc.HasBuff<Enraged>())
                BrimstoneFlames.DrawEffects(npc, ref drawColor);

            if (demonicFlames)
                DemonicFlames.DrawEffects(npc, ref drawColor);

            if (burningBlood )
                BurningBlood.DrawEffects(npc, ref drawColor);

            if (brainRot)
                BrainRot.DrawEffects(npc, ref drawColor);

            if (crushDepth)
                CrushDepth.DrawEffects(npc, ref drawColor);

            if (hadopelagicPressure)
                HadopelagicPressure.DrawEffects(npc, ref drawColor);

            if (dragonFire)
                Dragonfire.DrawEffects(npc, ref drawColor);

            if (vermillionFlux)
                VermillionFlux.DrawEffects(npc, ref drawColor);

            if (auricRebuke)
                AuricRebuke.DrawEffects(npc, ref drawColor);

            if (staticDischarge)
                StaticDischarge.DrawEffects(npc, ref drawColor);

            if (elementalMix)
                ElementalMix.DrawEffects(npc, ref drawColor);

            // Eutrophication and Temporal Sadness share the same visual effects
            if (eutrophication || temporalSadness)
                Eutrophication.DrawEffects(npc, ref drawColor);

            if (godSlayerInferno)
                GodSlayerInferno.DrawEffects(npc, ref drawColor);

            // Holy Flames and Banishing Fire share the same visual effects
            if (holyFlames || banishingFire)
                HolyFlames.DrawEffects(npc, ref drawColor);

            if (heavyBleeding)
                HeavyBleeding.DrawEffects(npc, ref drawColor);
            
            if (hyperiusFxTimer > 0)
            {
                float rate = (Main.GlobalTimeWrappedHourly * 5);
                List<Color> eColors = new List<Color>()
                {
                    Color.Yellow,
                    Color.Magenta,
                    Color.Red,
                    Color.Cyan,
                    Color.Lime
                };
                int colorIndex = (int)(rate / 2 % eColors.Count);
                Color currentColor = eColors[colorIndex];
                Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
                Color usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

                Texture2D tex2 = Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
                Texture2D sparkle = Request<Texture2D>("CalamityMod/Particles/BloomLineSoftEdge").Value;
                Vector2 drawPosition = npc.Center - Main.screenPosition;
                float drawRotation = npc.rotation + (npc.spriteDirection == -1 ? MathHelper.Pi : 0f);

                float power = (float)(Math.Pow(Utils.GetLerpValue(0, 20, hyperiusFxTimer, true), 3)) * MathHelper.Lerp(Math.Max(npc.height, npc.width) / 100, 1.4f, 0.5f);
                for (int i = 0; i < 4; i++)
                {
                    float iMult = (1 + 0.25f * i);
                    Main.EntitySpriteDraw(tex2, drawPosition, null, Color.Lerp(usedColor, Color.White, i * 0.1f) with { A = 0 } * 0.6f, Main.rand.NextFloat(-5f, 5f), tex2.Size() * 0.5f, new Vector2(1f, 0.8f) * 0.35f * Main.rand.NextFloat(0.9f, 1.1f) * iMult * power * (Utils.GetLerpValue(0, 20, hyperiusFxTimer)), SpriteEffects.None);

                    for (int b = -1; b <= 1; b += 2)
                    {
                        float uncappedSine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f / MathHelper.Pi);
                        float sine = MathHelper.Lerp(Math.Abs(uncappedSine), 0.75f, 0.75f);
                        Vector2 scale = new Vector2((0.25f / iMult) + (0.7f * (1 - sine)), 1.1f * sine * iMult) * power * 0.05f;
                        float rotation = MathHelper.PiOver4 * b * uncappedSine;
                        Main.EntitySpriteDraw(sparkle, drawPosition, null, Color.Lerp(usedColor, Color.White, i * 0.1f) with { A = 0 }, rotation, sparkle.Size() * 0.5f, scale, SpriteEffects.None);
                    }
                }
            }

            if (laceration)
                Laceration.DrawEffects(npc, ref drawColor);

            if (laserBurnTimer > 0)
            {
                int particleChance = Math.Max(3, 10 - (laserBurnStacks / 3));
                if (laserBurnTimer % particleChance == 0)
                {
                    Vector2 randPosition = new Vector2(npc.position.X + Main.rand.Next(0, npc.width), npc.position.Y + Main.rand.Next(0, npc.height));

                    Dust dust = Dust.NewDustPerfect(randPosition, Effects.ArsenalEffects.ArsenalLaserDust);
                    dust.velocity = ((Vector2.UnitX * 3 * (laserBurnStacks * 0.03f)).RotatedByRandom(100) * Main.rand.NextFloat(0.85f, 1f)) + npc.velocity * 0.5f;
                    dust.scale = Main.rand.NextFloat(0.55f, 0.7f) + laserBurnStacks * 0.01f;
                    dust.noGravity = true;
                    dust.color = Color.Red;
                    dust.fadeIn = laserBurnStacks * 0.3f;
                }
                if (laserBurnType == 0)
                {
                    Main.NewText("No Burn Type Set", Color.OrangeRed);
                    laserBurnMarked = false;
                    laserBurnTimer = 0;
                }
            }

            // These draw effects do not include Miracle Blight's shader
            if (miracleBlight)
                MiracleBlight.DrawEffects(npc, ref drawColor);

            if (nightwither)
                Nightwither.DrawEffects(npc, ref drawColor);

            if (pearlAura)
                PearlAura.DrawEffects(npc, ref drawColor);

            if (plague) // Plague debuff
                Plague.DrawEffects(npc, ref drawColor);

            if (relicOfResilienceWeakness)
                ProfanedWeakness.DrawEffects(npc, ref drawColor);

            if (riptide)
                RiptideDebuff.DrawEffects(npc, ref drawColor);
            
            if (somaShredStacks > 0 && !Main.dedServ)
                Shred.DrawEffects(npc, this, ref drawColor);
            
            if (sulphurPoison)
                SulphuricPoisoning.DrawEffects(npc, ref drawColor);

            if (trueVulnerabilityHex)
                TrueVulnerabilityHex.DrawEffects(npc, ref drawColor);

            if (vaporfied)
                Vaporfied.DrawEffects(npc, ref drawColor);

            if (veriumDoomTimer > 0)
            {
                int sparkleChance = Math.Max(2, 8 - (veriumDoomStacks / 2));
                if (veriumDoomTimer % sparkleChance == 0)
                {
                    float veriumRatio = (float)veriumDoomTimer / (float)veriumDoomTime;
                    Vector2 randPosition = new Vector2(npc.position.X + Main.rand.Next(0, npc.width), npc.position.Y + Main.rand.Next(0, npc.height));
                    Particle markedSparkle = new CustomPulse(randPosition, Vector2.Zero, Color.Lerp(new Color(103, 230, 240), new Color(255, 110, 220), 1 - veriumRatio), "CalamityMod/Particles/Sparkle", Vector2.One, Main.rand.NextFloat(-0.75f, 0.75f), 0.9f, 1.1f, 35);
                    GeneralParticleHandler.SpawnParticle(markedSparkle);
                }
            }

            if (voidfrost)
                Voidfrost.DrawEffects(npc, ref drawColor);

            // TODO -- These debuff visuals cannot be moved because they correspond to vanilla debuffs
            if (electrified)
            {
                if (Main.rand.NextBool())
                {
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, 0.35f);
                }
            }
            if (webbed)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustID.Web, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.1f;
                    Main.dust[dust].velocity.Y += 0.25f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }

            // Some extraneous and probably undocumented visual effect caused by the heart lad pet thing
            if (ladHearts > 0 && !npc.loveStruck && !Main.dedServ)
            {
                if (Main.rand.NextBool(5))
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(10f, 1f, 1f, 0.66f);
                    int heart = Gore.NewGore(npc.GetSource_FromThis(), npc.position + new Vector2(Main.rand.Next(npc.width + 1), Main.rand.Next(npc.height + 1)), velocity * Main.rand.Next(3, 6) * 0.33f, 331, Main.rand.Next(40, 121) * 0.01f);
                    Main.gore[heart].sticky = false;
                    Main.gore[heart].velocity *= 0.4f;
                    Main.gore[heart].velocity.Y -= 0.6f;
                }
            }

            // Vanilla debuff coloring effects + Hunter Potion. This allows GetAlpha (often used in PreDraw) to get vanilla debuff colors
            drawColor = npc.GetNPCColorTintedByBuffs(drawColor);

            // Calamity debuff coloring effects
            // These are in order of precedence because they override each other.
            if (glacialState)
                drawColor = Color.Cyan;

            else if (auricRebuke)
            {
                int scaleFactor = (int)(Utils.Remap(npc.width, 30, 400, 5, 15, true));
                drawColor = Main.rand.NextBool(scaleFactor) ? Color.Lerp(Color.DarkBlue, Color.White, Utils.Remap(npc.width, 30, 400, 0.4f, 0.7f, true)) : Color.White;
            }
            else if (vermillionFlux)
            {
                int scaleFactor = (int)(Utils.Remap(npc.width, 30, 400, 5, 15, true));
                drawColor = Main.rand.NextBool(scaleFactor) ? Color.Lerp(Color.DarkRed, Color.White, Utils.Remap(npc.width, 30, 400, 0, 0.7f, true)) : Color.White;
            }
            else if (electrified)
            {
                int scaleFactor = (int)(Utils.Remap(npc.width, 30, 400, 5, 15, true));
                drawColor = Main.rand.NextBool(scaleFactor) ? Color.Lerp(Color.SlateGray, Color.White, Utils.Remap(npc.width, 30, 400, 0, 0.7f, true)) : Color.White;
            }

            else if (absorberAffliction)
                drawColor = Color.DarkSeaGreen;

            else if (markedForDeath || vaporfied)
                drawColor = Color.Fuchsia;

            else if (pearlAura)
                drawColor = new Color(185, 185, 255);

            else if (timeDistortion || galvanicCorrosion)
                drawColor = Color.Aquamarine;
        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            // Don't make this affect the bestiary, that's goofy
            if (npc.IsABestiaryIconDummy)
                return null;

            if (Main.LocalPlayer.Calamity().trippy || (npc.type == NPCID.KingSlime && Main.zenithWorld))
                return new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, Main.DiscoR);

            if (npc.type == NPCID.QueenBee && Main.zenithWorld)
            {
                if (npc.life / (float)npc.lifeMax < 0.5f)
                    return new Color(0, 255, 0, 255 - npc.alpha);
                else
                    return new Color(255, 0, 0, 255 - npc.alpha);
            }

            if (npc.HasBuff<Enraged>())
                return new Color(200, 50, 50, 255 - npc.alpha);

            if (npc.type == NPCID.VileSpit || npc.type == NPCID.VileSpitEaterOfWorlds)
                return new Color(150, 200, 0, npc.alpha);

            if (npc.type == NPCID.AncientDoom || npc.type == NPCID.QueenSlimeMinionBlue || npc.type == NPCID.QueenSlimeMinionPink || npc.type == NPCID.QueenSlimeMinionPurple)
                return new Color(255, 255, 255, npc.alpha);

            return null;
        }


        //TODO - Make this a part of DebuffData
        public static List<(string, Predicate<NPC>)> moddedDebuffTextureList = new List<(string, Predicate<NPC>)>
        {
            // All Calamity DoTs in alphabetical order
            ("CalamityMod/Buffs/DamageOverTime/AstralInfectionDebuff", NPC => NPC.Calamity().astralInfection),
            ("CalamityMod/Buffs/DamageOverTime/AuricRebuke", NPC => NPC.Calamity().auricRebuke),
            ("CalamityMod/Buffs/DamageOverTime/BanishingFire", NPC => NPC.Calamity().banishingFire),
            ("CalamityMod/Buffs/DamageOverTime/BrainRot", NPC => NPC.Calamity().brainRot),
            ("CalamityMod/Buffs/DamageOverTime/BrimstoneFlames", NPC => NPC.Calamity().brimstoneFlames),
            ("CalamityMod/Buffs/DamageOverTime/DemonicFlames", NPC => NPC.Calamity().demonicFlames),
            ("CalamityMod/Buffs/DamageOverTime/BurningBlood", NPC => NPC.Calamity().burningBlood),
            ("CalamityMod/Buffs/DamageOverTime/CrushDepth", NPC => NPC.Calamity().crushDepth),
            ("CalamityMod/Buffs/DamageOverTime/Dragonfire", NPC => NPC.Calamity().dragonFire),
            ("CalamityMod/Buffs/DamageOverTime/ElementalMix", NPC => NPC.Calamity().elementalMix),
            ("CalamityMod/Buffs/DamageOverTime/GodSlayerInferno", NPC => NPC.Calamity().godSlayerInferno),
            ("CalamityMod/Buffs/DamageOverTime/HadopelagicPressure", NPC => NPC.Calamity().hadopelagicPressure),
            ("CalamityMod/Buffs/DamageOverTime/HolyFlames", NPC => NPC.Calamity().holyFlames),
            ("CalamityMod/Buffs/DamageOverTime/Laceration", NPC => NPC.Calamity().laceration),
            ("CalamityMod/Buffs/DamageOverTime/HeavyBleeding", NPC => NPC.Calamity().heavyBleeding),
            ("CalamityMod/Buffs/DamageOverTime/ManaBurn", NPC => NPC.Calamity().manaBurn > 0),
            ("CalamityMod/Buffs/DamageOverTime/MiracleBlight", NPC => NPC.Calamity().miracleBlight),
            ("CalamityMod/Buffs/DamageOverTime/Nightwither", NPC => NPC.Calamity().nightwither),
            ("CalamityMod/Buffs/DamageOverTime/Plague", NPC => NPC.Calamity().plague),
            ("CalamityMod/Buffs/DamageOverTime/RiptideDebuff", NPC => NPC.Calamity().riptide),
            ("CalamityMod/Buffs/DamageOverTime/SagePoison", NPC => NPC.Calamity().sagePoison),
            ("CalamityMod/Buffs/DamageOverTime/ShellfishClaps", NPC => NPC.Calamity().shellfishStaffDebuff),
            ("CalamityMod/Buffs/DamageOverTime/Shred", NPC => NPC.Calamity().somaShredStacks > 0),
            ("CalamityMod/Buffs/DamageOverTime/SnapClamDebuff", NPC => NPC.Calamity().snapClamDebuff),
            ("CalamityMod/Buffs/DamageOverTime/StaticDischarge", NPC => NPC.Calamity().staticDischarge),
            ("CalamityMod/Buffs/DamageOverTime/SulphuricPoisoning", NPC => NPC.Calamity().sulphurPoison),
            ("CalamityMod/Buffs/DamageOverTime/TrueVulnerabilityHex", NPC => NPC.Calamity().trueVulnerabilityHex),
            ("CalamityMod/Buffs/DamageOverTime/Vaporfied", NPC => NPC.Calamity().vaporfied),
            ("CalamityMod/Buffs/DamageOverTime/VermillionFlux", NPC => NPC.Calamity().vermillionFlux),
            ("CalamityMod/Buffs/DamageOverTime/Voidfrost", NPC => NPC.Calamity().voidfrost),
            ("CalamityMod/Buffs/DamageOverTime/VulnerabilityHex", NPC => NPC.Calamity().vulnerabilityHex),

            // All other important Calamity debuffs, in alphabetical order
            ("CalamityMod/Buffs/StatDebuffs/AbsorberAffliction", NPC => NPC.Calamity().absorberAffliction),
            ("CalamityMod/Buffs/StatDebuffs/ArmorCrunch", NPC => NPC.Calamity().armorCrunch),
            ("CalamityMod/Buffs/StatDebuffs/Crumbling", NPC => NPC.Calamity().crumble),
            ("CalamityMod/Buffs/StatDebuffs/Eutrophication", NPC => NPC.Calamity().eutrophication),
            ("CalamityMod/Buffs/StatDebuffs/GalvanicCorrosion", NPC => NPC.Calamity().galvanicCorrosion),
            ("CalamityMod/Buffs/StatDebuffs/GlacialState", NPC => NPC.Calamity().glacialState),
            ("CalamityMod/Buffs/StatDebuffs/Irradiated", NPC => NPC.Calamity().irradiated),
            ("CalamityMod/Buffs/StatDebuffs/MarkedforDeath", NPC => NPC.Calamity().markedForDeath),
            ("CalamityMod/Buffs/StatDebuffs/PearlAura", NPC => NPC.Calamity().pearlAura),
            ("CalamityMod/Buffs/StatDebuffs/ProfanedWeakness", NPC => NPC.Calamity().relicOfResilienceWeakness),
            ("CalamityMod/Buffs/StatDebuffs/TemporalSadness", NPC => NPC.Calamity().temporalSadness),
            ("CalamityMod/Buffs/StatDebuffs/TimeDistortion", NPC => NPC.Calamity().timeDistortion),
            ("CalamityMod/Buffs/StatDebuffs/WhisperingDeath", NPC => NPC.Calamity().whisperingDeath),
            ("CalamityMod/Buffs/StatDebuffs/WitherDebuff", NPC => NPC.Calamity().wither),
        };

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // This is used so that NPCs with specific PreDraws can still have Odd Mushroom clone drawing.
            // If Odd Mushroom clone drawing is done manually due to using a different texture, just return false instead of setting this.
            bool shouldDrawBool = true;

            if (npc.IsABestiaryIconDummy)
            {
                switch (npc.netID)
                {
                    case NPCID.DiggerHead:
                    case NPCID.GiantWormHead:
                    case NPCID.EaterofWorldsHead:
                    case NPCID.WyvernHead:
                    case NPCID.StardustWormHead:
                    case NPCID.SolarCrawltipedeHead:
                    case NPCID.CultistDragonHead:
                    case NPCID.TheDestroyer:
                    case NPCID.LeechHead:
                    case NPCID.DevourerHead:
                    case NPCID.TombCrawlerHead:
                    case NPCID.DuneSplicerHead:
                    case NPCID.BloodEelHead:
                    case NPCID.BoneSerpentHead:
                    case NPCID.SeekerHead:
                        return DrawVanillaBestiaryWorms(spriteBatch, npc, drawColor);
                }
            }
            if (npc.type != NPCID.BrainofCthulhu && (npc.type != NPCID.DukeFishron || npc.ai[0] <= 9f) && npc.active)
            {
                if (CalamityClientConfig.Instance.DebuffDisplay && (npc.boss || BossHealthBarManager.MinibossHPBarList.Contains(npc.type) || BossHealthBarManager.OneToMany.ContainsKey(npc.type) || CalamityNPCSets.ForceDrawDebuffDisplay[npc.type]))
                {
                    List<Texture2D> currentDebuffs = new List<Texture2D>() { };

                    for (int b = 0; b < moddedDebuffTextureList.Count(); b++)
                    {
                        if (moddedDebuffTextureList[b].Item2.Invoke(npc))
                        {
                            currentDebuffs.Add(Request<Texture2D>(moddedDebuffTextureList[b].Item1).Value);
                        }
                    }
                    // Vanilla damage over time debuffs
                    if (electrified)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Electrified].Value);
                    if (npc.onFire)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.OnFire].Value);
                    if (npc.poisoned)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Poisoned].Value);
                    if (npc.onFire2)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.CursedInferno].Value);
                    if (npc.onFrostBurn)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Frostburn].Value);
                    if (npc.venom)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Venom].Value);
                    if (npc.shadowFlame)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.ShadowFlame].Value);
                    if (npc.oiled)
                        currentDebuffs.Add(Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/Oiled").Value);
                    if (npc.javelined)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.BoneJavelin].Value);
                    if (npc.daybreak)
                        currentDebuffs.Add(Request<Texture2D>("CalamityMod/Buffs/DamageOverTime/Daybroken").Value);
                    if (npc.celled)
                        currentDebuffs.Add(Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/Celled").Value);
                    if (npc.dryadBane)
                        currentDebuffs.Add(Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/DryadsBane").Value);
                    if (npc.dryadWard)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.DryadsWard].Value);
                    if (npc.soulDrain && npc.realLife == -1)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.SoulDrain].Value);
                    if (npc.onFire3) // Hellfire
                        currentDebuffs.Add(Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/Hellfire").Value);
                    if (npc.onFrostBurn2) // Frostbite
                        currentDebuffs.Add(Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/Frostbite").Value);
                    if (npc.tentacleSpiked)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.TentacleSpike].Value);

                    // Vanilla stat debuffs
                    if (npc.confused)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Confused].Value);
                    if (npc.ichor)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Ichor].Value);
                    if (webbed)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Webbed].Value);
                    if (npc.midas)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Midas].Value);
                    if (npc.loveStruck)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Lovestruck].Value);
                    if (npc.stinky)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Stinky].Value);
                    if (npc.betsysCurse)
                        currentDebuffs.Add(Request<Texture2D>("CalamityMod/ExtraTextures/VanillaBuffs/BetsysCurse").Value);
                    if (npc.dripping)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Wet].Value);
                    if (npc.drippingSlime)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.Slimed].Value);
                    if (npc.drippingSparkleSlime)
                        currentDebuffs.Add(TextureAssets.Buff[BuffID.GelBalloonBuff].Value);

                    // Total amount of elements in the buff list
                    int buffTextureListLength = currentDebuffs.Count();

                    // Total length of a single row in the buff display
                    int totalLength = buffTextureListLength * 14;

                    // Max amount of buffs per row
                    int buffDisplayRowLimit = 5;

                    // The maximum length of a single row in the buff display
                    // Limited to 80 units, because every buff drawn here is half the size of a normal buff, 16 x 16, 16 * 5 = 80 units
                    float drawPosX = totalLength >= 80f ? 40f : (float)(totalLength / 2);

                    // The height of a single frame of the npc
                    float npcHeight = (npc.height * npc.scale)/2;//(float)(TextureAssets.Npc[npc.type].Value.Height / Main.npcFrameCount[npc.type] / 2) * npc.scale;

                    // Offset the debuff display based on the npc's graphical offset, and 16 units, to create some space between the sprite and the display
                    float drawPosY = npcHeight + npc.gfxOffY + 32f;

                    // Iterate through the buff texture list
                    for (int i = 0; i < currentDebuffs.Count; i++)
                    {
                        // Reset the X position of the display every 5th and non-zero iteration, otherwise decrease the X draw position by 16 units
                        if (i != 0)
                        {
                            if (i % buffDisplayRowLimit == 0)
                                drawPosX = 40f;
                            else
                                drawPosX -= 14f;
                        }

                        // Offset the Y position every row after 5 iterations to limit each displayed row to 5 debuffs
                        float additionalYOffset = 14f * (float)Math.Floor(i * 0.2);

                        // Draw the display
                        var tex = currentDebuffs[i];
                        spriteBatch.Draw(tex, npc.Center - screenPos - new Vector2(drawPosX, drawPosY + additionalYOffset), null, Color.White, 0f, default, 0.5f, SpriteEffects.None, 0f);

                        // Shred stack display
                        if (currentDebuffs[i] == TextureAssets.Buff[BuffType<Shred>()].Value)
                            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, somaShredStacks.ToString(), npc.Center - screenPos - new Vector2(drawPosX, drawPosY + additionalYOffset) + Vector2.One * 4f, Color.Gold, 0f, Vector2.Zero, Vector2.One * Main.UIScale * 0.8f);
                    }


                    // Draw summon tag display. TODO: make it use custom textures provided by SummonTag.
                    int yOffset = 0;
                    for (int i = NPC.maxBuffs - 1; i >= 0; i--)
                    {
                        if (npc.buffTime[i] > 0)
                        {
                            if (CalamityBuffSets.SummonTagDebuff.TryGetValue(npc.buffType[i], out SummonTag tag))
                            {
                                // Fetch the item and its frames
                                var tex = TextureAssets.Item[tag.TagItem].Value;
                                Rectangle frame = (Main.itemAnimations[tag.TagItem] == null) ? tex.Frame() : Main.itemAnimations[tag.TagItem].GetFrame(tex);
                                if (tag.TagTexture != null)
                                {
                                    tex = tag.TagTexture.Value;
                                    frame = tex.Frame();
                                }

                                // Draw it accordingly
                                // This is drawn below the NPC as opposed to above to differentiate from regular debuffs
                                Vector2 drawPos = npc.Center - screenPos + Vector2.UnitY * (drawPosY + frame.Height * 0.5f + yOffset);
                                spriteBatch.Draw(tex, drawPos, frame, Color.White, 0f, frame.Size() * 0.5f, 0.75f, SpriteEffects.None, 0f);
                                yOffset += frame.Height + 4;
                            }
                        }
                    }
                }
            }

            TownNPCAlertSystem(npc, Mod, spriteBatch);

            if (CalamityWorld.revenge || BossRushEvent.BossRushActive)
            {
                if (CalamityNPCTypeSets.Destroyer.Contains(npc.type))
                    shouldDrawBool = false;

                // Allows correct frames to draw in Rev+ phases
                // GFB can rot for all I care
                if (npc.type == NPCID.SkeletronPrime && !NPC.IsMechQueenUp)
                {
                    int frameHeight = TextureAssets.Npc[npc.type].Value.Height / Main.npcFrameCount[npc.type];
                    if (npc.ai[1] == 0f || npc.ai[1] == 4f)
                    {
                        newAI[2] += 1f;
                        if (newAI[2] >= 12f)
                        {
                            newAI[2] = 0f;
                            newAI[3] += frameHeight;

                            if (newAI[3] / frameHeight >= 2f)
                                newAI[3] = 0f;
                        }
                    }

                    // Spinning probe spawn or fly over phase
                    else if (npc.ai[1] == 5f || npc.ai[1] == 6f)
                    {
                        newAI[2] = 0f;
                        newAI[3] = frameHeight;
                    }

                    // Spinning phase
                    else
                    {
                        newAI[2] = 0f;
                        newAI[3] = frameHeight * 2;
                    }

                    npc.frame.Y = (int)newAI[3];
                }

                if (npc.type == NPCID.GolemHeadFree)
                {
                    // Draw the head as usual.
                    Texture2D golemHeadTexture = TextureAssets.Npc[npc.type].Value;
                    Vector2 headDrawPosition = npc.Center - screenPos;
                    spriteBatch.Draw(golemHeadTexture, headDrawPosition, npc.frame, npc.GetAlpha(drawColor), 0f, npc.frame.Size() * 0.5f, npc.scale, SpriteEffects.None, 0f);

                    // Draw the eyes. The way vanilla handles this is hardcoded bullshit that cannot handle different hitboxes and thus requires rewriting.
                    Color eyeColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 0);
                    Vector2 eyesDrawPosition = headDrawPosition - npc.scale * new Vector2(1f, 12f);
                    Rectangle eyesFrame = new Rectangle(0, 0, TextureAssets.Golem[1].Value.Width, TextureAssets.Golem[1].Value.Height / 2);
                    spriteBatch.Draw(TextureAssets.Golem[1].Value, eyesDrawPosition, eyesFrame, eyeColor, 0f, eyesFrame.Size() * 0.5f, npc.scale, SpriteEffects.None, 0f);

                    // Draw the glowmasks.
                    int frameCounter = (int)npc.frameCounter / 4;
                    Rectangle frame = TextureAssets.Extra[ExtrasID.GolemLights4].Value.Frame(1, 8);
                    frame.Y += frame.Height * 2 * frameCounter + npc.frame.Y;
                    Rectangle glowFrame = frame;
                    spriteBatch.Draw(TextureAssets.Extra[ExtrasID.GolemLights4].Value, eyesDrawPosition, glowFrame, eyeColor, 0f, glowFrame.Size() * 0.5f, npc.scale, SpriteEffects.None, 0f);
                    frame = npc.frame;
                    Rectangle glowFrame2 = frame;
                    spriteBatch.Draw(TextureAssets.Extra[ExtrasID.GolemLights5].Value, eyesDrawPosition, glowFrame2, eyeColor, 0f, glowFrame2.Size() * 0.5f, npc.scale, SpriteEffects.None, 0f);

                    // Draw the sparkle telegraphs for the laser spread attack if applicable.
                    if (npc.ai[0] == 3f && npc.ai[1] <= 60f)
                    {
                        spriteBatch.SetBlendState(BlendState.Additive);
                        for (int i = -1; i <= 1; i += 2)
                        {
                            Texture2D sparkle = Request<Texture2D>("CalamityMod/Particles/Sparkle2").Value;
                            Vector2 sparkleDraw = headDrawPosition + new Vector2(14f * i, -15f) * npc.scale;
                            Color drawFade = Color.Yellow * Utils.GetLerpValue(0, 30, 60f - npc.ai[1], true);
                            spriteBatch.Draw(sparkle, sparkleDraw, null, drawFade, MathHelper.Pi * 0.02f * npc.ai[1] * i, sparkle.Size() / 2f, 1.25f * npc.scale, SpriteEffects.None, 0f);
                        }
                        spriteBatch.SetBlendState(BlendState.AlphaBlend);
                    }
                    shouldDrawBool = false;
                }
            }

            if (npc.type == NPCID.Corruptor || npc.type == NPCID.BloodSquid || (npc.type == NPCID.HornetHoney && npc.ai[3] == 1f))
            {
                Texture2D texture = TextureAssets.Npc[npc.type].Value;

                SpriteEffects spriteEffects = SpriteEffects.None;
                if (npc.spriteDirection == -1)
                    spriteEffects = SpriteEffects.FlipHorizontally;

                Main.spriteBatch.Draw(texture, npc.Center - screenPos + new Vector2(0f, npc.gfxOffY), npc.frame, npc.GetAlpha(drawColor), npc.rotation, npc.frame.Size() / 2, npc.scale, spriteEffects, 0f);

                shouldDrawBool = false;
            }

            // VHex, Mana Burn and Miracle Blight visuals do not appear if Odd Mushroom is in use for sanity reasons
            if (!Main.LocalPlayer.Calamity().trippy)
            {
                if (npc.Calamity().vulnerabilityHex || npc.Calamity().trueVulnerabilityHex)
                {
                    float compactness = npc.width * 0.6f;
                    if (compactness < 10f)
                        compactness = 10f;
                    float power = npc.height / 100f;
                    if (power > 2.75f)
                        power = 2.75f;
                    var color = Color.Red;
                    if (VulnerabilityHexFireDrawer is null || VulnerabilityHexFireDrawer.LocalTimer >= VulnerabilityHexFireDrawer.SetLifetime)
                        VulnerabilityHexFireDrawer = new FireParticleSet(npc.Calamity().trueVulnerabilityHex ? npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<TrueVulnerabilityHex>())] : npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<VulnerabilityHex>())], 1, Color.Red * 1.25f, Color.Red, compactness, power);
                    else
                        VulnerabilityHexFireDrawer.DrawSet(npc.Bottom - Vector2.UnitY * (12f - npc.gfxOffY));
                }
                else
                    VulnerabilityHexFireDrawer = null;

                // Mana Burn effect is just vhex but blue
                if (npc.Calamity().manaBurn > 0)
                {
                    float compactness = npc.width * 0.6f;
                    if (compactness < 10f)
                        compactness = 10f;
                    float power = npc.height / 100f;
                    if (power > 2.75f)
                        power = 2.75f;
                    var color = Color.Blue;
                    if (ManaBurnFireDrawer is null || ManaBurnFireDrawer.LocalTimer >= ManaBurnFireDrawer.SetLifetime)
                        ManaBurnFireDrawer = new FireParticleSet(60, 1, color * 1.25f, color, compactness, power);
                    else
                        ManaBurnFireDrawer.DrawSet(npc.Bottom - Vector2.UnitY * (12f - npc.gfxOffY));
                }
                else
                    ManaBurnFireDrawer = null;

                // Only draw the NPC if told to by the miracle blight drawer.
                if (MiracleBlightRenderer.ValidToDraw(npc))
                    return MiracleBlightRenderer.ActuallyDoPreDraw;

                // Only draw DoG's death animation when told to by the renderer.
                if (DoGDeathAnimationRenderer.ValidToDraw(npc))
                    return DoGDeathAnimationRenderer.ActuallyDoPreDraw;
            }

            if (Main.zenithWorld)
            {
                if (NPC.AnyNPCs(NPCType<CeaselessVoid.CeaselessVoid>()))
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                    var midnightShader = GameShaders.Armor.GetShaderFromItemId(ItemID.MidnightRainbowDye);
                    midnightShader.Apply();
                }
            }

            return shouldDrawBool;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Energy shield
            if (npc.type == NPCID.CultistBoss || npc.type == NPCID.CultistBossClone)
            {
                spriteBatch.EnterShaderRegion();

                float intensity = newAI[1] / 35f;

                float lifeRatio = npc.type == NPCID.CultistBoss ? (npc.life / (float)npc.lifeMax) : (Main.npc[(int)npc.ai[3]].life / (float)Main.npc[(int)npc.ai[3]].lifeMax);

                float flickerPower = 0f;
                if (lifeRatio < 0.85f)
                    flickerPower += 0.1f;
                if (lifeRatio < 0.7f)
                    flickerPower += 0.1f;
                if (lifeRatio < 0.55f)
                    flickerPower += 0.1f;
                if (lifeRatio < 0.4f)
                    flickerPower += 0.1f;
                if (lifeRatio < 0.25f)
                    flickerPower += 0.1f;
                if (lifeRatio < 0.1f)
                    flickerPower += 0.1f;

                float opacity = 1f;
                opacity *= MathHelper.Lerp(MathHelper.Max(1f - flickerPower, 0.56f), 1f, (float)Math.Pow(Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.Lerp(3f, 5f, flickerPower)) * 0.5 + 0.5, 24D));

                // Dampen the opacity and intensity slightly, to allow Cultist to be more easily visible inside of the forcefield.
                // Dampen the opacity and intensity a bit more for the Clones.
                float intensityAndOpacityMult = npc.type == NPCID.CultistBossClone ? 0.9f : 1f;
                intensity *= intensityAndOpacityMult;
                opacity *= intensityAndOpacityMult;

                Texture2D forcefieldTexture = SupremeCalamitas.SupremeCalamitas.ForcefieldTexture.Value;

                if (npc.type == NPCID.CultistBoss)
                    GameShaders.Misc["CalamityMod:SupremeShield"].SetShaderTexture(Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
                else
                    GameShaders.Misc["CalamityMod:SupremeShield"].UseImage1("Images/Misc/noise");

                float colorTransitionAmt = (float)Math.Pow((double)(1f - lifeRatio), 2D);
                Color forcefieldColor = Color.Lerp(Color.MediumSpringGreen, Color.Black, colorTransitionAmt);
                Color secondaryForcefieldColor = Color.Lerp(Color.Cyan, Color.Blue, colorTransitionAmt);

                forcefieldColor *= opacity;
                secondaryForcefieldColor *= opacity;

                GameShaders.Misc["CalamityMod:SupremeShield"].UseSecondaryColor(secondaryForcefieldColor);
                GameShaders.Misc["CalamityMod:SupremeShield"].UseColor(forcefieldColor);
                GameShaders.Misc["CalamityMod:SupremeShield"].UseSaturation(1);
                GameShaders.Misc["CalamityMod:SupremeShield"].UseOpacity(0.65f);
                GameShaders.Misc["CalamityMod:SupremeShield"].Apply();

                // Actual Cultist has a bigger shield than the Clones.
                float shieldScale = npc.type == NPCID.CultistBossClone ? 1.65f : MathHelper.Lerp(1.65f, 3f, (float)Math.Pow((double)lifeRatio, 2D));
                spriteBatch.Draw(forcefieldTexture, npc.Center - Main.screenPosition, null, Color.White * opacity, 0f, forcefieldTexture.Size() * 0.5f, shieldScale, SpriteEffects.None, 0f);

                spriteBatch.ExitShaderRegion();
            }

            // Destroyer drawing and laser telegraphs
            else if (CalamityNPCTypeSets.Destroyer.Contains(npc.type) && !npc.IsABestiaryIconDummy)
            {
                Texture2D npcTexture = TextureAssets.Npc[npc.type].Value;
                int frameHeight = npcTexture.Height / Main.npcFrameCount[npc.type];

                Vector2 halfSize = npc.frame.Size() / 2;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (npc.spriteDirection == 1)
                    spriteEffects = SpriteEffects.FlipHorizontally;

                Color segmentDrawColor = npc.GetAlpha(drawColor);

                // Check if Destroyer is behind tiles and, if so, how much of the segment is behind tiles and adjust color accordingly
                int x = (int)((npc.position.X - 8f) / 16f);
                int x2 = (int)((npc.position.X + npc.width + 8f) / 16f);
                int y = (int)((npc.position.Y - 8f) / 16f);
                int y2 = (int)((npc.position.Y + npc.height + 8f) / 16f);
                for (int l = x; l <= x2; l++)
                {
                    for (int m = y; m <= y2; m++)
                    {
                        if (Lighting.Brightness(l, m) == 0f)
                            segmentDrawColor = Color.Black;
                    }
                }

                // Draw segments
                spriteBatch.Draw(npcTexture, npc.Center - screenPos + new Vector2(0, npc.gfxOffY),
                    npc.frame, segmentDrawColor, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);

                // Draw lights
                if (npc.ai[2] == 0f && segmentDrawColor != Color.Black)
                {
                    // This life ratio is fine now because all Destroyer segments update to have the same amount of life every frame
                    float destroyerLifeRatio = npc.life / (float)npc.lifeMax;

                    // Phases
                    bool phase4 = destroyerLifeRatio < (death ? 0.4f : 0.25f);
                    bool phase5 = destroyerLifeRatio < (death ? 0.2f : 0.1f);

                    // Spawn DR check
                    bool hasSpawnDR = newAI[1] < DestroyerAI.DRIncreaseTime && newAI[1] > 60f;

                    // Gradual color transition from ground to flight and vice versa
                    // 0f = Red, 1f = Purple
                    float phaseTransitionColorAmount = (hasSpawnDR || phase5) ? 1f : 0f;
                    if (!hasSpawnDR && !phase5)
                    {
                        if (newAI[3] >= DestroyerAI.GroundTelegraphStartGateValue)
                            phaseTransitionColorAmount = MathHelper.Clamp(1f - (newAI[3] - DestroyerAI.GroundTelegraphStartGateValue) / DestroyerAI.PhaseTransitionTelegraphTime, 0f, 1f);
                        else if (newAI[3] >= DestroyerAI.FlightTelegraphStartGateValue)
                            phaseTransitionColorAmount = MathHelper.Clamp((newAI[3] - DestroyerAI.FlightTelegraphStartGateValue) / DestroyerAI.PhaseTransitionTelegraphTime, 0f, 1f);
                    }

                    // Light colors
                    int alpha = 192;
                    Color groundColor = new Color(150, 0, 0, alpha);
                    Color flightColor = revenge ? new Color(0, 0, 150, alpha) : groundColor;
                    Color segmentColor = Color.Lerp(groundColor, flightColor, phaseTransitionColorAmount);
                    Color telegraphColor_Red = new Color(255, 125, 125, alpha);
                    Color telegraphColor_Green = new Color(125, 255, 125, alpha);
                    Color telegraphColor_Cyan = new Color(0, 255, 255, alpha);
                    Color telegraphColor = telegraphColor_Red;

                    // Telegraph for the laser breath and body lasers
                    float telegraphProgress = 0f;
                    if (destroyerLaserColor != -1)
                    {
                        if (npc.type == NPCID.TheDestroyer && death)
                        {
                            float telegraphGateValue = DestroyerAI.DeathModeLaserBreathGateValue - DestroyerAI.LaserTelegraphTime;
                            if (newAI[0] > telegraphGateValue)
                            {
                                switch (destroyerLaserColor)
                                {
                                    default:
                                    case 0:
                                        break;

                                    case 1:
                                        telegraphColor = telegraphColor_Green;
                                        break;

                                    case 2:
                                        telegraphColor = telegraphColor_Cyan;
                                        break;
                                }
                                telegraphProgress = MathHelper.Clamp((newAI[0] - telegraphGateValue) / DestroyerAI.LaserTelegraphTime, 0f, 1f);
                            }
                        }
                        else if (npc.type == NPCID.TheDestroyerBody && revenge)
                        {
                            float shootProjectileTime = death ? (phase5 ? 180f : phase4 ? 270f : 360f) : 450f;
                            float telegraphGateValue = shootProjectileTime - DestroyerAI.LaserTelegraphTime;
                            if (newAI[0] > telegraphGateValue)
                            {
                                switch (destroyerLaserColor)
                                {
                                    default:
                                    case 0:
                                        break;

                                    case 1:
                                        telegraphColor = telegraphColor_Green;
                                        break;

                                    case 2:
                                        telegraphColor = telegraphColor_Cyan;
                                        break;
                                }
                                telegraphProgress = MathHelper.Clamp((newAI[0] - telegraphGateValue) / DestroyerAI.LaserTelegraphTime, 0f, 1f);
                            }
                        }
                    }

                    Texture2D glowTexture = CalamityClientConfig.Instance.EnableVanillaTextureEdits ? ExtraTextureRefs.DestroyerHeadGlowmask.Value : TextureAssets.Dest[0].Value;
                    switch (npc.type)
                    {
                        default:
                        case NPCID.TheDestroyer:
                            break;

                        case NPCID.TheDestroyerBody:
                            glowTexture = CalamityClientConfig.Instance.EnableVanillaTextureEdits ? ExtraTextureRefs.DestroyerBodyGlowmask.Value : TextureAssets.Dest[1].Value;
                            break;

                        case NPCID.TheDestroyerTail:
                            glowTexture = CalamityClientConfig.Instance.EnableVanillaTextureEdits ? ExtraTextureRefs.DestroyerTailGlowmask.Value : TextureAssets.Dest[2].Value;
                            break;
                    }

                    float alphaMultiplier = 1f - npc.alpha / 255f;
                    spriteBatch.Draw(glowTexture, npc.Center - screenPos + new Vector2(0, npc.gfxOffY), npc.frame, Color.Lerp(segmentColor, telegraphColor, telegraphProgress) * alphaMultiplier, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                }
            }

            // Laser telegraph
            else if (npc.type == NPCID.Probe)
            {
                float eyeTelegraphGateValue = (NPC.IsMechQueenUp ? DestroyerAI.ProbeLaserGateValue_Mechdusa : revenge ? DestroyerAI.ProbeLaserGateValue_Rev : DestroyerAI.ProbeLaserGateValue) - DestroyerAI.ProbeLaserTelegraphTime;
                Texture2D glowTexture = Request<Texture2D>("CalamityMod/Particles/Sparkle").Value;
                Vector2 halfSize = npc.frame.Size() / 2;

                Vector2 drawPosition = npc.Center - screenPos + Vector2.UnitX.RotatedBy(npc.rotation) * (npc.width * 0.45f * npc.spriteDirection) + Vector2.UnitY * npc.gfxOffY;
                float colorScale = MathHelper.Clamp((npc.localAI[0] - eyeTelegraphGateValue) / DestroyerAI.ProbeLaserTelegraphTime, 0f, 1f);
                Color drawColor2 = new Color(255, 100, 150, 192) * colorScale;
                spriteBatch.SetBlendState(BlendState.Additive);
                spriteBatch.Draw(glowTexture, drawPosition, npc.frame, drawColor2, npc.rotation, halfSize, npc.scale * 1.1f, SpriteEffects.None, 0f);
                spriteBatch.SetBlendState(BlendState.AlphaBlend);
            }

            if (revenge)
            {
                // Telegraph for charge and blood shots
                if (npc.type == NPCID.Creeper)
                {
                    if (NPC.crimsonBoss < 0)
                        return;

                    Vector2 halfSize = npc.frame.Size() / 2;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    if (npc.spriteDirection == 1)
                        spriteEffects = SpriteEffects.FlipHorizontally;

                    bool brainIsInPhase2 = Main.npc[NPC.crimsonBoss].ai[0] < 0f;
                    if (brainIsInPhase2)
                    {
                        Vector2 distanceFromBrain = npc.Center - Main.npc[NPC.crimsonBoss].Center;
                        Color currentColor = npc.GetAlpha(drawColor);
                        float opacity = (1f - Main.npc[NPC.crimsonBoss].life / (float)Main.npc[NPC.crimsonBoss].lifeMax) * 2f;
                        opacity *= opacity;
                        if (Main.getGoodWorld)
                            opacity = 1f;

                        opacity = MathHelper.Clamp(opacity, 0f, 1f);
                        currentColor.R = (byte)(currentColor.R * opacity);
                        currentColor.G = (byte)(currentColor.G * opacity);
                        currentColor.B = (byte)(currentColor.B * opacity);
                        currentColor.A = (byte)(currentColor.A * opacity);
                        int totalAfterimages = 4;
                        for (int i = 0; i < totalAfterimages; i++)
                        {
                            Vector2 position = npc.position;
                            float distanceFromTargetX = Math.Abs(npc.Center.X - Main.LocalPlayer.Center.X);
                            float distanceFromTargetY = Math.Abs(npc.Center.Y - Main.LocalPlayer.Center.Y);
                            if (i == 0 || i == 2)
                                position.X = Main.LocalPlayer.Center.X + distanceFromTargetX;
                            else
                                position.X = Main.LocalPlayer.Center.X - distanceFromTargetX;

                            position.X -= npc.width / 2;
                            if (i == 0 || i == 1)
                                position.Y = Main.LocalPlayer.Center.Y + distanceFromTargetY;
                            else
                                position.Y = Main.LocalPlayer.Center.Y - distanceFromTargetY;

                            position.Y -= npc.height / 2;

                            int width = TextureAssets.Npc[npc.type] is null ? 0 : TextureAssets.Npc[npc.type].Width();
                            int height = TextureAssets.Npc[npc.type] is null ? 0 : TextureAssets.Npc[npc.type].Height();
                            spriteBatch.Draw(TextureAssets.Npc[npc.type].Value, new Vector2(position.X - screenPos.X + (float)(npc.width / 2) - (float)width * npc.scale / 2f + halfSize.X * npc.scale, position.Y - screenPos.Y + (float)npc.height - (float)height * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + halfSize.Y * npc.scale + npc.gfxOffY), npc.frame, currentColor, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                        }
                    }

                    float beginTelegraphGateValue = BrainOfCthulhuAI.TimeBeforeCreeperAttack - BrainOfCthulhuAI.CreeperTelegraphTime;
                    if (npc.ai[1] > beginTelegraphGateValue || npc.ai[0] == 1f)
                    {
                        float colorScale = npc.ai[0] == 1f ? 1f : MathHelper.Clamp((npc.ai[1] - beginTelegraphGateValue) / BrainOfCthulhuAI.CreeperTelegraphTime, 0f, 1f);
                        Color drawColor2 = new Color(150, 30, 30, 0) * colorScale;
                        for (int i = 0; i < 2; i++)
                        {
                            spriteBatch.Draw(TextureAssets.Npc[npc.type].Value, npc.Center - screenPos + new Vector2(0, npc.gfxOffY), npc.frame,
                                drawColor2, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                        }
                    }
                }

                // Telegraph for charges
                else if (npc.type == NPCID.SkeletronHead)
                {
                    float beginTelegraphGateValue = SkeletronAI.ChargeGateValue - SkeletronAI.ChargeTelegraphTime;
                    if (npc.localAI[1] > beginTelegraphGateValue)
                    {
                        float colorScale = MathHelper.Clamp((npc.localAI[1] - beginTelegraphGateValue) / SkeletronAI.ChargeTelegraphTime, 0f, 1f);
                        Color drawColor2 = new Color(150, 150, 150, 0) * colorScale;
                        Vector2 halfSize = npc.frame.Size() / 2;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        if (npc.spriteDirection == 1)
                            spriteEffects = SpriteEffects.FlipHorizontally;

                        for (int i = 0; i < 2; i++)
                        {
                            spriteBatch.Draw(TextureAssets.Npc[npc.type].Value, npc.Center - screenPos + new Vector2(0, npc.gfxOffY), npc.frame,
                                drawColor2, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                        }
                    }
                }

                // Telegraph for slaps
                else if (npc.type == NPCID.SkeletronHand)
                {
                    float beginTelegraphGateValue = SkeletronAI.HandSlapGateValue - SkeletronAI.HandSlapTelegraphTime;
                    if (newAI[2] > beginTelegraphGateValue)
                    {
                        float colorScale = MathHelper.Clamp((newAI[2] - beginTelegraphGateValue) / SkeletronAI.HandSlapTelegraphTime, 0f, 1f);
                        Color drawColor2 = new Color(150, 150, 150, 0) * colorScale;
                        Vector2 halfSize = npc.frame.Size() / 2;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        if (npc.spriteDirection == 1)
                            spriteEffects = SpriteEffects.FlipHorizontally;

                        Vector2 glowOffset = Vector2.UnitY * 8f;
                        for (int i = 0; i < 2; i++)
                        {
                            spriteBatch.Draw(TextureAssets.Npc[npc.type].Value, npc.Center - screenPos + new Vector2(0, npc.gfxOffY) - glowOffset, npc.frame,
                                drawColor2, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                        }
                    }
                }

                // Laser telegraph
                else if (npc.type == NPCID.WallofFleshEye && Main.wofNPCIndex >= 0)
                {
                    bool enraged = npc.localAI[3] > 0f;
                    float eyeTelegraphGateValue = WallOfFleshAI.LaserShootGateValue - WallOfFleshAI.LaserShootTelegraphTime;
                    if (npc.localAI[1] > eyeTelegraphGateValue || npc.localAI[2] > 0f || enraged)
                    {
                        Texture2D glowTexture = CalamityClientConfig.Instance.EnableVanillaTextureEdits ? ExtraTextureRefs.WallOfFleshEyeGlowmask.Value : TextureAssets.Npc[npc.type].Value;
                        Vector2 halfSize = npc.frame.Size() / 2;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        if (npc.spriteDirection == 1)
                            spriteEffects = SpriteEffects.FlipHorizontally;

                        float colorScale = enraged ? MathHelper.Clamp(npc.localAI[3] / WallOfFleshAI.EnragedLaserFiringDuration, 0f, 1f) :
                            npc.localAI[2] > 0f ? 1f - ((npc.localAI[2] - 1f) / WallOfFleshAI.TotalLasersPerBarrage) :
                            MathHelper.Clamp((npc.localAI[1] - eyeTelegraphGateValue) / WallOfFleshAI.LaserShootTelegraphTime, 0f, 1f);

                        Color drawColor2 = new Color(100, 0, 200, 192) * colorScale;
                        for (int i = 0; i < 2; i++)
                        {
                            spriteBatch.Draw(glowTexture, npc.Center - screenPos + new Vector2(0, npc.gfxOffY), npc.frame,
                                drawColor2, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                        }
                    }
                }

                else if (npc.type == NPCID.Plantera)
                {
                    // Percent life remaining
                    float lifeRatio = npc.life / (float)npc.lifeMax;

                    Texture2D npcTexture = TextureAssets.Npc[npc.type].Value;

                    SpriteEffects spriteEffects = SpriteEffects.None;
                    if (npc.spriteDirection == 1)
                        spriteEffects = SpriteEffects.FlipHorizontally;

                    Color originalColor = npc.GetAlpha(drawColor);
                    Color newColor = new Color(100, 255, 100, 255);
                    Vector2 glowOffset = Vector2.UnitY * -4f;
                    Vector2 drawPosition = npc.Center - screenPos + new Vector2(0, npc.gfxOffY) + glowOffset;
                    Vector2 origin = npc.frame.Size() / 2;

                    bool phase2 = lifeRatio <= 0.5f;
                    if (!phase2)
                    {
                        float telegraphTimer = Math.Abs(npc.ai[1]);
                        bool startSeedGatlingSporeGasTelegraph = npc.ai[1] > PlanteraAI.SeedGatlingColorChangeGateValue;
                        bool endSeedGatlingSporeGasTelegraph = npc.ai[1] < -PlanteraAI.SeedGatlingDuration + PlanteraAI.SeedGatlingColorChangeDuration;
                        if (startSeedGatlingSporeGasTelegraph)
                        {
                            float telegraphScalar = MathHelper.Clamp((telegraphTimer - PlanteraAI.SeedGatlingColorChangeGateValue) / PlanteraAI.SeedGatlingColorChangeDuration, 0f, 1f);
                            Color telegraphColor = Color.Lerp(originalColor, newColor, telegraphScalar);
                            spriteBatch.Draw(npcTexture, drawPosition, npc.frame, telegraphColor, npc.rotation, origin, npc.scale, spriteEffects, 0f);
                        }

                        // -300 to -120
                        else if (endSeedGatlingSporeGasTelegraph)
                        {
                            float telegraphScalar = MathHelper.Clamp((telegraphTimer - (PlanteraAI.SeedGatlingDuration - PlanteraAI.SeedGatlingColorChangeDuration)) / PlanteraAI.SeedGatlingColorChangeDuration, 0f, 1f);
                            Color telegraphColor = Color.Lerp(originalColor, newColor, telegraphScalar);
                            spriteBatch.Draw(npcTexture, drawPosition, npc.frame, telegraphColor, npc.rotation, origin, npc.scale, spriteEffects, 0f);
                        }
                    }
                    else
                    {
                        float telegraphTimer = Math.Abs(npc.ai[3]);
                        bool startChargeTelegraph = npc.ai[3] > PlanteraAI.ChargeTelegraphColorChangeGateValue;
                        bool endChargeTelegraph = npc.ai[3] <= -2f;
                        if (startChargeTelegraph)
                        {
                            float telegraphScalar = MathHelper.Clamp((telegraphTimer - PlanteraAI.ChargeTelegraphColorChangeGateValue) / PlanteraAI.SeedGatlingColorChangeDuration, 0f, 1f);
                            Color telegraphColor = Color.Lerp(originalColor, newColor, telegraphScalar);
                            spriteBatch.Draw(npcTexture, drawPosition, npc.frame, telegraphColor, npc.rotation, origin, npc.scale, spriteEffects, 0f);
                        }

                        // -195 to -2
                        else if (endChargeTelegraph)
                        {
                            float telegraphScalar = MathHelper.Clamp((Math.Abs(PlanteraAI.StopChargeGateValue) - telegraphTimer) / Math.Abs(PlanteraAI.StopChargeGateValue), 0f, 1f);
                            Color telegraphColor = Color.Lerp(originalColor, newColor, telegraphScalar);

                            if (CalamityClientConfig.Instance.Afterimages)
                            {
                                int afterimageAmount = 10;
                                int afterImageIncrement = 2;
                                for (int j = 0; j < afterimageAmount; j += afterImageIncrement)
                                {
                                    Color afterimageColor = telegraphColor;
                                    afterimageColor = Color.Lerp(afterimageColor, originalColor, 0.5f);
                                    afterimageColor = npc.GetAlpha(afterimageColor);
                                    afterimageColor *= (afterimageAmount - j) / 15f;
                                    Vector2 afterimagePos = npc.oldPos[j] + new Vector2(npc.width, npc.height) / 2f - screenPos;
                                    afterimagePos -= new Vector2(npcTexture.Width, npcTexture.Height / Main.npcFrameCount[npc.type]) * npc.scale / 2f;
                                    afterimagePos += origin * npc.scale + new Vector2(0f, npc.gfxOffY) + glowOffset;
                                    spriteBatch.Draw(npcTexture, afterimagePos, npc.frame, afterimageColor, npc.rotation, origin, npc.scale, spriteEffects, 0f);
                                }
                            }

                            spriteBatch.Draw(npcTexture, drawPosition, npc.frame, telegraphColor, npc.rotation, origin, npc.scale, spriteEffects, 0f);
                        }
                    }
                }
            }
        }

        public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (CalamityWorld.death || BossRushEvent.BossRushActive)
            {
                if (npc.type == NPCID.Creeper)
                {
                    bool brainIsInPhase2 = Main.npc[NPC.crimsonBoss].ai[0] < 0f;
                    if (brainIsInPhase2)
                        return false;
                }
            }

            return null;
        }

        public static Color buffColor(Color newColor, float R, float G, float B, float A)
        {
            newColor.R = (byte)((float)newColor.R * R);
            newColor.G = (byte)((float)newColor.G * G);
            newColor.B = (byte)((float)newColor.B * B);
            newColor.A = (byte)((float)newColor.A * A);
            return newColor;
        }

        public static bool DrawVanillaBestiaryWorms(SpriteBatch spriteBatch, NPC npc, Color drawColor)
        {
            npc.Opacity = 1;
            int segments = 6;
            int spacing = 20;
            int bashLength = 0;
            float bashSpeed = 0f;
            int speed = 3;
            float rotation = 0.6f;
            Texture2D wyvernArm = TextureAssets.Npc[NPCID.WyvernLegs].Value;
            Texture2D wyvernBody = TextureAssets.Npc[NPCID.WyvernBody].Value;
            switch (npc.netID)
            {
                case NPCID.DiggerHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, segments, 24, 0.4f, Vector2.Zero, speed, 10, 10, 0.2f);
                case NPCID.GiantWormHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, 8, 14, 0.6f, new Vector2(20, 0), 4, 10, 6, 0.18f);
                case NPCID.EaterofWorldsHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, segments, 34, 0.2f, new Vector2(30, 0), speed, 10, 16, 0.24f);
                case NPCID.WyvernHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, [wyvernArm, wyvernBody, wyvernBody, wyvernBody], 4, 28, 0.1f, new Vector2(36, 0), speed, 6, 50, 0.3f, true);
                case NPCID.StardustWormHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, 8, 14, rotation, new Vector2(0, 10), 4, 10, 6, 0.18f);
                case NPCID.SolarCrawltipedeHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, segments, spacing, rotation, Vector2.Zero, 6, 10, 16, 0.22f);
                case NPCID.CultistDragonHead:
                    return DrawSpecialBestiaryWorm(spriteBatch, npc, drawColor);
                case NPCID.TheDestroyer:
                    return DrawSpecialBestiaryWorm(spriteBatch, npc, drawColor);
                case NPCID.LeechHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, 8, 14, 0.6f, new Vector2(20, 0), 4, 10, 6, 0.18f);
                case NPCID.DevourerHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, segments, spacing, rotation, Vector2.Zero, speed, 20, 10, 0.2f);
                case NPCID.TombCrawlerHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, 9, 14, rotation, Vector2.Zero, speed, 20, 6, 0.14f);
                case NPCID.DuneSplicerHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, segments, 28, 0.4f, Vector2.Zero, speed, 10, bashLength, bashSpeed);
                case NPCID.BloodEelHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, 6, 22, 0.1f, Vector2.Zero, speed, 6, 20, 0.2f, true);
                case NPCID.BoneSerpentHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, 9, 16, rotation, Vector2.Zero, speed, 10, 30, 0.4f);
                case NPCID.SeekerHead:
                    return CalamityUtils.DrawAnimatedBestiaryWorm(spriteBatch, npc, drawColor, TextureAssets.Npc[npc.type].Value, TextureAssets.Npc[npc.type + 1].Value, segments, spacing, rotation, Vector2.Zero, speed, 20, 10, 0.2f);
            }
            return true;
        }

        public static bool DrawSpecialBestiaryWorm(SpriteBatch spriteBatch, NPC npc, Color drawColor)
        {
            // This is solely for The Destroyer and the Phantasm Dragon due to having more than 1 frame each but only for specific segments
            bool dragon = npc.type == NPCID.CultistDragonHead;
            Texture2D headTexture = TextureAssets.Npc[npc.type].Value;
            float wormTimer = npc.Calamity().bestiaryWormTimer;
            // Dragon head has 3 frames, Destroyer has 1
            int frameAmt = dragon ? 3 : 1;
            npc.frame = TextureAssets.Npc[npc.type].Frame(1, frameAmt, 0, 0);
            Vector2 baseOffset = new Vector2(dragon ? 0 : 20, dragon ? 0 : 20);
            // Buffers the segment position and rotations
            float offset = -0.2f;
            float startX = baseOffset.X;
            float startY = baseOffset.Y;
            int segmentSpacing = dragon ? 32 : 38;
            int animationSpeed = 3;
            int range = 10;
            int headOffset = dragon ? 40 : 20;
            float headSpeedOffset = dragon ? 0.2f : 0.16f;
            float rotationStrength = 0.2f;
            // Draw the body segments
            for (int i = 4; i > 0; i--)
            {
                // The first segment is slightly closer to keep up with the head
                float bodyOffset = i == 1 ? i * segmentSpacing * 0.4f : i * segmentSpacing - segmentSpacing * 0.5f;

                // Second dragon segment uses the arm, rest use the normal body
                Texture2D toUse = i == 2 ? TextureAssets.Npc[NPCID.CultistDragonBody1].Value : TextureAssets.Npc[NPCID.CultistDragonBody2].Value;
                // If it's The Destroyer instead use his texture and increase the frame count to two
                if (!dragon)
                    toUse = TextureAssets.Npc[NPCID.TheDestroyerBody].Value;
                int bodyFrameAmt = dragon ? 1 : 2;
                spriteBatch.Draw(toUse, npc.position + new Vector2(startX + bodyOffset, MathF.Sin((wormTimer + offset * i) * animationSpeed) * range + startY), toUse.Frame(1, bodyFrameAmt, 0, 0), npc.GetAlpha(drawColor), npc.rotation - MathHelper.PiOver2 - MathF.Cos((wormTimer + offset * i) * animationSpeed) * MathHelper.PiOver4 * rotationStrength, new Vector2(toUse.Width * 0.5f, toUse.Height * 0.5f / bodyFrameAmt), npc.scale, SpriteEffects.FlipHorizontally, 0f);
            }
            // Draw the head
            spriteBatch.Draw(headTexture, npc.position + new Vector2(startX + headOffset, MathF.Sin((wormTimer - headSpeedOffset) * animationSpeed) * range + startY), npc.frame, npc.GetAlpha(drawColor), npc.rotation - MathHelper.PiOver2 - MathF.Cos((wormTimer - headSpeedOffset) * animationSpeed) * MathHelper.PiOver4 * rotationStrength, new Vector2(headTexture.Width * 0.5f, headTexture.Height / (float)frameAmt), npc.scale, SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
        #endregion

        #region Any Events
        public static bool AnyEvents(Player player, bool checkBloodMoon = false)
        {
            if (Main.invasionType > InvasionID.None && Main.invasionProgressNearInvasion)
                return true;
            if (player.PillarZone())
                return true;
            if (DD2Event.Ongoing && player.ZoneOldOneArmy)
                return true;
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (Main.eclipse || Main.pumpkinMoon || Main.snowMoon))
                return true;
            if (AcidRainEvent.AcidRainEventIsOngoing && player.InSulphur())
                return true;
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && Main.bloodMoon && checkBloodMoon)
                return true;
            return false;
        }
        #endregion

        #region Get Downed Boss Variable
        public static bool GetDownedBossVariable(int type)
        {
            switch (type)
            {
                case NPCID.KingSlime:
                    return NPC.downedSlimeKing;

                case NPCID.EyeofCthulhu:
                    return NPC.downedBoss1;

                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                case NPCID.BrainofCthulhu:
                case NPCID.Creeper:
                    return NPC.downedBoss2;

                case NPCID.QueenBee:
                    return NPC.downedQueenBee;

                case NPCID.SkeletronHead:
                    return NPC.downedBoss3;

                case NPCID.Deerclops:
                    return NPC.downedDeerclops;

                case NPCID.WallofFlesh:
                case NPCID.WallofFleshEye:
                    return Main.hardMode;

                case NPCID.QueenSlimeBoss:
                    return NPC.downedQueenSlime;

                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                    return NPC.downedMechBoss1;

                case NPCID.Spazmatism:
                case NPCID.Retinazer:
                    return NPC.downedMechBoss2;

                case NPCID.SkeletronPrime:
                    return NPC.downedMechBoss3;

                case NPCID.Plantera:
                    return NPC.downedPlantBoss;

                case NPCID.HallowBoss:
                    return NPC.downedEmpressOfLight;

                case NPCID.Golem:
                case NPCID.GolemHead:
                    return NPC.downedGolemBoss;

                case NPCID.DukeFishron:
                    return NPC.downedFishron;

                case NPCID.CultistBoss:
                    return NPC.downedAncientCultist;

                case NPCID.MoonLordCore:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordHead:
                    return NPC.downedMoonlord;
            }

            if (type == NPCType<DesertScourgeHead>() || type == NPCType<DesertScourgeBody>() || type == NPCType<DesertScourgeTail>())
            {
                return DownedBossSystem.downedDesertScourge;
            }
            else if (type == NPCType<Crabulon.Crabulon>())
            {
                return DownedBossSystem.downedCrabulon;
            }
            else if (type == NPCType<HiveMind.HiveMind>())
            {
                return DownedBossSystem.downedHiveMind;
            }
            else if (type == NPCType<PerforatorHive>())
            {
                return DownedBossSystem.downedPerforator;
            }
            else if (type == NPCType<SlimeGodCore>())
            {
                return DownedBossSystem.downedSlimeGod;
            }
            else if (type == NPCType<Cryogen.Cryogen>())
            {
                return DownedBossSystem.downedCryogen;
            }
            else if (type == NPCType<AquaticScourgeHead>() || type == NPCType<AquaticScourgeBody>() || type == NPCType<AquaticScourgeBodyAlt>() || type == NPCType<AquaticScourgeTail>())
            {
                return DownedBossSystem.downedAquaticScourge;
            }
            else if (type == NPCType<BrimstoneElemental.BrimstoneElemental>())
            {
                return DownedBossSystem.downedBrimstoneElemental;
            }
            else if (type == NPCType<CalamitasClone>())
            {
                return DownedBossSystem.downedCalamitasClone;
            }
            else if (type == NPCType<Leviathan.Leviathan>() || type == NPCType<Anahita>())
            {
                return DownedBossSystem.downedLeviathan;
            }
            else if (type == NPCType<AstrumAureus.AstrumAureus>())
            {
                return DownedBossSystem.downedAstrumAureus;
            }
            else if (type == NPCType<AstrumDeusHead>() || type == NPCType<AstrumDeusBody>() || type == NPCType<AstrumDeusTail>())
            {
                return DownedBossSystem.downedAstrumDeus;
            }
            else if (type == NPCType<PlaguebringerGoliath.PlaguebringerGoliath>())
            {
                return DownedBossSystem.downedPlaguebringer;
            }
            else if (type == NPCType<RavagerBody>())
            {
                return DownedBossSystem.downedRavager;
            }
            else if (type == NPCType<ProfanedGuardianCommander>())
            {
                return DownedBossSystem.downedGuardians;
            }
            else if (type == NPCType<Dragonfolly>())
            {
                return DownedBossSystem.downedDragonfolly;
            }
            else if (type == NPCType<Providence.Providence>())
            {
                return DownedBossSystem.downedProvidence;
            }
            else if (type == NPCType<CeaselessVoid.CeaselessVoid>() || type == NPCType<DarkEnergy>())
            {
                return DownedBossSystem.downedCeaselessVoid;
            }
            else if (type == NPCType<StormWeaverHead>() || type == NPCType<StormWeaverBody>() || type == NPCType<StormWeaverTail>())
            {
                return DownedBossSystem.downedStormWeaver;
            }
            else if (type == NPCType<Signus.Signus>())
            {
                return DownedBossSystem.downedSignus;
            }
            else if (type == NPCType<Polterghast.Polterghast>())
            {
                return DownedBossSystem.downedPolterghast;
            }
            else if (type == NPCType<OldDuke.OldDuke>())
            {
                return DownedBossSystem.downedBoomerDuke;
            }
            else if (type == NPCType<DevourerofGodsHead>() || type == NPCType<DevourerofGodsBody>() || type == NPCType<DevourerofGodsTail>())
            {
                return DownedBossSystem.downedDoG;
            }
            else if (type == NPCType<Yharon.Yharon>())
            {
                return DownedBossSystem.downedYharon;
            }
            else if (type == NPCType<Artemis>() || type == NPCType<Apollo>() || type == NPCType<AresBody>() || type == NPCType<AresGaussNuke>() || type == NPCType<AresLaserCannon>() || type == NPCType<AresPlasmaFlamethrower>() || type == NPCType<AresTeslaCannon>() || type == NPCType<ThanatosHead>() || type == NPCType<ThanatosBody1>() || type == NPCType<ThanatosBody2>() || type == NPCType<ThanatosTail>())
            {
                return DownedBossSystem.downedExoMechs;
            }
            else if (type == NPCType<SupremeCalamitas.SupremeCalamitas>())
            {
                return DownedBossSystem.downedCalamitas;
            }
            else if (type == NPCType<PrimordialWyrmHead>())
            {
                return DownedBossSystem.downedPrimordialWyrm;
            }

            return true;
        }
        #endregion

        #region Speedrun Display
        public static void SetNewBossJustDowned(NPC npc)
        {
            if (!GetDownedBossVariable(npc.type))
            {
                CalamityNPCSets.BossSpeedrunTimerID.TryGetValue(npc.type, out int newBossTypeJustDowned);
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active)
                        continue;

                    CalamityPlayer mp = player.Calamity();
                    mp.lastSplitType = newBossTypeJustDowned;
                    mp.lastSplit = mp.previousSessionTotal.Add(SpeedrunTimerSystem.Elapsed);
                }
            }
        }
        #endregion

        #region Player Counts
        public static bool AnyLivingPlayers()
        {
            foreach (Player player in Main.ActivePlayers)
            {
                if (!player.dead && !player.ghost)
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetActivePlayerCount()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return 1;
            }

            return Main.CurrentFrameFlags.ActivePlayersCount;
        }
        #endregion

        #region Should Affect NPC
        public static bool ShouldAffectNPC(NPC target)
        {
            if (CalamityNPCTypeSets.EaterOfWorlds.Contains(target.type) || CalamityNPCTypeSets.Destroyer.Contains(target.type))
                return false;

            if (target.damage > 0 && !target.boss && !target.friendly && !target.dontTakeDamage && target.type != NPCID.Creeper && target.type != NPCType<RavagerClawLeft>() &&
                target.type != NPCID.MourningWood && target.type != NPCID.Everscream && target.type != NPCID.SantaNK1 && target.type != NPCType<RavagerClawRight>() &&
                target.type != NPCType<ReaperShark>() && target.type != NPCType<Mauler>() && target.type != NPCType<EidolonWyrmHead>() && target.type != NPCID.GolemFistLeft && target.type != NPCID.GolemFistRight &&
                target.type != NPCType<PrimordialWyrmHead>() && target.type != NPCType<ColossalSquid>() && target.type != NPCID.DD2Betsy && !CalamityNPCSets.ResistSlowingDebuffsAndOtherSpecialEffects[target.type] && !AcidRainEvent.AllMinibosses.Contains(target.type))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Old Duke Spawn
        public static void OldDukeSpawn(int plr, int type, int baitType)
        {
            Player player = Main.player[plr];

            if (!player.active || player.dead)
                return;

            int m = 0;
            while (m < Main.maxProjectiles)
            {
                Projectile projectile = Main.projectile[m];
                if (projectile.active && projectile.bobber && projectile.owner == plr)
                {
                    if (plr == Main.myPlayer && projectile.ai[0] == 0f)
                    {
                        for (int item = 0; item < Main.InventorySlotsTotal; item++)
                        {
                            if (player.inventory[item].type == baitType)
                            {
                                player.inventory[item].stack--;
                                if (player.inventory[item].stack <= 0)
                                {
                                    player.inventory[item].SetDefaults(0, false);
                                }
                                break;
                            }
                        }

                        projectile.ai[0] = 2f;
                        projectile.netUpdate = true;

                        // The vanilla game uses a special packet for Duke Fishron spawning.
                        // However, this packet doesn't work on modded NPC types, so we must create
                        // a custom one.
                        // Also, you can't use Netmode != NetmodeID.MultiplayerClient in a projectile context that
                        // has an owner, hence the MyPlayer check.
                        if (Main.myPlayer == projectile.owner)
                        {
                            if (!player.active || player.dead)
                                return;

                            Projectile proj = null;
                            foreach (Projectile p in Main.ActiveProjectiles)
                            {
                                proj = p;
                                if (p.bobber && p.owner == player.whoAmI)
                                {
                                    break;
                                }
                            }

                            if (proj is null)
                                return;

                            var spawnPosX = (int)proj.Center.X;
                            var spawnPosY = (int)proj.Center.Y + 100;
                            if (Main.netMode == NetmodeID.SinglePlayer)
                            {
                                int oldDuke = NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), spawnPosX, spawnPosY, NPCType<OldDuke.OldDuke>());
                                CalamityUtils.BossAwakenMessage(oldDuke);
                            }
                            else if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                SpawnBossOnPositionPacket.Send(spawnPosX, spawnPosY, NPCType<OldDuke.OldDuke>(), player);
                            }
                        }
                    }

                    break;
                }
                else
                {
                    m++;
                }
            }
        }
        #endregion

        #region Astral things
        public static void DoHitDust(NPC npc, int hitDirection, int dustType = 5, float xSpeedMult = 1f, int numHitDust = 5, int numDeathDust = 20)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, dustType, hitDirection * xSpeedMult, -1f);
            }

            if (npc.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, dustType, hitDirection * xSpeedMult, -1f);
                }
            }
        }

        public static void DoFlyingAI(NPC npc, float maxSpeed, float acceleration, float circleTime, float minDistanceTarget = 150f, bool shouldAttackTarget = true)
        {
            //Pick a new target.
            if (npc.target < 0 || npc.target >= Main.maxPlayers || Main.player[npc.target].dead)
            {
                npc.TargetClosest(true);
            }

            Player myTarget = Main.player[npc.target];
            Vector2 toTarget = myTarget.Center - npc.Center;
            float distanceToTarget = toTarget.Length();
            Vector2 maxVelocity = toTarget;

            if (distanceToTarget < 3f)
            {
                maxVelocity = npc.velocity;
            }
            else
            {
                float magnitude = maxSpeed / distanceToTarget;
                maxVelocity *= magnitude;
            }

            //Circular motion
            npc.ai[0]++;

            //y motion
            if (npc.ai[0] > circleTime * 0.5f)
            {
                npc.velocity.Y += acceleration;
            }
            else
            {
                npc.velocity.Y -= acceleration;
            }

            //x motion
            if (npc.ai[0] < circleTime * 0.25f || npc.ai[0] > circleTime * 0.75f)
            {
                npc.velocity.X += acceleration;
            }
            else
            {
                npc.velocity.X -= acceleration;
            }

            //reset
            if (npc.ai[0] > circleTime)
            {
                npc.ai[0] = 0f;
            }

            //if close enough
            if (shouldAttackTarget && distanceToTarget < minDistanceTarget)
            {
                npc.velocity += maxVelocity * 0.007f;
            }

            if (myTarget.dead)
            {
                maxVelocity.X = npc.direction * maxSpeed / 2f;
                maxVelocity.Y = -maxSpeed / 2f;
            }

            //maximise velocity
            if (npc.velocity.X < maxVelocity.X)
            {
                npc.velocity.X += acceleration;
            }

            if (npc.velocity.X > maxVelocity.X)
            {
                npc.velocity.X -= acceleration;
            }

            if (npc.velocity.Y < maxVelocity.Y)
            {
                npc.velocity.Y += acceleration;
            }

            if (npc.velocity.Y > maxVelocity.Y)
            {
                npc.velocity.Y -= acceleration;
            }

            //rotate towards player if alive
            if (!myTarget.dead)
            {
                npc.rotation = toTarget.ToRotation();
            }
            else //don't, do velocity instead
            {
                npc.rotation = npc.velocity.ToRotation();
            }

            npc.rotation += MathHelper.Pi;

            //tile collision
            float collisionDamp = 0.7f;
            if (npc.collideX)
            {
                npc.netUpdate = true;
                npc.velocity.X = npc.oldVelocity.X * -collisionDamp;

                if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                {
                    npc.velocity.X = 2f;
                }

                if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                {
                    npc.velocity.X = -2f;
                }
            }
            if (npc.collideY)
            {
                npc.netUpdate = true;
                npc.velocity.Y = npc.oldVelocity.Y * -collisionDamp;

                if (npc.velocity.Y > 0f && npc.velocity.Y < 1.5f)
                {
                    npc.velocity.Y = 1.5f;
                }

                if (npc.velocity.Y < 0f && npc.velocity.Y > -1.5f)
                {
                    npc.velocity.Y = -1.5f;
                }
            }

            //water collision
            if (npc.wet)
            {
                if (npc.velocity.Y > 0f)
                {
                    npc.velocity.Y *= 0.95f;
                }

                npc.velocity.Y -= 0.3f;

                if (npc.velocity.Y < -2f)
                {
                    npc.velocity.Y = -2f;
                }
            }

            //Taken from source. Important for net?
            if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
            {
                npc.netUpdate = true;
            }
        }

        public static void DoSpiderWallAI(NPC npc, int transformType, float chaseMaxSpeed = 2f, float chaseAcceleration = 0.08f)
        {
            //GET NEW TARGET
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead)
            {
                npc.TargetClosest();
            }

            Vector2 between = Main.player[npc.target].Center - npc.Center;
            float distance = between.Length();

            //modify vector depending on distance and speed.
            if (distance == 0f)
            {
                between.X = npc.velocity.X;
                between.Y = npc.velocity.Y;
            }
            else
            {
                distance = chaseMaxSpeed / distance;
                between.X *= distance;
                between.Y *= distance;
            }

            //update if target dead.
            if (Main.player[npc.target].dead)
            {
                between.X = npc.direction * chaseMaxSpeed / 2f;
                between.Y = -chaseMaxSpeed / 2f;
            }
            npc.spriteDirection = -1;

            //If spider can't see target, circle around to attempt to find the target.
            if (!Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
            {
                //CIRCULAR MOTION, SIMILAR TO FLYING AI (Eater of Souls etc.)
                npc.ai[0]++;

                if (npc.ai[0] > 0f)
                {
                    npc.velocity.Y += 0.023f;
                }
                else
                {
                    npc.velocity.Y -= 0.023f;
                }

                if (npc.ai[0] < -100f || npc.ai[0] > 100f)
                {
                    npc.velocity.X += 0.023f;
                }
                else
                {
                    npc.velocity.X -= 0.023f;
                }

                if (npc.ai[0] > 200f)
                {
                    npc.ai[0] = -200f;
                }

                npc.velocity.X += between.X * 0.007f;
                npc.velocity.Y += between.Y * 0.007f;
                npc.rotation = npc.velocity.ToRotation();

                if (npc.velocity.X > 1.5f)
                {
                    npc.velocity.X *= 0.9f;
                }

                if (npc.velocity.X < -1.5f)
                {
                    npc.velocity.X *= 0.9f;
                }

                if (npc.velocity.Y > 1.5f)
                {
                    npc.velocity.Y *= 0.9f;
                }

                if (npc.velocity.Y < -1.5f)
                {
                    npc.velocity.Y *= 0.9f;
                }

                npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -3f, 3f);
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -3f, 3f);
            }
            else //CHASE TARGET
            {
                if (npc.velocity.X < between.X)
                {
                    npc.velocity.X += chaseAcceleration;

                    if (npc.velocity.X < 0f && between.X > 0f)
                    {
                        npc.velocity.X += chaseAcceleration;
                    }
                }
                else if (npc.velocity.X > between.X)
                {
                    npc.velocity.X -= chaseAcceleration;

                    if (npc.velocity.X > 0f && between.X < 0f)
                    {
                        npc.velocity.X -= chaseAcceleration;
                    }
                }
                if (npc.velocity.Y < between.Y)
                {
                    npc.velocity.Y += chaseAcceleration;

                    if (npc.velocity.Y < 0f && between.Y > 0f)
                    {
                        npc.velocity.Y += chaseAcceleration;
                    }
                }
                else if (npc.velocity.Y > between.Y)
                {
                    npc.velocity.Y -= chaseAcceleration;

                    if (npc.velocity.Y > 0f && between.Y < 0f)
                    {
                        npc.velocity.Y -= chaseAcceleration;
                    }
                }
                npc.rotation = between.ToRotation();
            }

            //DAMP COLLISIONS OFF OF WALLS
            float collisionDamp = 0.5f;
            if (npc.collideX)
            {
                npc.netUpdate = true;
                npc.velocity.X = npc.oldVelocity.X * -collisionDamp;

                if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                {
                    npc.velocity.X = 2f;
                }

                if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                {
                    npc.velocity.X = -2f;
                }
            }
            if (npc.collideY)
            {
                npc.netUpdate = true;
                npc.velocity.Y = npc.oldVelocity.Y * -collisionDamp;

                if (npc.velocity.Y > 0f && npc.velocity.Y < 1.5f)
                {
                    npc.velocity.Y = 2f;
                }

                if (npc.velocity.Y < 0f && npc.velocity.Y > -1.5f)
                {
                    npc.velocity.Y = -2f;
                }
            }

            if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
            {
                npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int x = (int)npc.Center.X / 16;
                int y = (int)npc.Center.Y / 16;
                bool flag = false;

                for (int i = x - 1; i <= x + 1; i++)
                {
                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        if (Main.tile[i, j].WallType > 0)
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    npc.Transform(transformType);
                    return;
                }
            }
        }

        public static void DoVultureAI(NPC npc, float acceleration = 0.1f, float maxSpeed = 3f, int sitWidth = 30, int flyWidth = 50, int rangeX = 100, int rangeY = 100)
        {
            npc.localAI[0]++;
            npc.noGravity = true;
            npc.TargetClosest(true);

            if (npc.ai[0] == 0f)
            {
                npc.width = sitWidth;
                npc.noGravity = false;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (npc.velocity.X != 0f || npc.velocity.Y < 0f || npc.velocity.Y > 0.3)
                    {
                        npc.ai[0] = 1f;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        Rectangle playerRect = Main.player[npc.target].getRect();
                        Rectangle rangeRect = new Rectangle((int)npc.Center.X - rangeX, (int)npc.Center.Y - rangeY, rangeX * 2, rangeY * 2);
                        if (npc.localAI[0] > 20f && (rangeRect.Intersects(playerRect) || npc.life < npc.lifeMax))
                        {
                            npc.ai[0] = 1f;
                            npc.velocity.Y -= 6f;
                            npc.netUpdate = true;
                        }
                    }
                }
            }
            else if (!Main.player[npc.target].dead)
            {
                npc.width = flyWidth;

                //Collision damping
                if (npc.collideX)
                {
                    npc.velocity.X = npc.oldVelocity.X * -0.5f;

                    if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                    {
                        npc.velocity.X = 2f;
                    }

                    if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                    {
                        npc.velocity.X = -2f;
                    }
                }

                if (npc.collideY)
                {
                    npc.velocity.Y = npc.oldVelocity.Y * -0.5f;

                    if (npc.velocity.Y > 0f && npc.velocity.Y < 1f)
                    {
                        npc.velocity.Y = 1f;
                    }

                    if (npc.velocity.Y < 0f && npc.velocity.Y > -1f)
                    {
                        npc.velocity.Y = -1f;
                    }
                }

                if (npc.direction == -1 && npc.velocity.X > -maxSpeed)
                {
                    npc.velocity.X -= acceleration;

                    if (npc.velocity.X > maxSpeed)
                    {
                        npc.velocity.X -= acceleration;
                    }
                    else if (npc.velocity.X > 0f)
                    {
                        npc.velocity.X -= acceleration * 0.5f;
                    }

                    if (npc.velocity.X < -maxSpeed)
                    {
                        npc.velocity.X = -maxSpeed;
                    }
                }
                else if (npc.direction == 1 && npc.velocity.X < maxSpeed)
                {
                    npc.velocity.X += acceleration;

                    if (npc.velocity.X < -maxSpeed)
                    {
                        npc.velocity.X += acceleration;
                    }
                    else if (npc.velocity.X < 0f)
                    {
                        npc.velocity.X += acceleration * 0.5f;
                    }

                    if (npc.velocity.X > maxSpeed)
                    {
                        npc.velocity.X = maxSpeed;
                    }
                }

                float xDistance = Math.Abs(npc.Center.X - Main.player[npc.target].Center.X);
                float yLimiter = Main.player[npc.target].position.Y - (npc.height / 2f);
                if (xDistance > 50f)
                {
                    yLimiter -= 100f;
                }

                if (npc.position.Y < yLimiter)
                {
                    npc.velocity.Y += acceleration * 0.5f;

                    if (npc.velocity.Y < 0f)
                    {
                        npc.velocity.Y += acceleration * 0.1f;
                    }
                }
                else
                {
                    npc.velocity.Y -= acceleration * 0.5f;

                    if (npc.velocity.Y > 0f)
                    {
                        npc.velocity.Y -= acceleration * 0.1f;
                    }
                }

                if (npc.velocity.Y < -maxSpeed)
                {
                    npc.velocity.Y = -maxSpeed;
                }

                if (npc.velocity.Y > maxSpeed)
                {
                    npc.velocity.Y = maxSpeed;
                }
            }
            //Change velocity if wet.
            if (npc.wet)
            {
                if (npc.velocity.Y > 0f)
                {
                    npc.velocity.Y *= 0.95f;
                }

                npc.velocity.Y -= 0.5f;

                if (npc.velocity.Y < -4f)
                {
                    npc.velocity.Y = -4f;
                }
            }
        }

        /// <summary>
        /// Allows you to spawn dust on the NPC in a certain place. Uses the npc.position value as the base point for the rectangle.
        /// Takes direction and rotation into account.
        /// </summary>
        /// <param name="frameWidth">The width of the sheet for the NPC.</param>
        /// <param name="rect">The place to put a dust.</param>
        /// <param name="chance">The chance to spawn a dust (0.3 = 30%)</param>
        public static Dust SpawnDustOnNPC(NPC npc, int frameWidth, int frameHeight, int dustType, Rectangle rect, Vector2 velocity = default, float chance = 0.5f, bool useSpriteDirection = false)
        {
            Vector2 half = new Vector2(frameWidth / 2f, frameHeight / 2f);

            //"flip" the rectangle's position x-wise.
            if ((!useSpriteDirection && npc.direction == 1) || (useSpriteDirection && npc.spriteDirection == 1))
            {
                rect.X = frameWidth - rect.Right;
            }

            if (Main.rand.NextFloat(1f) < chance)
            {
                Vector2 offset = npc.Center - half + new Vector2(Main.rand.NextFloat(rect.Left, rect.Right), Main.rand.NextFloat(rect.Top, rect.Bottom)) - npc.Center;
                offset = offset.RotatedBy(npc.rotation);
                Dust d = Dust.NewDustPerfect(npc.Center + offset, dustType, velocity);
                return d;
            }
            return null;
        }
        #endregion

        #region Bestiary
        public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // Replace vanilla bestiary flavor text for certain NPCs
            // These are ordered by their order in the bestiary, if you're wondering why it seems so arbitrary lmao
            switch (npc.netID)
            {
                case NPCID.Guide:
                case NPCID.Dryad:
                case NPCID.Mechanic:
                case NPCID.EmpressButterfly:
                case NPCID.DemonEye:
                case NPCID.CataractEye:
                case NPCID.DialatedEye:
                case NPCID.SleepyEye:
                case NPCID.GreenEye:
                case NPCID.PurpleEye:
                case NPCID.Wraith:
                case NPCID.BloodNautilus:
                case NPCID.DiggerHead:
                case NPCID.UndeadMiner:
                case NPCID.GraniteGolem:
                case NPCID.GraniteFlyer:
                case NPCID.GreekSkeleton:
                case NPCID.UndeadViking:
                case NPCID.IcyMerman:
                case NPCID.IceElemental:
                case NPCID.DesertBeast:
                case NPCID.DuneSplicerHead:
                case NPCID.SandElemental:
                case NPCID.SandShark:
                case NPCID.SandsharkCorrupt:
                case NPCID.SandsharkCrimson:
                case NPCID.SandsharkHallow:
                case NPCID.MeteorHead:
                case NPCID.AngryBones:
                case NPCID.AngryBonesBig:
                case NPCID.AngryBonesBigMuscle:
                case NPCID.AngryBonesBigHelmet:
                case NPCID.BlueArmoredBones:
                case NPCID.BlueArmoredBonesMace:
                case NPCID.BlueArmoredBonesNoPants:
                case NPCID.BlueArmoredBonesSword:
                case NPCID.HellArmoredBones:
                case NPCID.HellArmoredBonesSpikeShield:
                case NPCID.HellArmoredBonesMace:
                case NPCID.HellArmoredBonesSword:
                case NPCID.RustyArmoredBonesAxe:
                case NPCID.RustyArmoredBonesFlail:
                case NPCID.RustyArmoredBonesSword:
                case NPCID.RustyArmoredBonesSwordNoArmor:
                case NPCID.SkeletonSniper:
                case NPCID.TacticalSkeleton:
                case NPCID.SkeletonCommando:
                case NPCID.BoneLee:
                case NPCID.Paladin:
                case NPCID.DiabolistRed:
                case NPCID.DiabolistWhite:
                case NPCID.Necromancer:
                case NPCID.NecromancerArmored:
                case NPCID.RaggedCaster:
                case NPCID.RaggedCasterOpenCoat:
                case NPCID.DungeonGuardian:
                case NPCID.BoneSerpentHead:
                case NPCID.Demon:
                case NPCID.VoodooDemon:
                case NPCID.RedDevil:
                case NPCID.WyvernHead:
                case NPCID.Harpy:
                case NPCID.MartianProbe:
                case NPCID.SeekerHead:
                case NPCID.DesertDjinn:
                case NPCID.ChaosElemental:
                case NPCID.GoblinThief:
                case NPCID.GoblinSummoner:
                case NPCID.GoblinSorcerer:
                case NPCID.PirateCaptain:
                case NPCID.Scutlix:
                case NPCID.MartianSaucerCore:
                case NPCID.TorchGod:
                case NPCID.EyeofCthulhu:
                case NPCID.BrainofCthulhu:
                case NPCID.SkeletronHead:
                case NPCID.WallofFlesh:
                case NPCID.QueenSlimeBoss:
                case NPCID.Retinazer:
                case NPCID.Spazmatism:
                case NPCID.TheDestroyer:
                case NPCID.SkeletronPrime:
                case NPCID.Plantera:
                case NPCID.HallowBoss:
                case NPCID.Golem:
                case NPCID.DukeFishron:
                case NPCID.CultistBoss:
                case NPCID.CultistDevote:
                case NPCID.LunarTowerNebula:
                case NPCID.LunarTowerSolar:
                case NPCID.LunarTowerVortex:
                case NPCID.LunarTowerStardust:
                case NPCID.MoonLordCore:
                    FlavorTextBestiaryInfoElement f = new("Hi CS0120");
                    bestiaryEntry.Info.RemoveAll(i => i.GetType() == f.GetType());
                    bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement(CalamityUtils.GetTextValue($"Bestiary.Vanilla.{Lang.GetNPCName(npc.netID).Key}")));
                    break;
                default:
                    break;

            }

            // Create a string array containing all an NPC's debuff resistances
            string[] elements =
            [
                NPCDebuffResistText(npc.Calamity().VulnerableToCold, CalamityUtils.GetTextValue("UI.DebuffSystem.Cold")),
                NPCDebuffResistText(npc.Calamity().VulnerableToElectricity, CalamityUtils.GetTextValue("UI.DebuffSystem.Electricity")),
                NPCDebuffResistText(npc.Calamity().VulnerableToHeat, CalamityUtils.GetTextValue("UI.DebuffSystem.Heat")),
                NPCDebuffResistText(npc.Calamity().VulnerableToSickness, CalamityUtils.GetTextValue("UI.DebuffSystem.Sickness")),
                NPCDebuffResistText(npc.Calamity().VulnerableToWater, CalamityUtils.GetTextValue("UI.DebuffSystem.Water"))
            ];

            // Insert the debuff info into the NPC's bestiary entry
            bestiaryEntry.Info.Insert(0, new BestiaryDebuffInfo(elements));

            // Add the Astral Infection to the Enchanted Nightcrawler's entry as it spawns there now
            if (npc.type == NPCID.EnchantedNightcrawler)
            {
                bestiaryEntry.AddTags(GetInstance<AstralInfectionBiome>().ModBiomeBestiaryInfoElement);
            }

            // Add the Surface Mushroom biome to the Truffle Worm's entry as it spawns there now
            if (npc.type == NPCID.TruffleWorm)
            {
                bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.SurfaceMushroom);
            }

            // Remove the static portraits from vanilla worms so that Calamity's worm movement can be added in PreDraw
            switch (npc.netID)
            {
                case NPCID.DiggerHead:
                case NPCID.GiantWormHead:
                case NPCID.EaterofWorldsHead:
                case NPCID.WyvernHead:
                case NPCID.StardustWormHead:
                case NPCID.SolarCrawltipedeHead:
                case NPCID.CultistDragonHead:
                case NPCID.TheDestroyer:
                case NPCID.LeechHead:
                case NPCID.DevourerHead:
                case NPCID.TombCrawlerHead:
                case NPCID.DuneSplicerHead:
                case NPCID.BloodEelHead:
                case NPCID.BoneSerpentHead:
                case NPCID.SeekerHead:
                    NPCID.Sets.NPCBestiaryDrawOffset[npc.type] = NPCID.Sets.NPCBestiaryDrawOffset[npc.type] with { CustomTexturePath = null };
                    break;
            }
        }

        public static string NPCDebuffResistText(bool? effectiveness, string name)
        {
            string result = CalamityUtils.GetTextValue("UI.DebuffSystem.Neutral");
            if (effectiveness == true)
            {
                result = CalamityUtils.GetTextValue("UI.DebuffSystem.Weak");
            }
            else if (effectiveness == false)
            {
                result = CalamityUtils.GetTextValue("UI.DebuffSystem.Resistant");
            }
            result += " " + CalamityUtils.GetTextValue("UI.DebuffSystem.To") + " " + name;
            return result;
        }
        #endregion
    }
}
