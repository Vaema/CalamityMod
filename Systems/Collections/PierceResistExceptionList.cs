using System.Collections.Generic;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Melee.MaceFlails;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class PierceResistExceptionList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
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
                ProjectileType<AcidRocket>(),
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
                ProjectileType<TaserHook>(),
                ProjectileType<Teslabeam>(),
                ProjectileType<TyphonsGreedStaff>(),
                ProjectileType<UrchinMaceProjectile>(),
                ProjectileType<UrchinStingerProj>(),
                ProjectileType<ViolenceThrownProjectile>(),
                ProjectileType<WaterLeechProj>(),
                ProjectileType<YateveoBloomMace>(),
                ProjectileType<YharimsCrystalBeam>(),
            ];
        }

        public override void Unload() => List = null;

        public static bool IsException(Projectile projectile) => List.Contains(projectile.type);
    }
}
