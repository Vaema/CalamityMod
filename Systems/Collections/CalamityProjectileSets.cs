using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee.MaceFlails;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    [ReinitializeDuringResizeArrays]
    public static class CalamityProjectileSets
    {
        public static SetFactory Factory = new SetFactory(ProjectileLoader.ProjectileCount, "CalamityMod/ProjectileID", Search);
        public static IdDictionary Search = IdDictionary.Create<ProjectileID, int>();

        /// <summary>
        /// If <see langword="true"/> for a projectile type, then that minion will completely ignore Calamity's summon damage penalty mechanic with no exceptions.<br/>
        /// Unused by Calamity itself, and is only used for external mods to add to through mod calls.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] MinionWhichIgnoresSummonerNerf = Factory.CreateBoolSet();

        /// <summary>
        /// If <see langword="true"/> for a projectile type, then that projectile is spawned by a post-Plantera Dungeon enemy.<br/>
        /// This increases the projectile's damage by a flat 30 if Moon Lord has been defeated.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] IsBuffedDungeonProjectile = Factory.CreateBoolSet(ProjectileID.PaladinsHammerHostile, ProjectileID.ShadowBeamHostile, ProjectileID.InfernoHostileBolt,
                ProjectileID.InfernoHostileBlast, ProjectileID.LostSoulHostile, ProjectileID.SniperBullet, ProjectileID.RocketSkeleton, ProjectileID.BulletDeadeye, ProjectileID.Shadowflames);

        /// <summary>
        /// If <see langword="true"/> for a projectile type, then that projectile is spawned by a Solar Eclipse, Pumpkin Moon, or Frost Moon enemy.<br/>
        /// This increases the projectile's damage by a flat 15 during those events if Devourer of Gods has been defeated.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] IsBuffedEventProjectile = Factory.CreateBoolSet(ProjectileID.FlamingWood, ProjectileID.GreekFire1, ProjectileID.GreekFire2, ProjectileID.GreekFire3,
                ProjectileID.FlamingScythe, ProjectileID.FlamingArrow, ProjectileID.PineNeedleHostile, ProjectileID.OrnamentHostile, ProjectileID.OrnamentHostileShrapnel,
                ProjectileID.FrostWave, ProjectileID.FrostShard, ProjectileID.Missile, ProjectileID.Present, ProjectileID.Spike, ProjectileID.BulletDeadeye, ProjectileID.EyeLaser,
                ProjectileID.Nail, ProjectileID.DrManFlyFlask);

        /// <summary>
        /// If <see langword="true"/> for a projectile type, then that projectile is considered a friendly bee.<br/>
        /// Used to allow the projectile to inflict Plague while wearing the Plaguebringer Carapace.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] IsFriendlyBeeProjectile = Factory.CreateBoolSet(ProjectileID.GiantBee, ProjectileID.Bee, ProjectileID.Wasp, ProjectileType<PlaguenadeBee>(),
                ProjectileType<PlaguePrincess>(), ProjectileType<BabyPlaguebringer>(), ProjectileType<PlagueBeeSmall>());

        /// <summary>
        /// If <see langword="true"/> for a projectile type, then that projectile is a bomb or other explosive which is not a weapon.<br/>
        /// Used to provide early-game worm bosses a resistance to their explosive damage.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] ResistedExplosiveProjectile = Factory.CreateBoolSet(ProjectileID.Grenade, ProjectileID.StickyGrenade, ProjectileID.BouncyGrenade, ProjectileID.Bomb,
                ProjectileID.StickyBomb, ProjectileID.BouncyBomb, ProjectileID.Dynamite, ProjectileID.StickyDynamite, ProjectileID.BouncyDynamite, ProjectileID.Explosives,
                ProjectileID.ExplosiveBunny, ProjectileID.PartyGirlGrenade, ProjectileID.BombFish, ProjectileID.ScarabBomb, ProjectileID.TNTBarrel, ProjectileType<AeroExplosive>());

        /// <summary>
        /// If <see langword="true"/> for a projectile type, then that projectile will never be reflected by armor or accessory effects.<br/>
        /// Set this for persistent projectiles such as deathrays to avoid major screwing of their behavior.<br/>
        /// Only needs to be set for hostile projectiles, as these effects already have a check to ensure they never trigger in PvP.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] ShouldNotBeReflected = Factory.CreateBoolSet(ProjectileID.SaucerDeathray, ProjectileID.PhantasmalDeathray, ProjectileType<BrimstoneMonster>(),
                ProjectileType<InfernadoRevenge>(), ProjectileType<OverlyDramaticDukeSummoner>(), ProjectileType<ProvidenceHolyRay>(), ProjectileType<OldDukeVortex>(),
                ProjectileType<BrimstoneRay>(), ProjectileType<AresDeathBeamStart>(), ProjectileType<AresGaussNukeProjectileBoom>(), ProjectileType<AresLaserBeamStart>(),
                ProjectileType<ArtemisSpinLaserbeam>(), ProjectileType<BirbAura>(), ProjectileType<ThanatosBeamStart>());
    }
}
