using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Rogue
{
    public class ElephantGunThrown : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/ElephantGun";

        public static int ChargeupTime = 10;
        public static int Lifetime = 500;
        public float OverallProgress => 1 - Projectile.timeLeft / (float)Lifetime;
        public float ThrowProgress => 1 - Projectile.timeLeft / (float)(Lifetime);
        public float ChargeProgress => 1 - (Projectile.timeLeft - Lifetime) / (float)(ChargeupTime);

        public Player Owner => Main.player[Projectile.owner];
        public ref float Returning => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime + ChargeupTime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override bool ShouldUpdatePosition()
        {
            return ChargeProgress >= 1;
        }

        public override bool? CanDamage()
        {
            //We don't want the anticipation to deal damage.
            if (ChargeProgress < 1)
                return false;

            if (Returning == 1f)
                return false;

            return base.CanDamage();
        }

        //Swing animation keys
        public CurveSegment pullback = new CurveSegment(EasingType.PolyOut, 0f, 0f, MathHelper.PiOver4 * -1.2f, 2);
        public CurveSegment throwout = new CurveSegment(EasingType.PolyOut, 0.7f, MathHelper.PiOver4 * -1.2f, MathHelper.PiOver4 * 1.2f + MathHelper.PiOver2, 3);
        internal float ArmAnticipationMovement() => PiecewiseAnimation(ChargeProgress, new CurveSegment[] { pullback, throwout });

        public override void AI()
        {
            //Anticipation animation. Make the player look like theyre holding the boomerang
            if (ChargeProgress < 1)
            {
                float armRotation = ArmAnticipationMovement() * Owner.direction;

                Owner.heldProj = Projectile.whoAmI;

                Projectile.Center = Owner.MountedCenter + Vector2.UnitY.RotatedBy(armRotation * Owner.gravDir) * -40f * Owner.gravDir;
                Projectile.rotation = (-MathHelper.PiOver2 + armRotation) * Owner.gravDir;

                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + armRotation);

                Projectile.tileCollide = false;
                return;
            }

            Projectile.rotation += 0.4f * Projectile.direction;

            //Play the throw sound when the throw ACTUALLY BEGINS.
            //Additionally, make the projectile collide and set its speed and velocity
            if (Projectile.timeLeft == Lifetime)
            {
                SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow, Projectile.Center);
                Projectile.Center = Owner.MountedCenter + Projectile.velocity * 12f;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 17.5f;
            }

            //Boomerang spinny sounds
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
                Projectile.soundDelay = 8;
            }

            if (Projectile.velocity.Length() < 2f)
            {
                Returning = 1f;
                Projectile.numHits = 0;
            }

            if (Returning == 0f && Projectile.velocity.Length() > 2f && Projectile.timeLeft < (455 + ChargeupTime))
            {
                Projectile.velocity *= 0.88f;
            }

            if (Returning == 1f && Projectile.velocity.Length() < 20f)
            {
                Projectile.velocity *= 1.1f;
            }

            if (Returning == 1f)
            {
                //Aim back at the player
                Projectile.velocity = Projectile.velocity.Length() * (Owner.MountedCenter - Projectile.Center).SafeNormalize(Vector2.One);

                if ((Projectile.Center - Owner.MountedCenter).Length() < 24f)
                {
                    Projectile.Kill();
                }
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity *= 0.3f;
            Returning = 1f;
        }
    }
}
