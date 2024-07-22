using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class VolatileStarcore : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        private static int Lifetime = 240;
        private static int NumAnimationFrames = 6;
        private static int AnimationFrameTime = 2;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = NumAnimationFrames;
        }
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 48;
        }

        public override void AI()
        {
            // Draw offsets
            DrawOffsetX = -10;
            DrawOriginOffsetY = -10;
            DrawOriginOffsetX = 0;

            // Set rotation on frame 1
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }

            // Dust only shows up after the first few frames
            if (Projectile.localAI[0] >= 5f)
                SpawnDust();

            // Lighting and spin
            Lighting.AddLight(Projectile.Center, 1.8f, 1.6f, 0.5f);

            // Increment frame counter
            Projectile.localAI[0] += 1f;

            // Update animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > AnimationFrameTime)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= NumAnimationFrames)
                Projectile.frame = 0;
            if (Projectile.timeLeft == 239)
            {
                for (int i = 0; i <= 3; i++)
                {
                    Particle lightning = new ThunderBoltVFX(() => Projectile.Center, (Projectile.velocity).ToRotation() - MathHelper.PiOver2, 0.4f, Color.Red, 50, 0);
                    GeneralParticleHandler.SpawnParticle(lightning);
                }
            }
            if (Projectile.timeLeft <= 231)
            {
                SparkParticle trail = new SparkParticle(Projectile.Center - Projectile.velocity * 1.8f, -Projectile.velocity * 0.01f, false, 11, 4f, Color.OrangeRed * 0.5f);
                GeneralParticleHandler.SpawnParticle(trail);
            }
            if (Projectile.timeLeft <= 220)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center + Main.rand.NextVector2Circular(15, 15) - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 10, -Projectile.velocity * Main.rand.NextFloat(0.5f, 1.5f), false, 45, 0.9f, Color.Red);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        private void SpawnDust()
        {
            int trailDustCount = 4; //5
            int trailDustType = 264;
            for (int i = 0; i < trailDustCount; ++i)
            {
                float scale = Main.rand.NextFloat(1.0f, 1.4f);
                int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, trailDustType, 0f, 0f);
                Main.dust[idx].velocity = Projectile.velocity * 0.8f;
                Main.dust[idx].scale = scale;
                Main.dust[idx].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Daybreak, 180);
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            // Spawn a Helium Flash on impact
            int type = ModContent.ProjectileType<HeliumFlashBlast>();
            int damage = (int)(HeliumFlash.ExplosionDamageMultiplier * Projectile.damage);
            float kb = 9.5f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, type, damage, kb, Projectile.owner, 0f, 0f);
        }
    }
}
