using System.Collections.Generic;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Melee.MaceFlails;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that holds a list of all projectiles that should be, exceptionally, ignored by projectile destruction.
    /// </summary>
    public sealed class ProjectileDestroyExceptionsList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                // Holdout projectiles.
                ProjectileID.Phantasm,
                ProjectileID.VortexBeater,
                ProjectileID.DD2PhoenixBow,
                ProjectileID.LastPrism,
                ProjectileID.LastPrismLaser,
                ProjectileID.LaserMachinegun,
                ProjectileID.ChargedBlasterCannon,
                ProjectileID.MedusaHead,

                ProjectileType<UrchinMaceProj>(),
                ProjectileType<BrokenBiomeBladeHoldout>(),
                ProjectileType<AridGrandeur>(),
                ProjectileType<BitingEmbrace>(),
                ProjectileType<DecaysRetort>(),
                ProjectileType<BiomeBladeHoldout>(),
                ProjectileType<TrueAridGrandeur>(),
                ProjectileType<TrueBitingEmbrace>(),
                ProjectileType<TrueDecaysRetort>(),
                ProjectileType<TrueGrovetendersTouch>(),
                ProjectileType<HeavensMight>(),
                ProjectileType<HellbornHoldout>(),
                ProjectileType<TrueBiomeBladeHoldout>(),
                ProjectileType<LamentationsOfTheChained>(),
                ProjectileType<ChainedMeatHook>(),
                ProjectileType<SwordsmithsPride>(),
                ProjectileType<SanguineFury>(),
                ProjectileType<EarthenTides>(),
                ProjectileType<GalaxiaHoldout>(),
                ProjectileType<PhoenixsPride>(),
                ProjectileType<PolarisGaze>(),
                ProjectileType<AndromedasStride>(),
                ProjectileType<AriesWrath>(),
                ProjectileType<ArkoftheAncientsSwungBlade>(),
                ProjectileType<ArkoftheAncientsParryHoldout>(),
                ProjectileType<TrueArkoftheAncientsSwungBlade>(),
                ProjectileType<TrueArkoftheAncientsParryHoldout>(),
                ProjectileType<ArkoftheElementsSwungBlade>(),
                ProjectileType<ArkoftheElementsParryHoldout>(),
                ProjectileType<ArkoftheCosmosSwungBlade>(),
                ProjectileType<ArkoftheCosmosParryHoldout>(),
                ProjectileType<BasherHoldout>(),
                ProjectileType<OldLordClaymoreHoldout>(),
                ProjectileType<GrandDadHoldout>(),
                ProjectileType<GrandGuardianHoldout>(),
                ProjectileType<EarthHoldout>(),
                ProjectileType<MajesticGuardHoldout>(),
                ProjectileType<HellkiteHoldout>(),
                ProjectileType<RiftburstBow>(),
                ProjectileType<CometQuasherHoldout>(),
                ProjectileType<StellarStrikerHoldout>(),
                ProjectileType<ContagionBow>(),
                ProjectileType<DaemonsFlameBow>(),
                ProjectileType<DrataliornusBow>(),
                ProjectileType<FlakKrakenHoldout>(),
                ProjectileType<BuzzkillHoldout>(),
                ProjectileType<StarfleetMK2Gun>(),
                ProjectileType<SuperradiantSlaughtererHoldout>(),
                ProjectileType<NorfleetCannon>(),
                ProjectileType<FlurrystormCannonShooting>(),
                ProjectileType<ChickenCannonHeld>(),
                ProjectileType<PumplerHoldout>(),
                ProjectileType<ClockworkBowHoldout>(),
                ProjectileType<UltimaBowProjectile>(),
                ProjectileType<CondemnationHoldout>(),
                ProjectileType<SurgeDriverHoldout>(),
                ProjectileType<StarmageddonHeld>(),

                ProjectileType<NanoPurgeHoldout>(),
                ProjectileType<AetherfluxCannonHoldout>(),
                ProjectileType<YharimsCrystalPrism>(),
                ProjectileType<DarkSparkPrism>(),
                ProjectileType<YharimsCrystalBeam>(),
                ProjectileType<DarkSparkBeam>(),
                ProjectileType<GhastlyVisageProj>(),
                ProjectileType<ApotheosisWorm>(),
                ProjectileType<SpiritCongregation>(),
                ProjectileType<RancorLaserbeam>(),
                ProjectileType<NebulousCataclysm_Held>(),

                ProjectileType<FlakKrakenProjectile>(),
                ProjectileType<InfernadoFriendly>(),
                ProjectileType<DragonRageStaff>(),
                ProjectileType<MurasamaSlash>(),
                ProjectileType<PhaseslayerProjectile>(),
                ProjectileType<TaintedBladeSlasher>(),
                ProjectileType<PhotonRipperProjectile>(),
                ProjectileType<SpineOfThanatosProjectile>(),

                ProjectileType<FinalDawnProjectile>(),
                ProjectileType<FinalDawnThrow>(),
                ProjectileType<FinalDawnHorizontalSlash>(),
                ProjectileType<FinalDawnFireSlash>(),

                // Some hostile boss projectiles.
                ProjectileID.SaucerDeathray,
                ProjectileID.PhantasmalDeathray,

                ProjectileType<BrimstoneMonster>(),
                ProjectileType<InfernadoRevenge>(),
                ProjectileType<OverlyDramaticDukeSummoner>(),
                ProjectileType<ProvidenceHolyRay>(),
                ProjectileType<OldDukeVortex>(),
                ProjectileType<BrimstoneRay>(),
                ProjectileType<AresDeathBeamStart>(),
                ProjectileType<AresGaussNukeProjectileBoom>(),
                ProjectileType<AresLaserBeamStart>(),
                ProjectileType<ArtemisSpinLaserbeam>(),
                ProjectileType<BirbAura>(),
                ProjectileType<ThanatosBeamStart>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if a projectile belongs in the exception list.
        /// </summary>
        public static bool Includes(int projType) => List.Contains(projType);
    }
}
