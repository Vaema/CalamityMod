using CalamityMod.Items.Ammo;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class HyperiusSplit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private Color currentColor = Color.Black;
        private int rotDirection = 1;
        private float rotIntensity;
        private bool rotPhase2 = false;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 500;
            Projectile.extraUpdates = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10 * Projectile.extraUpdates;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            Projectile.localAI[0]++;
            if (currentColor == Color.Black)
            {
                Projectile.alpha = 255;
                rotDirection = Main.rand.NextBool() ? 1 : -1;
                rotIntensity = Main.rand.NextFloat(0.5f, 1.5f);
                Projectile.timeLeft = Main.rand.Next(250, 300 + 1);
                switch (Projectile.ai[2])
                {
                    case 4: // Yellow shot
                        currentColor = Color.Yellow * 0.65f;
                        break;
                    case 3: // Magenta shot
                        currentColor = Color.Magenta * 0.65f;
                        break;
                    case 2: // Red shot
                        currentColor = Color.Red * 0.65f;
                        break;
                    case 1: // Blue shot
                        currentColor = Color.Cyan * 0.65f;
                        break;
                    default: // Green shot
                        currentColor = Color.Lime * 0.65f;
                        break;
                }
            }
            if (targetDist < 1200f)
            {
                GlowOrbParticle orb = new GlowOrbParticle(Projectile.Center - Projectile.velocity, -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.9f), false, 3, 0.55f, currentColor, true, true);
                GeneralParticleHandler.SpawnParticle(orb);
            }

            if (Projectile.timeLeft == 180)
                rotPhase2 = true;

            if (rotPhase2)
            {
                rotIntensity *= 1.001f;
                Projectile.velocity *= 0.997f;
                Projectile.velocity = Projectile.velocity.RotatedBy(-0.025f * rotIntensity * rotDirection);
            }
            else
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(0.015f * rotIntensity * rotDirection);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int b = 0; b < 2; b++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 66, new Vector2(2, 2).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.5f, 1.1f);
                dust.color = currentColor;
                dust.fadeIn = 0;
            }
        }
        public override bool? CanDamage() => Projectile.localAI[0] < 20 ? false : null;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage.Flat += HyperiusBullet.SplitBulletBonusDamage;
    }
}
