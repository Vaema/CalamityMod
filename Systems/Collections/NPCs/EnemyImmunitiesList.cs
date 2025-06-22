using System.Collections.Generic;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains all of the NPC IDs of all NPCs that are immune to special debuffs.
    /// </summary>
    public sealed class EnemyImmunitiesList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.KingSlime,
                NPCType<KingSlimeJewelRuby>(),
                NPCType<KingSlimeJewelSapphire>(),
                NPCType<KingSlimeJewelEmerald>(),
                NPCID.EyeofCthulhu,
                NPCID.EaterofWorldsHead,
                NPCID.EaterofWorldsBody,
                NPCID.EaterofWorldsTail,
                NPCID.BrainofCthulhu,
                NPCID.Creeper,
                NPCID.QueenBee,
                NPCID.Deerclops,
                NPCID.SkeletronHead,
                NPCID.SkeletronHand,
                NPCID.WallofFlesh,
                NPCID.WallofFleshEye,
                NPCID.PirateShipCannon,
                NPCID.QueenSlimeBoss,
                NPCID.Probe,
                NPCID.Retinazer,
                NPCID.Spazmatism,
                NPCID.SkeletronPrime,
                NPCID.PrimeCannon,
                NPCID.PrimeSaw,
                NPCID.PrimeLaser,
                NPCID.PrimeVice,
                NPCID.Plantera,
                NPCID.PlanterasTentacle,
                NPCType<PlanterasFreeTentacle>(),
                NPCID.Everscream,
                NPCID.SantaNK1,
                NPCID.IceQueen,
                NPCID.MourningWood,
                NPCID.Pumpking,
                NPCID.Mothron,
                NPCID.Golem,
                NPCID.GolemHead,
                NPCID.GolemHeadFree,
                NPCID.GolemFistRight,
                NPCID.GolemFistLeft,
                NPCID.MartianSaucerCore,
                NPCID.MartianSaucerCannon,
                NPCID.MartianSaucerTurret,
                NPCID.DukeFishron,
                NPCID.Sharkron,
                NPCID.Sharkron2,
                NPCID.HallowBoss,
                NPCID.CultistBoss,
                NPCID.CultistDragonHead,
                NPCID.CultistDragonBody1,
                NPCID.CultistDragonBody2,
                NPCID.CultistDragonBody3,
                NPCID.CultistDragonBody4,
                NPCID.CultistDragonTail,
                NPCID.AncientCultistSquidhead,
                NPCID.MoonLordHead,
                NPCID.MoonLordHand,
                NPCID.MoonLordCore,
                NPCID.MoonLordFreeEye,
                NPCID.DD2WyvernT1,
                NPCID.DD2WyvernT2,
                NPCID.DD2WyvernT3,
                NPCID.DD2DarkMageT1,
                NPCID.DD2DarkMageT3,
                NPCID.DD2SkeletonT1,
                NPCID.DD2SkeletonT3,
                NPCID.DD2WitherBeastT2,
                NPCID.DD2WitherBeastT3,
                NPCID.DD2DrakinT2,
                NPCID.DD2DrakinT3,
                NPCID.DD2KoboldWalkerT2,
                NPCID.DD2KoboldWalkerT3,
                NPCID.DD2KoboldFlyerT2,
                NPCID.DD2KoboldFlyerT3,
                NPCID.DD2OgreT2,
                NPCID.DD2OgreT3,
                NPCID.DD2Betsy,
                NPCType<DesertNuisanceHead>(),
                NPCType<DesertNuisanceBody>(),
                NPCType<DesertNuisanceTail>(),
                NPCType<DesertNuisanceHeadYoung>(),
                NPCType<DesertNuisanceBodyYoung>(),
                NPCType<DesertNuisanceTailYoung>(),
                NPCType<GiantClam>(),
                NPCType<PerforatorHeadLarge>(),
                NPCType<PerforatorHeadMedium>(),
                NPCType<PerforatorHeadSmall>(),
                NPCType<PerforatorBodyLarge>(),
                NPCType<PerforatorBodyMedium>(),
                NPCType<PerforatorBodySmall>(),
                NPCType<PerforatorTailLarge>(),
                NPCType<PerforatorTailMedium>(),
                NPCType<PerforatorTailSmall>(),
                NPCType<EbonianPaladin>(),
                NPCType<CrimulanPaladin>(),
                NPCType<SplitEbonianPaladin>(),
                NPCType<SplitCrimulanPaladin>(),
                NPCType<Horse>(),
                NPCType<CloudElemental>(),
                NPCType<CryogenShield>(),
                NPCType<AquaticScourgeHead>(),
                NPCType<AquaticScourgeBody>(),
                NPCType<AquaticScourgeBodyAlt>(),
                NPCType<AquaticScourgeTail>(),
                NPCType<CragmawMire>(),
                NPCType<Cataclysm>(),
                NPCType<Catastrophe>(),
                NPCType<SoulSeeker>(),
                NPCType<GreatSandShark>(),
                NPCType<AnahitasIceShield>(),
                NPCType<AureusSpawn>(),
                NPCType<PlaguebringerMiniboss>(),
                NPCType<PlagueHomingMissile>(),
                NPCType<PlagueMine>(),
                NPCType<RavagerClawLeft>(),
                NPCType<RavagerClawRight>(),
                NPCType<RavagerLegLeft>(),
                NPCType<RavagerLegRight>(),
                NPCType<RockPillar>(),
                NPCType<RavagerHead>(),
                NPCType<ProfanedGuardianDefender>(),
                NPCType<ProfanedGuardianHealer>(),
                NPCType<DraconicSwarmer>(),
                NPCType<ProvSpawnDefense>(),
                NPCType<ProvSpawnHealer>(),
                NPCType<ProvSpawnOffense>(),
                NPCType<BobbitWormHead>(),
                NPCType<Mauler>(),
                NPCType<ColossalSquid>(),
                NPCType<ReaperShark>(),
                NPCType<EidolonWyrmHead>(),
                NPCType<NuclearTerror>(),
                NPCType<OldDukeToothBall>(),
                NPCType<SulphurousSharkron>(),
                NPCType<SupremeCataclysm>(),
                NPCType<SupremeCatastrophe>(),
                NPCType<SoulSeekerSupreme>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if an NPC is in this list.
        /// </summary>
        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
