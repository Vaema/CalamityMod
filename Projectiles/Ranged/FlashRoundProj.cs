using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class FlashRoundProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public bool Exploding = false;
        public float ExplosionPower = 1.5f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.direction;
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * (Exploding ? 1.5f : 0.2f));

            Projectile.localAI[0] += 1f;
            
            if (Exploding)
            {
                if (Projectile.timeLeft == 59)
                {
                    SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.55f, Volume = 0.4f }, Projectile.Center);
                }
                Projectile.velocity = Vector2.Zero;

                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(4) ? 91 : 264, (new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.9f, 1.2f)) * ExplosionPower);
                dust.scale = Main.rand.NextFloat(0.75f, 0.9f) * ExplosionPower;
                dust.noGravity = true;

                if (Projectile.timeLeft <= 50 && ExplosionPower > 0.5f)
                    ExplosionPower -= 0.015f;
            }
            else
            {
                if (Projectile.localAI[0] > 4f)
                {
                    if (Main.rand.NextBool())
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, 63, -Projectile.velocity * Main.rand.NextFloat(0.9f, 1.1f));
                        dust.scale = Main.rand.NextFloat(0.7f, 0.85f);
                        dust.noGravity = true;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.localAI[0] > 6f && !Exploding)
                CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, Color.White);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Exploding)
            {
                Projectile.timeLeft = 60;
                Exploding = true;
                Projectile.damage = (int)(Projectile.damage * 0.15f);
                Projectile.ExpandHitboxBy(100);
                Projectile.alpha = 255;
            }
            target.AddBuff(BuffID.Confused, 300);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!Exploding)
            {
                Projectile.timeLeft = 60;
                Exploding = true;
                Projectile.damage = (int)(Projectile.damage * 0.15f);
                Projectile.ExpandHitboxBy(100);
                Projectile.alpha = 255;
                Projectile.tileCollide = false;
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            if (!Exploding)
            {
                for (int k = 0; k < 9; k++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, 63, new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.9f, 1.1f));
                    dust.scale = Main.rand.NextFloat(0.6f, 0.75f);
                    dust.noGravity = true;
                }
            }
        }
    }
}
