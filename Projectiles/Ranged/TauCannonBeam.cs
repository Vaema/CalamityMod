using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class TauCannonBeam : BaseLaserbeamProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override float Lifetime => IsStage3Laser ? 180 : 25;

        public override float MaxScale => IsStage3Laser ? 4f : 1f;

        public override float MaxLaserLength => 1200f;

        private const string LaserTexturePath = "CalamityMod/ExtraTextures/Lasers/TauCannonBeam";
        public override Texture2D LaserBeginTexture => Request<Texture2D>(LaserTexturePath + "Start").Value;
        public override Texture2D LaserMiddleTexture => Request<Texture2D>(LaserTexturePath + "Middle").Value;
        public override Texture2D LaserEndTexture => Request<Texture2D>(LaserTexturePath + "End").Value;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private Projectile Holdout => Main.projectile[(int)Projectile.ai[1]];

        private bool IsStage3Laser => Projectile.ai[2] == 1f;

        private Player Owner { get; set; }

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, IsStage3Laser ? 70f : 10f, ref _);
        }

        public override void UpdateLaserMotion()
        {
            Projectile.velocity = Holdout.velocity;
            Projectile.rotation = Holdout.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override void AttachToSomething()
        {
            if (Owner == null || !Owner.active || Owner.dead || Owner.CCed || Owner.noItems || Owner.ownedProjectileCounts[ProjectileType<TauCannonHoldout>()] == 0) return;
            Projectile.Center = Holdout.ModProjectile<TauCannonHoldout>().GunTipPosition + Holdout.velocity * (IsStage3Laser ? 20f : 5f);
        }

        public override void ExtraBehavior()
        {
            Owner ??= Main.player[Projectile.owner];
            Projectile.localNPCHitCooldown = IsStage3Laser ? 5 : -1;

            Vector2 effectsPosition = Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Main.rand.NextFloat());

            Dust laserDust = Dust.NewDustPerfect(effectsPosition + Main.rand.NextVector2Circular(10f, 10f), 72, Main.rand.NextVector2Circular(3f, 3f), Scale: Main.rand.NextFloat(1f, 1.5f));
            laserDust.noGravity = true;

            Vector2 randomLineEffectPosition = effectsPosition + Main.rand.NextVector2Circular(50f, 50f);
            Particle laserLineEffect = new ManaDrainStreak(Owner, 0.3f, -Projectile.velocity * Main.rand.NextFloat(100f, 300f), Main.rand.NextFloat(25f, 100f), Color.Fuchsia, Color.Fuchsia, Main.rand.Next(5, 16), randomLineEffectPosition);
            GeneralParticleHandler.SpawnParticle(laserLineEffect);
        }
    }
}
