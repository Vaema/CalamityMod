using System;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Particles;
using Microsoft.Build.Execution;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class GaussRifleBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.extraUpdates < 30 && Projectile.timeLeft % 3 == 0)
                Projectile.extraUpdates++;
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (Projectile.timeLeft % 2 == 0 && targetDist < 1400 && Projectile.timeLeft <= 585)
            {
                float cLerp = Utils.GetLerpValue(500, 600, Projectile.timeLeft);
                Particle spark = new GlowSparkParticle(Projectile.Center - Projectile.velocity * 0.5f, Projectile.velocity * 0.01f, false, 7, 0.08f, Color.Lerp(Color.Gold, Color.Goldenrod, cLerp), new Vector2(0.3f, 1), true, true);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (Main.rand.NextBool(7))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), 267);
                dust.scale = Main.rand.NextFloat(0.5f, 1.1f);
                dust.velocity = Projectile.velocity * Main.rand.NextFloat(-3, 3);
                dust.noGravity = true;
                dust.color = Main.rand.NextBool() ? Color.Gold : Color.Goldenrod;
            }
            if (Projectile.timeLeft == 600)
            {
                Particle pulse = new GlowSparkParticle(Projectile.Center, Projectile.velocity * 3, false, 8, 0.042f, Color.Goldenrod, new Vector2(1.5f, 0.9f), true);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Particle spark = new GlowSparkParticle(Projectile.Center, Projectile.velocity, false, 8, MathHelper.Clamp(0.06f - Projectile.numHits * 0.01f, 0f, 0.06f), Color.Goldenrod * 0.9f, new Vector2(1, 0.5f), true);
            GeneralParticleHandler.SpawnParticle(spark);

            if (Projectile.numHits == 0)
            {
                Particle bolt2 = new CustomPulse(target.Center, Vector2.Zero, Color.Gold * 0.55f, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 1.4f, 0, 23);
                GeneralParticleHandler.SpawnParticle(bolt2);
                Particle bolt3 = new CustomPulse(target.Center, Vector2.Zero, Color.Gold * 0.35f, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 1.6f, 0, 18);
                GeneralParticleHandler.SpawnParticle(bolt3);

                for (int i = 0; i < 8; i++)
                {
                    Vector2 dustVel = Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(3f, 7f);
                    Dust dust = Dust.NewDustPerfect(target.Center + dustVel * 5, 267, -dustVel * 0.5f, 0, default, Main.rand.NextFloat(0.5f, 1f));
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool() ? Color.Gold : Color.Goldenrod;
                }

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<GaussRifleExplosion>(), Projectile.damage / 3, 0, Projectile.owner);
            }

            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.84f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), 66);
                dust.scale = Main.rand.NextFloat(0.5f, 1.1f);
                dust.velocity = Projectile.velocity.RotateRandom(0.3f) * Main.rand.NextFloat(1, 3);
                dust.noGravity = true;
                dust.color = Main.rand.NextBool() ? Color.LightYellow : Color.Gold;
                dust.noLight = true;
            }
        }
    }
}
