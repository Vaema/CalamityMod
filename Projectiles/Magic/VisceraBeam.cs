using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityMod.Balancing;
using System;

namespace CalamityMod.Projectiles.Magic
{
    public class VisceraBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 9;
            Projectile.height = 9;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 7;
            Projectile.MaxUpdates = 100;
            Projectile.timeLeft = 900;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            for (int i = 0; i <= 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 60 : DustID.Blood);
                dust.position = Projectile.Center;
                dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                dust.velocity = new Vector2(7, 7).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.9f);
                dust.noGravity = true;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[1] > 0)
            {
                SoundStyle hitSound = new("CalamityMod/Sounds/NPCKilled/PerfLargeDeath");
                SoundEngine.PlaySound(hitSound with { Volume = 0.5f }, Projectile.Center);
                for (int i = 0; i <= 14; i++)
                {
                    BloodParticle blood = new BloodParticle(Projectile.Center, new Vector2(15, 15).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.9f) + new Vector2(0, -7), 60, Main.rand.NextFloat(0.4f, 0.65f), Color.Red);
                    GeneralParticleHandler.SpawnParticle(blood);
                }

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<VisceraBoom>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack * 4, Projectile.owner, 0f, Projectile.ai[1]);

                Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, (int)(Viscera.BoomLifetime * 0.38f), false);
                GeneralParticleHandler.SpawnParticle(bloodsplosion);
                Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(255, 32, 32), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f, 0.155f, Viscera.BoomLifetime);
                GeneralParticleHandler.SpawnParticle(bloodsplosion2);
            }
            else
            {
                SoundStyle hitSound = new("CalamityMod/Sounds/NPCHit/PerfLargeHit", 3);
                SoundEngine.PlaySound(hitSound with { Volume = 0.7f }, Projectile.Center);

                for (int i = 0; i <= 6; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 60 : DustID.Blood);
                    dust.scale = Main.rand.NextFloat(0.7f, 1.4f);
                    dust.velocity = Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.8f, 1.9f);
                    dust.noGravity = true;
                }
            }

            int heal = (int)Math.Round(hit.Damage * 0.05);
            if (heal > BalancingConstants.LifeStealCap)
                heal = BalancingConstants.LifeStealCap;

            if (Main.player[Main.myPlayer].lifeSteal <= 0f || heal <= 0 || target.lifeMax <= 5)
                return;

            CalamityGlobalProjectile.SpawnLifeStealProjectile(Projectile, Main.player[Projectile.owner], heal, ProjectileID.VampireHeal, BalancingConstants.LifeStealRange);
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (Projectile.ai[1] > 0)
                Projectile.penetrate = 1;

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] == 16f)
            {
                if (Projectile.ai[1] > 0)
                {
                    for (int i = 0; i <= 25; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 60 : DustID.Blood);
                        dust.scale = Main.rand.NextFloat(0.9f, 1.9f);
                        dust.velocity = Projectile.velocity.RotatedByRandom(0.6) * Main.rand.NextFloat(1.8f, 2.9f);
                        dust.noGravity = true;
                    }
                }
                else
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 60 : DustID.Blood);
                        dust.scale = Main.rand.NextFloat(0.7f, 1.4f);
                        dust.velocity = Projectile.velocity.RotatedByRandom(0.6) * Main.rand.NextFloat(0.8f, 1.9f);
                        dust.noGravity = true;
                    }
                }
            }
            if (Projectile.localAI[0] > 16f)
            {
                int bloody = Dust.NewDust(Projectile.Center, 1, 1, DustID.Blood);
                Main.dust[bloody].position = Projectile.Center + Main.rand.NextVector2Circular(8, 8);
                Main.dust[bloody].scale = Main.rand.NextFloat(0.3f, 0.8f);
                Main.dust[bloody].velocity = -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.6f);
                Main.dust[bloody].noGravity = true;
                if (Projectile.localAI[0] % 3 == 0 && targetDist < 1400f)
                {
                    AltSparkParticle spark = new AltSparkParticle(Projectile.Center - Projectile.velocity * 0.5f, Projectile.velocity * 0.01f, false, 7, 0.8f, Color.DarkRed);
                    GeneralParticleHandler.SpawnParticle(spark);
                    SparkParticle spark2 = new SparkParticle(Projectile.Center - Projectile.velocity * 0.5f, Projectile.velocity * 0.01f, false, 4, 0.65f, Color.Red);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
