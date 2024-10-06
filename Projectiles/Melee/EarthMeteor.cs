using CalamityMod.Buffs.DamageOverTime;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Utilities;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;
using Terraria.Graphics.Renderers;

namespace CalamityMod.Projectiles.Melee
{
    public class EarthMeteor : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public ref float time => ref Projectile.ai[0];
        public SlotId AudSlot;
        public Color mainColor = Color.White;
        public Color randomColor = Color.White;
        public Color variedColor = Color.White;
        public int colorTimer = 0;
        public int fallTime = 180;

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 84;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            Projectile.scale = 1.2f;
            randomColor = Main.rand.Next(3) switch
            {
                0 => Color.OrangeRed,
                1 => Color.MediumTurquoise,
                _ => Color.LawnGreen,
            };
            if (time == 0)
            {
                mainColor = randomColor;
            }

            if (time % 20 == 0)
            {
                variedColor = colorTimer switch
                {
                    0 => Color.OrangeRed,
                    1 => Color.MediumTurquoise,
                    _ => Color.LawnGreen,
                };
                colorTimer++;
                if (colorTimer >= 3)
                    colorTimer = 0;
            }
            mainColor = Color.Lerp(mainColor, variedColor, 0.07f);

            if (time == 0 && Projectile.ai[2] == 2)
            {
                SoundStyle fire2 = new("CalamityMod/Sounds/Item/WeldingShoot");
                AudSlot = SoundEngine.PlaySound(fire2 with { Volume = 0.01f, Pitch = 0.01f, IsLooped = true }, Projectile.Center);
            }
            if (SoundEngine.TryGetActiveSound(AudSlot, out var ChargeSound) && ChargeSound.IsPlaying && Projectile.ai[2] == 2)
            {
                ChargeSound.Position = Projectile.Center;
                ChargeSound.Pitch = Utils.Remap(time, 0, fallTime, 0.4f, -0.8f) * 100;
                ChargeSound.Volume = Utils.Remap(time, fallTime * 0.2f, fallTime, 0f, 0.9f) * 100;
            }
            if (time == (int)(fallTime * 0.2f) && Projectile.ai[2] > 0)
            {
                Vector2 spawnSpot = Owner.Center + new Vector2(Main.rand.NextFloat(-450, 450), Main.rand.NextFloat(-450, -650));
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnSpot, Vector2.Zero, ModContent.ProjectileType<EarthMeteor>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 0, Projectile.ai[2] - 1);
            }
            if (time == fallTime)
            {
                Projectile.extraUpdates = 15;

                NPC target = Owner.Calamity().mouseWorld.ClosestNPCAt(2000);
                if (target != null)
                    Projectile.velocity = (target.Center - Projectile.Center + target.velocity * 8).SafeNormalize(Vector2.UnitX) * 8;
                else
                    Projectile.velocity = (Owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 8;
            }
            if (time >= fallTime)
            {
                // Spawn in a helix-style pattern
                float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 16f;
                if (targetDist < 1400f && time % 2 == 0)
                {
                    GlowSparkParticle orb = new(Projectile.Center + offset, -Projectile.velocity * 0.5f, false, 10, 0.03f, mainColor, new Vector2(0.5f, 1f), false, false);
                    GeneralParticleHandler.SpawnParticle(orb);

                    GlowSparkParticle orb2 = new(Projectile.Center - offset, -Projectile.velocity * 0.5f, false, 10, 0.03f, mainColor, new Vector2(0.5f, 1f), false, false);
                    GeneralParticleHandler.SpawnParticle(orb2);
                }
            }
            else
            {
                float randSize = Main.rand.NextFloat(0.8f, 1.2f);
                for (int i = 0; i < 2; i++)
                {
                    Particle bloom = new CustomPulse(Projectile.Center, Vector2.Zero, mainColor * Utils.Remap(time, 0, fallTime, 0, 0.65f), "CalamityMod/Particles/LargeBloom", new Vector2(Utils.Remap(time, 0, fallTime, 0.3f, 3), Utils.Remap(time, fallTime * 0.7f, fallTime, 1, 2)), 0, 0.8f * randSize, 0f, 3);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                Particle bloom3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White * Utils.Remap(time, 0, fallTime, 0f, 0.65f), "CalamityMod/Particles/LargeBloom", new Vector2(Utils.Remap(time, 0, fallTime, 0.3f, 3), Utils.Remap(time, fallTime * 0.7f, fallTime, 1, 2)), 0, 0.65f * randSize, 0f, 3);
                GeneralParticleHandler.SpawnParticle(bloom3);
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time <= fallTime)
                return false;

            Color auraColor = mainColor;
            for (int i = 0; i < 7; i++)
            {
                Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/EarthMeteor").Value;
                Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 20f).ToRotationVector2();
                rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);
                Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition + rotationalDrawOffset, null, auraColor, Projectile.rotation, centerTexture.Size() * 0.5f, Projectile.scale * 1.1f, SpriteEffects.None, 0f);
            }

            
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Lerp(Color.White, randomColor, 0.3f), 1);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(AudSlot, out var ChargeSound) && Projectile.ai[2] == 2)
                ChargeSound?.Stop();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
            if (Projectile.numHits <= 0)
            {
                Player Owner = Main.player[Projectile.owner];
                Owner.Calamity().GeneralScreenShakePower = 4.5f;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<EarthBoom>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner);
                for (int i = 0; i < 20; i++)
                {
                    randomColor = Main.rand.Next(3) switch
                    {
                        0 => Color.OrangeRed,
                        1 => Color.MediumTurquoise,
                        _ => Color.LawnGreen,
                    };
                    Dust dust2 = Dust.NewDustPerfect(target.Center, 278, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(5.5f, 20));
                    dust2.scale = Main.rand.NextFloat(0.85f, 1.15f);
                    dust2.noGravity = false;
                    dust2.color = Color.Lerp(Color.White, randomColor, 0.5f);

                    Particle sparker = new CustomSpark(target.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(5.5f, 20), "CalamityMod/Particles/Sparkle", false, 38, Main.rand.NextFloat(2.2f, 4.8f), randomColor, new Vector2(0.4f, Main.rand.NextFloat(0.9f, 1.4f)), true, true);
                    GeneralParticleHandler.SpawnParticle(sparker);
                }

                for (int i = 0; i < 3; i++)
                {
                    Particle bolt2 = new CustomPulse(target.Center, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.5f - i * 0.05f, 19 - i * 4);
                    GeneralParticleHandler.SpawnParticle(bolt2);
                    Particle bolt3 = new CustomPulse(target.Center, Vector2.Zero, Color.MediumTurquoise, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.35f - i * 0.05f, 17 - i * 4);
                    GeneralParticleHandler.SpawnParticle(bolt3);
                    Particle bolt4 = new CustomPulse(target.Center, Vector2.Zero, Color.LawnGreen, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.2f - i * 0.05f, 15 - i * 4);
                    GeneralParticleHandler.SpawnParticle(bolt4);
                }

                SoundStyle fire2 = new("CalamityMod/Sounds/Item/EarthMeteor");
                SoundEngine.PlaySound(fire2 with { Volume = 0.9f }, target.Center);

                Particle blastRing = new CustomPulse(target.Center, Vector2.Zero, mainColor, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 4f, 3f, 18, true);
                GeneralParticleHandler.SpawnParticle(blastRing);
                Particle blastRing2 = new CustomPulse(target.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 3f, 2f, 18, true);
                GeneralParticleHandler.SpawnParticle(blastRing2);

            }
        }
        public override bool? CanDamage() => time < fallTime ? false : null;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 80, targetHitbox);
    }
}
