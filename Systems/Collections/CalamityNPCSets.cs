using System;
using System.Collections.Generic;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.Astral;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    [ReinitializeDuringResizeArrays]
    public static class CalamityNPCSets
    {
        public static SetFactory Factory = new SetFactory(NPCLoader.NPCCount, "CalamityMod/NPCID", Search);
        public static IdDictionary Search = IdDictionary.Create<NPCID, int>();

        /// <summary>
        /// If <see langword="true"/> for an NPC type, makes this NPC be susceptible to <see cref="BuffID.Confused"/>.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] CalamityNPCNotImmuneToConfused = Factory.CreateBoolSet(NPCType<AeroSlime>(), NPCType<AstralachneaGround>(), NPCType<AstralachneaWall>(), NPCType<BloomSlime>(),
                NPCType<Bohldohr>(), NPCType<CalamityEye>(), NPCType<CrimulanBlightSlime>(), NPCType<Cryon>(), NPCType<CryoSlime>(), NPCType<DespairStone>(), NPCType<EbonianBlightSlime>(),
                NPCType<FearlessGoldfishWarrior>(), NPCType<HeatSpirit>(), NPCType<MantisShrimp>(), NPCType<OverloadedSoldier>(), NPCType<PerennialSlime>(), NPCType<RenegadeWarlock>(),
                NPCType<Rimehound>(), NPCType<Rotdog>(), NPCType<Scryllar>(), NPCType<ScryllarRage>(), NPCType<StellarCulex>(), NPCType<Stormlion>(), NPCType<SuperDummyNPC>(),
                NPCType<WulfrumGyrator>(), NPCType<WulfrumRover>());

        /// <summary>
        /// If <see langword="true"/> for an NPC type, forces this NPC to draw Calamity's debuff display, even if it is not a boss.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] ForceDrawDebuffDisplay = Factory.CreateBoolSet(NPCID.TargetDummy, NPCID.WallofFleshEye, NPCType<SuperDummyNPC>());

        /// <summary>
        /// If <see langword="true"/> for an NPC type, prevents Kami Flu's green color filter from being drawn on this NPC.<br/>
        /// This does NOT grant immunity to the debuff, only prevents its special color drawing.<br/>
        /// Used to prevent cheesing of Duke boss phase 3s by negating invisibility.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] DoesNotDrawKamiFluDebuffColor = Factory.CreateBoolSet(NPCID.DukeFishron, NPCType<OldDuke>());

        /// <summary>
        /// If <see langword="true"/> for an NPC type, allows the NPC to scale its health based on the Boss Health Boost Percentage config option, even if it is not a boss.<br/>
        /// Also allows the NPC to receive health scaling from having multiple players in Expert+.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] ScalesHealthLikeBoss = Factory.CreateBoolSet(NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail, NPCID.SkeletronHand,
                NPCID.WallofFleshEye, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeVice, NPCID.PrimeSaw, NPCID.GolemHead,
                NPCID.GolemHeadFree, NPCID.GolemFistRight, NPCID.GolemFistLeft, NPCID.Sharkron, NPCID.Sharkron2, NPCID.MoonLordHead, NPCID.MoonLordHand, NPCType<DarkEnergy>(),
                NPCType<BrimstoneHeart>(), NPCType<SoulSeeker>(), NPCType<SoulSeekerSupreme>(), NPCType<Cataclysm>(), NPCType<SupremeCataclysm>(), NPCType<Catastrophe>(),
                NPCType<SupremeCatastrophe>(), NPCType<SepulcherHead>(), NPCType<SepulcherBody>(), NPCType<SepulcherTail>(), NPCType<SepulcherArm>(), NPCType<SepulcherBodyEnergyBall>(),
                NPCType<PrimordialWyrmBody>(), NPCType<PrimordialWyrmBodyAlt>(), NPCType<PrimordialWyrmHead>(), NPCType<PrimordialWyrmTail>(), NPCType<AquaticAberration>(),
                NPCType<AnahitasIceShield>(), NPCType<CryogenShield>(), NPCType<OldDukeToothBall>(), NPCType<SulphurousSharkron>(), NPCType<Bumblefuck2>(), NPCType<AureusSpawn>(),
                NPCType<Brimling>(), NPCType<CrabShroom>(), NPCType<CosmicGuardianBody>(), NPCType<CosmicGuardianTail>(), NPCType<CosmicGuardianHead>(), NPCType<DankCreeper>(),
                NPCType<HiveBlob>(), NPCType<HiveBlob2>(), NPCType<DarkHeart>(), NPCType<DesertNuisanceBody>(), NPCType<DesertNuisanceHead>(), NPCType<DesertNuisanceTail>(),
                NPCType<DesertNuisanceBodyYoung>(), NPCType<DesertNuisanceHeadYoung>(), NPCType<DesertNuisanceTailYoung>(), NPCType<PolterPhantom>(), NPCType<PhantomFuckYou>(),
                NPCType<BloodlettingServant>(), NPCType<KingSlimeJewelEmerald>(), NPCType<KingSlimeJewelRuby>(), NPCType<KingSlimeJewelSapphire>(), NPCType<PlanterasFreeTentacle>(),
                NPCType<SkeletronPrime2>(), NPCType<PlagueHomingMissile>(), NPCType<PlagueMine>(), NPCType<ProfanedRocks>(), NPCType<ProvSpawnDefense>(), NPCType<ProvSpawnOffense>(),
                NPCType<ProvSpawnHealer>(), NPCType<RockPillar>(), NPCType<FlamePillar>(), NPCType<CosmicMine>(), NPCType<CosmicLantern>(), NPCType<ProfanedGuardianDefender>(),
                NPCType<ProfanedGuardianHealer>(), NPCType<CorruptSlimeSpawn>(), NPCType<CorruptSlimeSpawn2>(), NPCType<CrimsonSlimeSpawn>(), NPCType<CrimsonSlimeSpawn2>(),
                NPCType<PerforatorHeadLarge>(), NPCType<PerforatorBodyLarge>(), NPCType<PerforatorTailLarge>(), NPCType<PerforatorHeadMedium>(), NPCType<PerforatorBodyMedium>(),
                NPCType<PerforatorTailMedium>(), NPCType<PerforatorHeadSmall>(), NPCType<PerforatorBodySmall>(), NPCType<PerforatorTailSmall>(), NPCType<EbonianPaladin>(),
                NPCType<CrimulanPaladin>(), NPCType<SplitEbonianPaladin>(), NPCType<SplitCrimulanPaladin>(), NPCType<SlimeGodCore>(), NPCType<RavagerBody>(), NPCType<RavagerClawLeft>(),
                NPCType<RavagerClawRight>(), NPCType<RavagerLegLeft>(), NPCType<RavagerLegRight>(), NPCType<RavagerHead>());

        /// <summary>
        /// Associates an NPC type with the base value of their max health in Boss Rush.<br/>
        /// If an NPC type is not a key in this dictionary, then it will use its standard max health value.
        /// </summary>
        public static Dictionary<int, int> BossRushHealth = new Dictionary<int, int>
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

            { NPCID.TheDestroyer, 600000 }, // 30 seconds + immunity timer at start
            { NPCID.TheDestroyerBody, 600000 },
            { NPCID.TheDestroyerTail, 600000 },
            { NPCID.Probe, 10000 },

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
        };

        /// <summary>
        /// Associates an NPC type with an ID number used for Calamity's Speedrun Timer. Used for drawing the correct map icon for the boss.<br/>
        /// If an NPC type is not a key in this dictionary, then it will not be displayed on the Speedrun Timer.
        /// </summary>
        public static Dictionary<int, int> BossSpeedrunTimerID = new Dictionary<int, int>
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
    }
}
