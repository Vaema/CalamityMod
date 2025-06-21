using System.Collections.Generic;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
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
using CalamityMod.NPCs.SupremeCalamitas;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> that has all NPC IDs of bosses or boss minions that have HP scaling.
    /// </summary>
    public sealed class BossHPScalingList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.EaterofWorldsHead,
                NPCID.EaterofWorldsBody,
                NPCID.EaterofWorldsTail,
                NPCID.SkeletronHand,
                NPCID.WallofFleshEye,
                NPCID.TheDestroyerBody,
                NPCID.TheDestroyerTail,
                NPCID.PrimeCannon,
                NPCID.PrimeLaser,
                NPCID.PrimeVice,
                NPCID.PrimeSaw,
                NPCID.GolemHead,
                NPCID.GolemHeadFree,
                NPCID.GolemFistRight,
                NPCID.GolemFistLeft,
                NPCID.Sharkron,
                NPCID.Sharkron2,
                NPCID.MoonLordHead,
                NPCID.MoonLordHand,
                NPCType<DarkEnergy>(),
                NPCType<BrimstoneHeart>(),
                NPCType<SoulSeeker>(),
                NPCType<SoulSeekerSupreme>(),
                NPCType<Cataclysm>(),
                NPCType<SupremeCataclysm>(),
                NPCType<Catastrophe>(),
                NPCType<SupremeCatastrophe>(),
                NPCType<SepulcherHead>(),
                NPCType<SepulcherBody>(),
                NPCType<SepulcherTail>(),
                NPCType<SepulcherArm>(),
                NPCType<SepulcherBodyEnergyBall>(),
                NPCType<PrimordialWyrmBody>(),
                NPCType<PrimordialWyrmBodyAlt>(),
                NPCType<PrimordialWyrmHead>(),
                NPCType<PrimordialWyrmTail>(),
                NPCType<AquaticAberration>(),
                NPCType<AnahitasIceShield>(),
                NPCType<CryogenShield>(),
                NPCType<OldDukeToothBall>(),
                NPCType<SulphurousSharkron>(),
                NPCType<DraconicSwarmer>(),
                NPCType<AureusSpawn>(),
                NPCType<Brimling>(),
                NPCType<CrabShroom>(),
                NPCType<CosmicGuardianBody>(),
                NPCType<CosmicGuardianTail>(),
                NPCType<CosmicGuardianHead>(),
                NPCType<DankCreeper>(),
                NPCType<HiveBlob>(),
                NPCType<HiveBlob2>(),
                NPCType<DarkHeart>(),
                NPCType<DesertNuisanceBody>(),
                NPCType<DesertNuisanceHead>(),
                NPCType<DesertNuisanceTail>(),
                NPCType<DesertNuisanceBodyYoung>(),
                NPCType<DesertNuisanceHeadYoung>(),
                NPCType<DesertNuisanceTailYoung>(),
                NPCType<PolterPhantom>(),
                NPCType<PhantomFuckYou>(),
                NPCType<BloodlettingServant>(),
                NPCType<KingSlimeJewelEmerald>(),
                NPCType<KingSlimeJewelRuby>(),
                NPCType<KingSlimeJewelSapphire>(),
                NPCType<PlanterasFreeTentacle>(),
                NPCType<PlagueHomingMissile>(),
                NPCType<PlagueMine>(),
                NPCType<ProfanedRocks>(),
                NPCType<ProvSpawnDefense>(),
                NPCType<ProvSpawnOffense>(),
                NPCType<ProvSpawnHealer>(),
                NPCType<RockPillar>(),
                NPCType<FlamePillar>(),
                NPCType<CosmicMine>(),
                NPCType<CosmicLantern>(),
                NPCType<ProfanedGuardianDefender>(),
                NPCType<ProfanedGuardianHealer>(),
                NPCType<CorruptSlimeSpawn>(),
                NPCType<CorruptSlimeSpawn2>(),
                NPCType<CrimsonSlimeSpawn>(),
                NPCType<CrimsonSlimeSpawn2>(),

                NPCType<PerforatorHeadLarge>(),
                NPCType<PerforatorBodyLarge>(),
                NPCType<PerforatorTailLarge>(),
                NPCType<PerforatorHeadMedium>(),
                NPCType<PerforatorBodyMedium>(),
                NPCType<PerforatorTailMedium>(),
                NPCType<PerforatorHeadSmall>(),
                NPCType<PerforatorBodySmall>(),
                NPCType<PerforatorTailSmall>(),

                NPCType<EbonianPaladin>(),
                NPCType<CrimulanPaladin>(),
                NPCType<SplitEbonianPaladin>(),
                NPCType<SplitCrimulanPaladin>(),
                NPCType<SlimeGodCore>(),

                NPCType<RavagerBody>(),
                NPCType<RavagerClawLeft>(),
                NPCType<RavagerClawRight>(),
                NPCType<RavagerLegLeft>(),
                NPCType<RavagerLegRight>(),
                NPCType<RavagerHead>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
