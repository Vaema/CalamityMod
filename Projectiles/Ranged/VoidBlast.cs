using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class VoidBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Melee/GalaxiaBolt";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.alpha = 150;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 1;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] == 12f)
            {
                Projectile.localAI[0] = 0f;
                for (int l = 0; l < 12; l++)
                {
                    Vector2 dustVel = Vector2.UnitX * (float)-(float)Projectile.width / 2f;
                    dustVel += -Vector2.UnitY.RotatedBy((double)((float)l * 3.14159274f / 6f), default) * new Vector2(8f, 16f);
                    dustVel = dustVel.RotatedBy((double)(Projectile.rotation - 1.57079637f), default);
                    int shadowDust = Dust.NewDust(Projectile.Center, 0, 0, DustID.ShadowbeamStaff, 0f, 0f, 160, default, 1f);
                    Main.dust[shadowDust].scale = 1.1f;
                    Main.dust[shadowDust].noGravity = true;
                    Main.dust[shadowDust].position = Projectile.Center + dustVel;
                    Main.dust[shadowDust].velocity = Projectile.velocity * 0.1f;
                    Main.dust[shadowDust].velocity = Vector2.Normalize(Projectile.Center - Projectile.velocity * 3f - Main.dust[shadowDust].position) * 1.25f;
                }
            }
            Projectile.alpha -= 15;
            int alphaControl = 150;
            if (Projectile.Center.Y >= Projectile.ai[1])
            {
                alphaControl = 0;
            }
            if (Projectile.alpha < alphaControl)
            {
                Projectile.alpha = alphaControl;
            }
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(90) * Projectile.direction;
            if (Main.rand.NextBool(16))
            {
                Vector2 value3 = Vector2.UnitX.RotatedByRandom(1.5707963705062866).RotatedBy((double)Projectile.velocity.ToRotation(), default);
                int extraDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1.2f);
                Main.dust[extraDust].velocity = value3 * 0.66f;
                Main.dust[extraDust].position = Projectile.Center + value3 * 12f;
            }
            if (Main.rand.NextBool(48) && Main.netMode != NetmodeID.Server)
            {
                int voidGore = Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f), 16, 1f);
                Main.gore[voidGore].velocity *= 0.66f;
                Main.gore[voidGore].velocity += Projectile.velocity * 0.3f;
            }
            if (Projectile.ai[1] == 1f)
            {
                Projectile.light = 0.9f;
                if (Main.rand.NextBool(10))
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1.2f);
                }
                if (Main.rand.NextBool(20) && Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, new Vector2(Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f), Main.rand.Next(16, 18), 1f);
                }
            }
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.1f / 255f, (255 - Projectile.alpha) * 0.7f / 255f, (255 - Projectile.alpha) * 0.15f / 255f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Voidfrost>(), 180);
            target.AddBuff(ModContent.BuffType<WhisperingDeath>(), 180);
            if (Projectile.numHits == 0)
            {
                Player Owner = Main.player[Projectile.owner];
                Owner.Calamity().sharkGunDamageScaling++;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundStyle fire = new("CalamityMod/Sounds/Item/OmicronBeam");
            SoundEngine.PlaySound(fire with { Volume = 0.9f }, Projectile.Center);
            float blastSize = 170;
            float minMultiplier = 0.25f;
            int hitsToMinMult = 5;
            Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
            blast.timeLeft = 2;
            blast.DamageType = Projectile.DamageType;
            for (float k = 0; k < 3; k++)
            {
                float colorRando = Main.rand.NextFloat(0, 1);
                int partLifetime = Main.rand.Next(13, 15 + 1);
                float scale = Main.rand.NextFloat(0.10f, 0.16f);
                Vector2 spawnPos = Projectile.Center + (Main.rand.NextVector2Circular(20, 20) * (k + 1));
                Particle blastRing = new CustomPulse(spawnPos, Vector2.Zero, Color.Lerp(Color.DarkOrchid, Color.Indigo, colorRando) * 0.6f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0.07f, scale, partLifetime);
                GeneralParticleHandler.SpawnParticle(blastRing);
            }
            for (int i = 0; i < 3; i++)
            {
                Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/SmallBloom", Vector2.One, Main.rand.NextFloat(-10, 10), 1f, 1.2f, 15, false);
                GeneralParticleHandler.SpawnParticle(blastRing);
            }
            Particle innerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Indigo, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.03f, 1f, 24, true);
            GeneralParticleHandler.SpawnParticle(innerGlow);
            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2();
                Particle cross = new GlowSparkParticle(Projectile.Center, velocity, false, 12, 0.4f, Color.BlueViolet * 0.7f, new Vector2(0.07f, 0.08f), true, false);
                GeneralParticleHandler.SpawnParticle(cross);
            }
            for (float k = 0; k < 10; k++)
            {
                Particle spark = new CustomSpark(Projectile.Center, new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(2f, 4f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 27, Main.rand.NextFloat(1.1f, 1.3f), Color.Lerp(Color.MediumPurple, Color.Indigo, Main.rand.NextFloat(0.5f, 0.7f)), new Vector2(0.6f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.35f, 0.4f));
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (Main.rand.NextBool())
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(72, 73 + 1), Main.rand.NextVector2CircularEdge(8f, 8f) * (Main.rand.NextFloat(1f, 1.2f) + Projectile.scale));
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f) + Projectile.scale;
                    dust.alpha = Main.rand.Next(120, 180 + 1);
                }
            }
        }
    }
}
