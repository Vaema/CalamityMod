using System;
using System.Collections.Generic;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Melee.MaceFlails;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod
{
    public sealed class CalamityLists : ModSystem
    {
        public static List<int> hornetList;
        public static List<int> mossHornetList;
        public static List<int> minibossList;

        public static List<int> pierceResistList;
        public static List<int> pierceResistExceptionLeviAureusList;
        public static List<int> pierceResistExceptionList;

        public static List<int> AstrumDeusIDs;
        public static List<int> DevourerOfGodsIDs;
        public static List<int> CosmicGuardianIDs;
        public static List<int> AquaticScourgeIDs;
        public static List<int> PerforatorIDs;
        public static List<int> DesertScourgeIDs;
        public static List<int> EaterofWorldsIDs;
        public static List<int> SlimeGodIDs;
        public static List<int> DeathModeSplittingWormIDs;
        public static List<int> DestroyerIDs;
        public static List<int> ThanatosIDs;
        public static List<int> AresIDs;
        public static List<int> SkeletronPrimeIDs;
        public static List<int> StormWeaverIDs;
        public static List<int> RavagerIDs;
        public static List<int> GolemIDs;
        public static List<int> BoundNPCIDs;

        public static List<int> GrenadeResistIDs;
        public static List<int> ZeroContactDamageNPCList;
        public static List<int> HardmodeNPCNerfList;

        public static SortedDictionary<int, int> BossRushHPChanges;
        public static SortedDictionary<int, int> BossValues;
        public static SortedDictionary<int, int> bossTypes;

        public static List<int> legOverrideList;

        public static List<int> kamiDebuffColorImmuneList;

        public static Dictionary<int, int> EncryptedSchematicIDRelationship;

        public static List<int> DisabledSummonerNerfItems;
        public static List<int> DisabledSummonerNerfMinions;

        public static List<int> VeneratedLocketBanlist; //To ban projectiles from locket, mainly spikeballs altho Toasty asked me to add mod calls for adding stuff like Dreamtastic

        /// <summary>
        /// Each Sunken Sea subbiome has a correspoding spawn condition boolean value and a biome type.
        /// </summary>
        public static SortedDictionary<SunkenSeaBiomeFlags, (Func<NPCSpawnInfo, bool> SpawnCondition, int BiomeType)> SunkenSeaBiomeCorrespondentValues { get; private set; }

        public override void OnModLoad()
        {
            hornetList = new List<int>()
            {
                NPCID.BigHornetStingy,
                NPCID.LittleHornetStingy,
                NPCID.BigHornetSpikey,
                NPCID.LittleHornetSpikey,
                NPCID.BigHornetLeafy,
                NPCID.LittleHornetLeafy,
                NPCID.BigHornetHoney,
                NPCID.LittleHornetHoney,
                NPCID.BigHornetFatty,
                NPCID.LittleHornetFatty,
                NPCID.BigStinger,
                NPCID.LittleStinger,
                NPCID.Hornet,
                NPCID.HornetFatty,
                NPCID.HornetHoney,
                NPCID.HornetLeafy,
                NPCID.HornetSpikey,
                NPCID.HornetStingy
            };

            mossHornetList = new List<int>()
            {
                NPCID.MossHornet,
                NPCID.TinyMossHornet,
                NPCID.LittleMossHornet,
                NPCID.BigMossHornet,
                NPCID.GiantMossHornet
            };

            minibossList = new List<int>()
            {
                NPCType<EidolonWyrmHead>(),
                NPCType<Mauler>(),
                NPCType<ReaperShark>(),
                NPCType<ColossalSquid>(),
                NPCType<GreatSandShark>(),
                NPCType<GiantClam>(),
                NPCType<ArmoredDiggerHead>(),
                NPCType<ArmoredDiggerBody>(),
                NPCType<ArmoredDiggerTail>(),
                NPCType<ThiccWaifu>(),
                NPCType<Horse>(),
                NPCType<PlaguebringerMiniboss>(),
                NPCID.Pumpking,
                NPCID.MourningWood,
                NPCID.IceQueen,
                NPCID.SantaNK1,
                NPCID.Everscream,
                NPCID.DD2Betsy,
                NPCID.Mothron,
                NPCID.MartianSaucer,
                NPCID.MartianSaucerCannon,
                NPCID.MartianSaucerCore,
                NPCID.MartianSaucerTurret,
            };

            pierceResistList = new List<int>()
            {
                NPCID.EaterofWorldsHead,
                NPCID.EaterofWorldsBody,
                NPCID.EaterofWorldsTail,
                NPCID.Creeper,
                NPCID.TheDestroyer,
                NPCID.TheDestroyerBody,
                NPCID.TheDestroyerTail,
                NPCType<DesertScourgeHead>(),
                NPCType<DesertScourgeBody>(),
                NPCType<DesertScourgeTail>(),
                NPCType<PerforatorHeadLarge>(),
                NPCType<PerforatorBodyLarge>(),
                NPCType<PerforatorTailLarge>(),
                NPCType<PerforatorHeadMedium>(),
                NPCType<PerforatorBodyMedium>(),
                NPCType<PerforatorTailMedium>(),
                NPCType<PerforatorHeadSmall>(),
                NPCType<PerforatorBodySmall>(),
                NPCType<PerforatorTailSmall>(),
                NPCType<AquaticScourgeHead>(),
                NPCType<AquaticScourgeBody>(),
                NPCType<AquaticScourgeBodyAlt>(),
                NPCType<AquaticScourgeTail>(),
                NPCType<AstrumAureus>(),
                NPCType<Leviathan>(),
                NPCType<RavagerHead>(),
                NPCType<RavagerClawLeft>(),
                NPCType<RavagerClawRight>(),
                NPCType<RavagerLegLeft>(),
                NPCType<RavagerLegRight>(),
                NPCType<AstrumDeusHead>(),
                NPCType<AstrumDeusBody>(),
                NPCType<AstrumDeusTail>(),
                NPCType<ProfanedRocks>(),
                NPCType<DarkEnergy>(),
                NPCType<StormWeaverHead>(),
                NPCType<StormWeaverBody>(),
                NPCType<StormWeaverTail>(),
                NPCType<CosmicGuardianHead>(),
                NPCType<CosmicGuardianBody>(),
                NPCType<CosmicGuardianTail>(),
                NPCType<ThanatosHead>(),
                NPCType<ThanatosBody1>(),
                NPCType<ThanatosBody2>(),
                NPCType<ThanatosTail>(),
                NPCType<BrimstoneHeart>(),
                NPCType<AresBody>(),
                NPCType<AresLaserCannon>(),
                NPCType<AresTeslaCannon>(),
                NPCType<AresPlasmaFlamethrower>(),
                NPCType<AresGaussNuke>()
            };

            pierceResistExceptionLeviAureusList = new List<int>()
            {
                ProjectileID.NettleBurstEnd,
                ProjectileID.NettleBurstLeft,
                ProjectileID.NettleBurstRight,
                ProjectileID.PrincessWeapon,
                ProjectileType<AnahitasArpeggioNote>(),
                ProjectileType<AtlantisSpear>(),
                ProjectileType<AuroraFire>(),
                ProjectileType<BallisticPoisonCloud>(),
                ProjectileType<DuststormCloudHitbox>()
            };

            pierceResistExceptionList = new List<int>()
            {
                ProjectileID.Arkhalis,
                ProjectileID.ChargedBlasterLaser,
                ProjectileID.ClingerStaff,
                ProjectileID.FinalFractal,
                ProjectileID.FlyingKnife,
                ProjectileID.LastPrismLaser,
                ProjectileID.MechanicalPiranha,
                ProjectileID.MonkStaffT3,
                ProjectileID.PiercingStarlight,
                ProjectileID.Terragrim,
                ProjectileType<AcidicSaxBubble>(),
                ProjectileType<BasherHoldout>(),
                ProjectileType<BlushieStaffProj>(),
                ProjectileType<BonebreakerProjectile>(),
                ProjectileType<CometQuasherHoldout>(),
                ProjectileType<DarkSparkBeam>(),
                ProjectileType<DevilsSunriseCyclone>(),
                ProjectileType<DevilsSunriseProj>(),
                ProjectileType<DragonRageStaff>(),
                ProjectileType<EarthHoldout>(),
                ProjectileType<EclipsesStealth>(),
                ProjectileType<EidolicWailSoundwave>(),
                ProjectileType<EmesisGore>(),
                ProjectileType<EradicatorProjectile>(),
                ProjectileType<ExoFlareCluster>(),
                ProjectileType<EyeOfNightCell>(),
                ProjectileType<FantasyTalismanProj>(),
                ProjectileType<FantasyTalismanStealth>(),
                ProjectileType<GodsParanoiaProj>(),
                ProjectileType<GrandDadHoldout>(),
                ProjectileType<GrandGuardianHoldout>(),
                ProjectileType<HellbornHoldout>(),
                ProjectileType<HellkiteHoldout>(),
                ProjectileType<InsidiousHarpoon>(),
                ProjectileType<JawsProjectile>(),
                ProjectileType<LeviathanTooth>(),
                ProjectileType<LiliesOfFinalityAoE>(),
                ProjectileType<LionfishProj>(),
                ProjectileType<MajesticGuardHoldout>(),
                ProjectileType<MechanicalBarracuda>(),
                ProjectileType<MetalShard>(),
                ProjectileType<MurasamaSlash>(),
                ProjectileType<NastyChollaBol>(),
                ProjectileType<OmnibladeSwing>(),
                ProjectileType<PhaseslayerProjectile>(),
                ProjectileType<PhotonRipperProjectile>(),
                ProjectileType<PlaguedFuelPackCloud>(),
                ProjectileType<PlantationStaffSporeCloud>(),
                ProjectileType<PrismaticRay>(),
                ProjectileType<RancorLaserbeam>(),
                ProjectileType<ReaperProjectile>(),
                ProjectileType<RespiteblockHoldout>(),
                ProjectileType<SacrificeProjectile>(),
                ProjectileType<SnapClamProj>(),
                ProjectileType<SnapClamStealth>(),
                ProjectileType<Snowflake>(),
                ProjectileType<SparklingLaser>(),
                ProjectileType<SpiritCongregation>(),
                ProjectileType<StarmageddonBinaryStarCenter>(),
                ProjectileType<StellarStrikerHoldout>(),
                ProjectileType<StickyBol>(),
                ProjectileType<AcidRocket>(),
                ProjectileType<TaserHook>(),
                ProjectileType<Teslabeam>(),
                ProjectileType<TyphonsGreedStaff>(),
                ProjectileType<UrchinMaceProjectile>(),
                ProjectileType<UrchinStingerProj>(),
                ProjectileType<ViolenceThrownProjectile>(),
                ProjectileType<WaterLeechProj>(),
                ProjectileType<YateveoBloomMace>(),
                ProjectileType<YharimsCrystalBeam>(),
            };

            // Lists of enemies that resist piercing to some extent (mostly worms).
            // Could prove useful for other things as well.

            AstrumDeusIDs = new List<int>
            {
                NPCType<AstrumDeusHead>(),
                NPCType<AstrumDeusBody>(),
                NPCType<AstrumDeusTail>()
            };

            DevourerOfGodsIDs = new List<int>
            {
                NPCType<DevourerofGodsHead>(),
                NPCType<DevourerofGodsBody>(),
                NPCType<DevourerofGodsTail>()
            };

            CosmicGuardianIDs = new List<int>
            {
                NPCType<CosmicGuardianHead>(),
                NPCType<CosmicGuardianBody>(),
                NPCType<CosmicGuardianTail>()
            };

            AquaticScourgeIDs = new List<int>
            {
                NPCType<AquaticScourgeHead>(),
                NPCType<AquaticScourgeBody>(),
                NPCType<AquaticScourgeBodyAlt>(),
                NPCType<AquaticScourgeTail>()
            };

            PerforatorIDs = new List<int>
            {
                NPCType<PerforatorHeadLarge>(),
                NPCType<PerforatorBodyLarge>(),
                NPCType<PerforatorTailLarge>(),
                NPCType<PerforatorHeadMedium>(),
                NPCType<PerforatorBodyMedium>(),
                NPCType<PerforatorTailMedium>(),
                NPCType<PerforatorHeadSmall>(),
                NPCType<PerforatorBodySmall>(),
                NPCType<PerforatorTailSmall>()
            };

            DesertScourgeIDs = new List<int>
            {
                NPCType<DesertScourgeHead>(),
                NPCType<DesertScourgeBody>(),
                NPCType<DesertScourgeTail>()
            };

            EaterofWorldsIDs = new List<int>
            {
                NPCID.EaterofWorldsHead,
                NPCID.EaterofWorldsBody,
                NPCID.EaterofWorldsTail
            };

            SlimeGodIDs = new List<int>
            {
                NPCType<EbonianPaladin>(),
                NPCType<CrimulanPaladin>(),
                NPCType<SplitEbonianPaladin>(),
                NPCType<SplitCrimulanPaladin>(),
                NPCType<SlimeGodCore>()
            };

            DeathModeSplittingWormIDs = new List<int>
            {
                NPCID.DuneSplicerHead,
                NPCID.DuneSplicerBody,
                NPCID.DuneSplicerTail,
                NPCID.DiggerHead,
                NPCID.DiggerBody,
                NPCID.DiggerTail,
                NPCID.SeekerHead,
                NPCID.SeekerBody,
                NPCID.SeekerTail
            };

            DestroyerIDs = new List<int>
            {
                NPCID.TheDestroyer,
                NPCID.TheDestroyerBody,
                NPCID.TheDestroyerTail
            };

            ThanatosIDs = new List<int>
            {
                NPCType<ThanatosHead>(),
                NPCType<ThanatosBody1>(),
                NPCType<ThanatosBody2>(),
                NPCType<ThanatosTail>()
            };

            AresIDs = new List<int>
            {
                NPCType<AresBody>(),
                NPCType<AresGaussNuke>(),
                NPCType<AresLaserCannon>(),
                NPCType<AresPlasmaFlamethrower>(),
                NPCType<AresTeslaCannon>()
            };

            SkeletronPrimeIDs = new List<int>
            {
                NPCID.SkeletronPrime,
                NPCType<SkeletronPrime2>(),
                NPCID.PrimeCannon,
                NPCID.PrimeLaser,
                NPCID.PrimeSaw,
                NPCID.PrimeVice
            };

            StormWeaverIDs = new List<int>
            {
                NPCType<StormWeaverHead>(),
                NPCType<StormWeaverBody>(),
                NPCType<StormWeaverTail>()
            };

            // Purposefully does not include the freed head
            RavagerIDs = new List<int>
            {
                NPCType<RavagerBody>(),
                NPCType<RavagerClawLeft>(),
                NPCType<RavagerClawRight>(),
                NPCType<RavagerLegLeft>(),
                NPCType<RavagerLegRight>(),
                NPCType<RavagerHead>()
            };

            GolemIDs = new List<int>
            {
                NPCID.Golem,
                NPCID.GolemHead,
                NPCID.GolemHeadFree,
                NPCID.GolemFistLeft,
                NPCID.GolemFistRight
            };

            GrenadeResistIDs = new List<int>
            {
                ProjectileID.Grenade,
                ProjectileID.StickyGrenade,
                ProjectileID.BouncyGrenade,
                ProjectileID.Bomb,
                ProjectileID.StickyBomb,
                ProjectileID.BouncyBomb,
                ProjectileID.Dynamite,
                ProjectileID.StickyDynamite,
                ProjectileID.BouncyDynamite,
                ProjectileID.Explosives,
                ProjectileID.ExplosiveBunny,
                ProjectileID.PartyGirlGrenade,
                ProjectileID.BombFish,
                ProjectileID.Beenade,
                ProjectileID.Bee,
                ProjectileID.GiantBee,
                ProjectileType<AeroExplosive>(),
                ProjectileID.ScarabBomb,
                ProjectileID.TNTBarrel
            };

            ZeroContactDamageNPCList = new List<int>
            {
                NPCID.Harpy,
                NPCID.Salamander,
                NPCID.Salamander2,
                NPCID.Salamander3,
                NPCID.Salamander4,
                NPCID.Salamander5,
                NPCID.Salamander6,
                NPCID.Salamander7,
                NPCID.Salamander8,
                NPCID.Salamander9,
                NPCID.GiantCursedSkull,
                NPCID.FungiBulb,
                NPCID.GiantFungiBulb,
                NPCID.IcyMerman,
                NPCID.AngryNimbus,
                NPCID.SandElemental,
                NPCID.DarkCaster,
                NPCID.FireImp,
                NPCID.Tim,
                NPCID.CultistArcherBlue,
                NPCID.DesertDjinn,
                NPCID.DiabolistRed,
                NPCID.DiabolistWhite,
                NPCID.Gastropod,
                NPCID.IceElemental,
                NPCID.IchorSticker,
                NPCID.Necromancer,
                NPCID.NecromancerArmored,
                NPCID.RaggedCaster,
                NPCID.RaggedCasterOpenCoat,
                NPCID.RuneWizard,
                NPCID.SkeletonArcher,
                NPCID.SkeletonCommando,
                NPCID.SkeletonSniper,
                NPCID.TacticalSkeleton,
                NPCID.Clown,
                NPCID.GoblinArcher,
                NPCID.GoblinSorcerer,
                NPCID.GoblinSummoner,
                NPCID.PirateCrossbower,
                NPCID.PirateDeadeye,
                NPCID.PirateCaptain,
                NPCID.SnowmanGangsta,
                NPCID.SnowBalla,
                NPCID.DrManFly,
                NPCID.Eyezor,
                NPCID.Nailhead,
                NPCID.BrainScrambler,
                NPCID.GigaZapper,
                NPCID.RayGunner,
                NPCID.ScutlixRider,
                NPCID.MartianWalker,
                NPCID.MartianTurret,
                NPCID.ElfCopter,
                NPCID.ElfArcher,
                NPCID.NebulaBrain,
                NPCID.NebulaSoldier,
                NPCID.StardustCellSmall,
                NPCID.StardustJellyfishBig,
                NPCID.StardustSoldier,
                NPCID.StardustSpiderBig,
                NPCID.VortexHornetQueen,
                NPCID.VortexRifleman,
                NPCID.VortexSoldier,
                NPCID.PirateShipCannon,
                NPCID.MartianSaucer,
                NPCID.MartianSaucerCannon,
                NPCID.MartianSaucerCore,
                NPCID.MartianSaucerTurret,
                NPCID.Probe,
                NPCID.CultistBoss,
                NPCID.GolemHead,
                NPCID.GolemHeadFree,
                NPCID.MoonLordFreeEye,
                NPCID.BloodSquid,
                NPCID.PlanterasHook,
                NPCID.Dandelion,
                NPCID.DD2DarkMageT1,
                NPCID.DD2DarkMageT3,
                NPCID.DD2OgreT2,
                NPCID.DD2OgreT3,
                NPCID.DD2GoblinBomberT1,
                NPCID.DD2GoblinBomberT2,
                NPCID.DD2GoblinBomberT3,
                NPCID.DD2JavelinstT1,
                NPCID.DD2JavelinstT2,
                NPCID.DD2JavelinstT3,
                NPCID.DD2KoboldWalkerT2,
                NPCID.DD2KoboldWalkerT3,
                NPCID.DD2DrakinT2,
                NPCID.DD2DrakinT3,
                NPCID.DD2KoboldFlyerT2,
                NPCID.DD2KoboldFlyerT3,
                NPCID.DD2WitherBeastT2,
                NPCID.DD2WitherBeastT3,
                NPCID.DD2LightningBugT3,
                NPCID.MourningWood,
                NPCID.Pumpking,
                NPCID.Everscream,
                NPCID.IceQueen,
                NPCID.SantaNK1,
                NPCID.DevourerBody,
                NPCID.DevourerTail,
                NPCID.DiggerBody,
                NPCID.DiggerTail,
                NPCID.TombCrawlerBody,
                NPCID.TombCrawlerTail,
                NPCID.DuneSplicerBody,
                NPCID.DuneSplicerTail,
                NPCID.GiantWormBody,
                NPCID.GiantWormTail,
                NPCID.LeechBody,
                NPCID.LeechTail,
                NPCID.StardustWormBody,
                NPCID.StardustWormTail,
                NPCID.SeekerBody,
                NPCID.SeekerTail,
                NPCID.BoneSerpentBody,
                NPCID.BoneSerpentTail,
                NPCID.WyvernBody,
                NPCID.WyvernTail,
                NPCID.WyvernBody2,
                NPCID.WyvernBody3,
                NPCID.WyvernLegs,
                NPCID.CultistDragonBody1,
                NPCID.CultistDragonBody2,
                NPCID.CultistDragonBody3,
                NPCID.CultistDragonBody4,
                NPCID.CultistDragonTail,
                NPCID.BloodEelBody,
                NPCID.BloodEelTail,
                NPCID.AncientDoom
            };

            // Reduce contact damage by 25%
            HardmodeNPCNerfList = new List<int>
            {
                NPCID.AnglerFish,
                NPCID.AngryTrapper,
                NPCID.Arapaima,
                NPCID.BlackRecluse,
                NPCID.BlackRecluseWall,
                NPCID.BloodJelly,
                NPCID.FungoFish,
                NPCID.GreenJellyfish,
                NPCID.Clinger,
                NPCID.ArmoredSkeleton,
                NPCID.ArmoredViking,
                NPCID.Mummy,
                NPCID.DarkMummy,
                NPCID.LightMummy,
                NPCID.BloodFeeder,
                NPCID.DesertBeast,
                NPCID.ChaosElemental,
                NPCID.BloodMummy,
                NPCID.CorruptSlime,
                NPCID.Slimeling,
                NPCID.Corruptor,
                NPCID.Crimslime,
                NPCID.BigCrimslime,
                NPCID.LittleCrimslime,
                NPCID.CrimsonAxe,
                NPCID.CursedHammer,
                NPCID.Derpling,
                NPCID.Herpling,
                NPCID.DiggerHead,
                NPCID.DesertGhoul,
                NPCID.DesertGhoulCorruption,
                NPCID.DesertGhoulCrimson,
                NPCID.DesertGhoulHallow,
                NPCID.DuneSplicerHead,
                NPCID.EnchantedSword,
                NPCID.FloatyGross,
                NPCID.GiantBat,
                NPCID.GiantFlyingFox,
                NPCID.FungiSpore,
                NPCID.GiantTortoise,
                NPCID.IceTortoise,
                NPCID.HoppinJack,
                NPCID.Mimic,
                NPCID.IlluminantBat,
                NPCID.IlluminantSlime,
                NPCID.JungleCreeper,
                NPCID.JungleCreeperWall,
                NPCID.DesertLamiaDark,
                NPCID.DesertLamiaLight,
                NPCID.BigMossHornet,
                NPCID.GiantMossHornet,
                NPCID.LittleMossHornet,
                NPCID.MossHornet,
                NPCID.TinyMossHornet,
                NPCID.Moth,
                NPCID.PigronCorruption,
                NPCID.PigronCrimson,
                NPCID.PigronHallow,
                NPCID.Pixie,
                NPCID.PossessedArmor,
                NPCID.RockGolem,
                NPCID.DesertScorpionWalk,
                NPCID.DesertScorpionWall,
                NPCID.Slimer,
                NPCID.Slimer2,
                NPCID.ToxicSludge,
                NPCID.Unicorn,
                NPCID.WanderingEye,
                NPCID.Werewolf,
                NPCID.Wolf,
                NPCID.SeekerHead,
                NPCID.Wraith,
                NPCID.ChatteringTeethBomb,
                NPCID.IceGolem,
                NPCID.RainbowSlime,
                NPCID.SandShark,
                NPCID.SandsharkCorrupt,
                NPCID.SandsharkCrimson,
                NPCID.SandsharkHallow,
                NPCID.ShadowFlameApparition,
                NPCID.Parrot,
                NPCID.PirateCorsair,
                NPCID.PirateDeckhand,
                NPCID.PirateGhost,
                NPCID.BlueArmoredBonesMace,
                NPCID.BlueArmoredBonesSword,
                NPCID.BoneLee,
                NPCID.DungeonSpirit,
                NPCID.FlyingSnake,
                NPCID.HellArmoredBones,
                NPCID.HellArmoredBonesSpikeShield,
                NPCID.HellArmoredBonesSword,
                NPCID.MisterStabby,
                NPCID.Butcher,
                NPCID.CreatureFromTheDeep,
                NPCID.DeadlySphere,
                NPCID.Frankenstein,
                NPCID.Fritz,
                NPCID.Psycho,
                NPCID.Reaper,
                NPCID.SwampThing,
                NPCID.ThePossessed,
                NPCID.Vampire,
                NPCID.VampireBat,
                NPCID.HeadlessHorseman,
                NPCID.Hellhound,
                NPCID.Poltergeist,
                NPCID.Scarecrow1,
                NPCID.Scarecrow2,
                NPCID.Scarecrow3,
                NPCID.Scarecrow4,
                NPCID.Scarecrow5,
                NPCID.Scarecrow6,
                NPCID.Scarecrow7,
                NPCID.Scarecrow8,
                NPCID.Scarecrow9,
                NPCID.Scarecrow10,
                NPCID.Splinterling,
                NPCID.Flocko,
                NPCID.GingerbreadMan,
                NPCID.Krampus,
                NPCID.Nutcracker,
                NPCID.NutcrackerSpinning,
                NPCID.PresentMimic,
                NPCID.Yeti,
                NPCID.ZombieElf,
                NPCID.ZombieElfBeard,
                NPCID.ZombieElfGirl,
                NPCID.BloodEelHead,
                NPCID.GoblinShark,
                NPCID.EyeballFlyingFish,
                NPCID.ZombieMerman
            };

            BoundNPCIDs = new List<int>
            {
                NPCID.BoundGoblin,
                NPCID.BoundWizard,
                NPCID.BoundMechanic,
                NPCID.SleepingAngler,
                NPCID.BartenderUnconscious,
                NPCID.WebbedStylist,
                NPCID.GolferRescue
            };

            // Collections
            BossRushHPChanges = new SortedDictionary<int, int>
            {
                // Tier 1
                { NPCID.KingSlime, 300000 }, // 30 seconds
                { NPCID.BlueSlime, 3600 },
                { NPCID.SlimeSpiked, 7200 },
                { NPCID.GreenSlime, 2700 },
                { NPCID.RedSlime, 5400 },
                { NPCID.PurpleSlime, 7200 },
                { NPCID.YellowSlime, 6300 },
                { NPCID.IceSlime, 4500 },
                { NPCID.UmbrellaSlime, 5400 },
                { NPCID.RainbowSlime, 30000 },
                { NPCID.Pinky, 15000 },
                { NPCType<KingSlimeJewelRuby>(), 21000 },
                { NPCType<KingSlimeJewelSapphire>(), 18000 },
                { NPCType<KingSlimeJewelEmerald>(), 24000 },

                { NPCID.EyeofCthulhu, 450000 }, // 30 seconds
                { NPCID.ServantofCthulhu, 6000 },
                { NPCType<BloodlettingServant>(), 12000 },

                { NPCID.EaterofWorldsHead, 10000 }, // 30 seconds + immunity timer at start
                { NPCID.EaterofWorldsBody, 10000 },
                { NPCID.EaterofWorldsTail, 10000 },

                { NPCID.BrainofCthulhu, 100000 }, // 30 seconds with creepers
                { NPCID.Creeper, 10000 },

                { NPCID.QueenBee, 315000 }, // 30 seconds
                { NPCID.Bee, 3000 },
                { NPCID.BeeSmall, 2000 },
                { NPCID.BigHornetHoney, 10000 },
                { NPCID.HornetHoney, 7500 },
                { NPCID.LittleHornetHoney, 5000 },

                { NPCID.Deerclops, 315000 }, // 30 seconds

                { NPCID.SkeletronHead, 150000 }, // 30 seconds
                { NPCID.SkeletronHand, 60000 },

                { NPCID.WallofFlesh, 450000 }, // 30 seconds
                { NPCID.WallofFleshEye, 450000 },
                { NPCID.TheHungry, 10000 },
                { NPCID.TheHungryII, 5000 },
                { NPCID.LeechHead, 5000 },
                { NPCID.LeechBody, 5000 },
                { NPCID.LeechTail, 5000 },

                // Tier 2
                { NPCID.QueenSlimeBoss, 200000 }, // 30 seconds
                { NPCID.QueenSlimeMinionBlue, 6000 },
                { NPCID.QueenSlimeMinionPink, 6000 },
                { NPCID.QueenSlimeMinionPurple, 5000 },

                { NPCID.Spazmatism, 150000 }, // 30 seconds
                { NPCID.Retinazer, 125000 },
                { NPCType<Foveanator>(), 137500 },

                { NPCID.TheDestroyer, 250000 }, // 30 seconds + immunity timer at start
                { NPCID.TheDestroyerBody, 250000 },
                { NPCID.TheDestroyerTail, 250000 },
                { NPCID.Probe, 5000 },

                { NPCID.SkeletronPrime, 160000 }, // 30 seconds
                { NPCType<SkeletronPrime2>(), 160000 },
                { NPCID.PrimeVice, 54000 },
                { NPCID.PrimeCannon, 45000 },
                { NPCID.PrimeSaw, 45000 },
                { NPCID.PrimeLaser, 38000 },

                { NPCID.Plantera, 160000 }, // 30 seconds
                { NPCID.PlanterasTentacle, 5000 },
                { NPCType<PlanterasFreeTentacle>(), 5000 },

                // Tier 3
                { NPCID.Golem, 50000 }, // 30 seconds
                { NPCID.GolemHead, 30000 },
                { NPCID.GolemHeadFree, 30000 },
                { NPCID.GolemFistLeft, 25000 },
                { NPCID.GolemFistRight, 25000 },

                { NPCID.HallowBoss, 200000 }, // 30 seconds

                { NPCID.DukeFishron, 290000 }, // 30 seconds

                { NPCID.CultistBoss, 220000 }, // 30 seconds
                { NPCID.CultistDragonHead, 60000 },
                { NPCID.CultistDragonBody1, 60000 },
                { NPCID.CultistDragonBody2, 60000 },
                { NPCID.CultistDragonBody3, 60000 },
                { NPCID.CultistDragonBody4, 60000 },
                { NPCID.CultistDragonTail, 60000 },
                { NPCID.AncientCultistSquidhead, 50000 },

                { NPCID.MoonLordCore, 160000 }, // 1 minute
                { NPCID.MoonLordHand, 45000 },
                { NPCID.MoonLordHead, 60000 },
                { NPCID.MoonLordLeechBlob, 800 }

                // 9.5 minutes in total for vanilla Boss Rush bosses
            };

            // NOTE: This does not account for Calamity's base value increases
            BossValues = new SortedDictionary<int, int>
            {
                { NPCID.KingSlime, Item.buyPrice(0, 2) },
                { NPCID.EyeofCthulhu, Item.buyPrice(0, 2) },
                // Evil bosses drop 5 gold in vanilla; unmodified
                { NPCID.QueenBee, Item.buyPrice(0, 8) },
                { NPCID.Deerclops, Item.buyPrice(0, 8) },
                { NPCID.SkeletronHead, Item.buyPrice(0, 12) },
                { NPCID.WallofFlesh, Item.buyPrice(0, 12) },
                { NPCID.QueenSlimeBoss, Item.buyPrice(0, 16) },
                { NPCID.Spazmatism, Item.buyPrice(0, 16) },
                { NPCID.Retinazer, Item.buyPrice(0, 16) },
                { NPCID.TheDestroyer, Item.buyPrice(0, 16) },
                { NPCID.SkeletronPrime, Item.buyPrice(0, 16) },
                { NPCID.Plantera, Item.buyPrice(0, 20) },
                { NPCID.Golem, Item.buyPrice(0, 25) },
                { NPCID.HallowBoss, Item.buyPrice(0, 30) },
                { NPCID.DukeFishron, Item.buyPrice(0, 30) },
                { NPCID.CultistBoss, Item.buyPrice(0, 50) }
                // Moon Lord drops 1 plat in vanilla; unmodified
            };

            bossTypes = new SortedDictionary<int, int>()
            {
                { NPCID.KingSlime, 1 },
                { NPCType<DesertScourgeHead>(), 2 },
                { NPCID.EyeofCthulhu, 3 },
                { NPCType<Crabulon>(), 4 },
                { NPCID.EaterofWorldsHead, 5 },
                { NPCID.EaterofWorldsBody, 5 },
                { NPCID.EaterofWorldsTail, 5 },
                { NPCID.BrainofCthulhu, 6 },
                { NPCType<HiveMind>(), 7 },
                { NPCType<PerforatorHive>(), 8 },
                { NPCID.QueenBee, 9 },
                { NPCID.SkeletronHead, 10 },
                { NPCType<SlimeGodCore>(), 11 },
                { NPCType<SplitEbonianPaladin>(), 11 },
                { NPCType<SplitCrimulanPaladin>(), 11 },
                { NPCID.WallofFlesh, 12 },
                { NPCType<Cryogen>(), 13 },
                { NPCID.Retinazer, 14 },
                { NPCID.Spazmatism, 14 },
                { NPCType<AquaticScourgeHead>(), 15 },
                { NPCID.TheDestroyer, 16 },
                { NPCType<BrimstoneElemental>(), 17 },
                { NPCID.SkeletronPrime, 18 },
                { NPCType<CalamitasClone>(), 19 },
                { NPCID.Plantera, 20 },
                { NPCType<Leviathan>(), 21 },
                { NPCType<Anahita>(), 21 },
                { NPCType<AstrumAureus>(), 22 },
                { NPCID.Golem, 23 },
                { NPCType<PlaguebringerGoliath>(), 24 },
                { NPCID.DukeFishron, 25 },
                { NPCType<RavagerBody>(), 26 },
                { NPCID.CultistBoss, 27 },
                { NPCType<AstrumDeusHead>(), 28 },
                { NPCID.MoonLordCore, 29 },
                { NPCType<ProfanedGuardianCommander>(), 30 },
                { NPCType<Bumblefuck>(), 31 },
                { NPCType<Providence>(), 32 },
                { NPCType<CeaselessVoid>(), 33 },
                { NPCType<StormWeaverHead>(), 34 },
                { NPCType<Signus>(), 35 },
                { NPCType<Polterghast>(), 36 },
                { NPCType<OldDuke>(), 37 },
                { NPCType<DevourerofGodsHead>(), 38 },
                { NPCType<Yharon>(), 39 },
                { NPCType<SupremeCalamitas>(), 40 },
                { NPCType<AresBody>(), 41 },
                { NPCType<ThanatosHead>(), 41 },
                { NPCType<Artemis>(), 41 },
                { NPCType<Apollo>(), 41 },
                { NPCID.QueenSlimeBoss, 42 },
                { NPCID.HallowBoss, 43 },
                { NPCID.Deerclops, 44 }
            };

            legOverrideList = new List<int>()
            {
                EquipLoader.GetEquipSlot(CalamityMod.Instance, "ProfanedSoulCrystal", EquipType.Legs),
                EquipLoader.GetEquipSlot(CalamityMod.Instance, "AquaticHeart", EquipType.Legs),
                //CalamityMod.Instance.GetEquipSlot("SirenLeg", EquipType.Legs), whate even was SirenLeg vs SirenLegAlt?
                EquipLoader.GetEquipSlot(CalamityMod.Instance, "Popo", EquipType.Legs)
            };

            // Duke Fishron and Old Duke phase 3 becomes way too easy if you can make him stop being invisible with Yanmei's Knife.
            // This is a list so that other NPCs can be added as necessary.
            // IT DOES NOT make them immune to the debuff, just stops them from being recolored.
            kamiDebuffColorImmuneList = new List<int>()
            {
                NPCID.DukeFishron,
                NPCType<OldDuke>()
            };

            EncryptedSchematicIDRelationship = new Dictionary<int, int>()
            {
                [1] = ItemType<EncryptedSchematicPlanetoid>(),
                [2] = ItemType<EncryptedSchematicJungle>(),
                [3] = ItemType<EncryptedSchematicHell>(),
                [4] = ItemType<EncryptedSchematicIce>(),
            };

            DisabledSummonerNerfItems = new();
            DisabledSummonerNerfMinions = new();

            VeneratedLocketBanlist = new List<int>()
            {
                ItemType<PoisonPack>(),
                ItemType<SkyStabber>(),
                ItemType<Nychthemeron>(),
                ItemType<HellsSun>(),
                ItemType<GodsParanoia>(),
                ItemType<SlickCane>(),
                ItemType<Mycoroot>(),
                ItemType<CosmicKunai>()
            };

            SunkenSeaBiomeCorrespondentValues = new()
            {
                { SunkenSeaBiomeFlags.UndergroundDesert, (spawnInfo => spawnInfo.Player.ZoneDesert, -1 /* None needed. */) },
                { SunkenSeaBiomeFlags.TimelessShores, (spawnInfo => spawnInfo.Player.Calamity().ZoneTimelessShores, GetInstance<TimelessShoresBiome>().Type) },
                { SunkenSeaBiomeFlags.RadiantReefs, (spawnInfo => spawnInfo.Player.Calamity().ZoneRadiantReefs, GetInstance<RadiantReefsBiome>().Type) },
                { SunkenSeaBiomeFlags.PolypForest, (spawnInfo => spawnInfo.Player.Calamity().ZonePolypForest, GetInstance<PolypForestBiome>().Type) },
                { SunkenSeaBiomeFlags.GleamingBurrows, (spawnInfo => spawnInfo.Player.Calamity().ZoneGleamingBurrows, GetInstance<GleamingBurrowsBiome>().Type) },
                { SunkenSeaBiomeFlags.BasaltGully, (spawnInfo => spawnInfo.Player.Calamity().ZoneBasaltGully, GetInstance<BasaltGullyBiome>().Type) },
            };
        }

        public override void Unload()
        {
            hornetList = null;
            mossHornetList = null;
            minibossList = null;
            pierceResistList = null;
            pierceResistExceptionLeviAureusList = null;
            pierceResistExceptionList = null;

            AstrumDeusIDs = null;
            DevourerOfGodsIDs = null;
            CosmicGuardianIDs = null;
            AquaticScourgeIDs = null;
            PerforatorIDs = null;
            DesertScourgeIDs = null;
            EaterofWorldsIDs = null;
            SlimeGodIDs = null;
            DeathModeSplittingWormIDs = null;
            DestroyerIDs = null;
            ThanatosIDs = null;
            AresIDs = null;
            SkeletronPrimeIDs = null;
            StormWeaverIDs = null;
            RavagerIDs = null;
            GolemIDs = null;
            BoundNPCIDs = null;
            GrenadeResistIDs = null;
            ZeroContactDamageNPCList = null;
            HardmodeNPCNerfList = null;

            BossRushHPChanges?.Clear();
            BossRushHPChanges = null;
            BossValues?.Clear();
            BossValues = null;
            bossTypes?.Clear();
            bossTypes = null;

            legOverrideList = null;

            kamiDebuffColorImmuneList = null;

            EncryptedSchematicIDRelationship = null;

            DisabledSummonerNerfItems = null;
            DisabledSummonerNerfMinions = null;

            VeneratedLocketBanlist = null;

            SunkenSeaBiomeCorrespondentValues = null;
        }
    }
}
