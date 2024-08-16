using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class DestructionStar : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/StarofDestruction";
        public ref float time => ref Projectile.ai[0];
        private float Radius = 47f;
        public float fade = 0;
        public int beepTimerMax = 50;
        public int beepTimer = 0;
        public int stealthHitCooldown = 0;
        public bool makeReadySound = false;
        public int rotDirection = 1;
        public float rot = 0.01f;
        public bool setRot = true;
        public override void SetDefaults()
        {
            Projectile.width = 94;
            Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }
        public override void AI()
        {
            if (setRot)
            {
                rotDirection = Main.rand.NextBool() ? 1 : -1;
                setRot = false;
            }
            Projectile.rotation += rot;
            rot += (Utils.Remap(Projectile.timeLeft, 0, 240, 0.02f, 0.0015f)) * rotDirection;

            Player Owner = Main.player[Projectile.owner];
            Vector2 moveToMouse = (Owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
            if (Projectile.numHits == 0 || Projectile.Calamity().stealthStrike)
            {
                if (Projectile.velocity.Length() < (Projectile.Calamity().stealthStrike ? 18 : 13) && time > 8)
                    Projectile.velocity += moveToMouse * (Projectile.Calamity().stealthStrike ? 0.8f : 0.4f);
                else if (time > 8)
                    Projectile.velocity *= 0.9f;

                if (Projectile.velocity.Length() > 3 && Main.rand.NextBool())
                {
                    Vector2 placement = Projectile.Center + Main.rand.NextVector2Circular(70, 70);
                    float speed = Main.rand.NextFloat(0.9f, 1.9f);
                    Particle spark = new GlowOrbParticle(placement, -Projectile.velocity * speed, false, 11, 0.8f, Color.Black, false, false, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                    Particle spark2 = new GlowOrbParticle(placement, -Projectile.velocity * speed, false, 11, 0.5f, Color.LightGreen, true, false, true);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }

                fade = MathHelper.Lerp(fade, 0, 0.088f);
                if (Projectile.timeLeft < 240)
                {
                    if (beepTimer >= beepTimerMax)
                    {
                        beepTimerMax = (int)(beepTimerMax * 0.81f);
                        fade = 1;
                        rot *= -0.8f;
                        rotDirection *= -1;
                        SoundStyle sound = new("CalamityMod/Sounds/Custom/PlagueSounds/PBGAttackSwitchShort");
                        SoundEngine.PlaySound(sound with { Volume = 0.3f, Pitch = 0.7f * Utils.GetLerpValue(240, 0, Projectile.timeLeft, true) }, Projectile.Center);
                        beepTimer = 0;
                    }
                    beepTimer++;
                }
            }
            else if (!Projectile.Calamity().stealthStrike)
            {
                fade = 0;
                Projectile.velocity = Vector2.Zero;
                if (time == 8)
                    Projectile.Kill();
            }
            //if (Projectile.Calamity().stealthStrike)

            time++;
            if (stealthHitCooldown > 0)
                stealthHitCooldown--;

            if (Projectile.Calamity().stealthStrike && Projectile.numHits > 0)
            {
                Projectile.velocity = Vector2.Zero;
                if (time == 8)
                {
                    Projectile.numHits = 0;
                    stealthHitCooldown = 45;
                    makeReadySound = true;
                    Explode(false);

                    Projectile.velocity += moveToMouse * 30;
                }
            }
            if (stealthHitCooldown == 0 && makeReadySound)
            {
                SoundStyle explo2 = new("CalamityMod/Sounds/Item/DeadSunRicochet");
                SoundEngine.PlaySound(explo2 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
                for (int i = 0; i <= 15; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, dustStyle, Projectile.velocity);
                    dust.scale = Main.rand.NextFloat(0.8f, 1f);
                    dust.velocity = new Vector2(13, 13).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f);
                    dust.noGravity = true;
                    dust.color = Color.LightGreen;
                }
                makeReadySound = false;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            time = 0;
            SoundStyle sound = new("CalamityMod/Sounds/Item/DudFire");
            SoundEngine.PlaySound(sound with { Volume = 0.6f, Pitch = -0.4f }, Projectile.Center);

            Particle orb7 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/SmallBloomRing", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 3, 0f, 12, false);
            GeneralParticleHandler.SpawnParticle(orb7);
        }
        public override void OnKill(int timeLeft)
        {
            if (!Projectile.Calamity().stealthStrike)
                Explode(false);
            else
                Explode(true);

        }

        internal void Explode(bool big)
        {
            float sizeBonus = big ? 2 : 1;
            float bigExplosionDamage = 1f;
            Player Owner = Main.player[Projectile.owner];
            Particle orb = new CustomPulse(Projectile.Center, Vector2.Zero, Color.LightGreen with { A = 0 }, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0f, 0.82f * sizeBonus, 11);
            GeneralParticleHandler.SpawnParticle(orb);
            Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0f, 0.74f * sizeBonus, 11);
            GeneralParticleHandler.SpawnParticle(orb2);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DestructionExplosion>(), (int)(Projectile.damage * (big ? bigExplosionDamage : Projectile.Calamity().stealthStrike ? 0.4f : 0.8f)), Projectile.knockBack * 1.5f, Projectile.owner);
            SoundStyle explo = new("CalamityMod/Sounds/Item/MeldExplosion");
            SoundEngine.PlaySound(explo with { Volume = 0.9f }, Projectile.Center);
            if (big)
            {
                SoundStyle explo2 = new("CalamityMod/Sounds/Item/EarthMeteor");
                SoundEngine.PlaySound(explo2 with { Volume = 0.9f, Pitch = -0.2f }, Projectile.Center);

                SoundStyle explo3 = new("CalamityMod/Sounds/Item/ExobladeDashImpact");
                SoundEngine.PlaySound(explo3 with { Volume = 0.9f, Pitch = -0.2f }, Projectile.Center);
            }

            Particle bolt2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.LightGreen, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 2.56f * 1.3f * sizeBonus, 18);
            GeneralParticleHandler.SpawnParticle(bolt2);

            Particle bolt3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.LightGreen, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.3f * sizeBonus, 30);
            GeneralParticleHandler.SpawnParticle(bolt3);

            Particle bolt4 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.LightGreen * 0.55f, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.3f * 1.35f * sizeBonus, 30);
            GeneralParticleHandler.SpawnParticle(bolt4);

            for (int i = 0; i < 2; i++)
            {
                Particle orb7 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/SmallBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), (2.2f + i * 0.5f) * sizeBonus, 0f, 80, false);
                GeneralParticleHandler.SpawnParticle(orb7);
            }
            for (int i = 0; i < 12; i++)
            {
                Particle spark3 = new GlowOrbParticle(Projectile.Center, new Vector2(21, 21).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1f) * sizeBonus, false, 25, Main.rand.NextFloat(0.8f, 1.5f) * sizeBonus, Color.LightGreen, true, false, false);
                GeneralParticleHandler.SpawnParticle(spark3);
                Particle spark4 = new GlowOrbParticle(Projectile.Center, new Vector2(21, 21).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1f) * sizeBonus, false, 25, Main.rand.NextFloat(0.8f, 1.5f) * sizeBonus, Color.Black, false, false, false);
                GeneralParticleHandler.SpawnParticle(spark4);
            }
            for (int i = 0; i < (25 * sizeBonus); i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 278);
                dust.noGravity = false;
                dust.velocity = new Vector2(18, 18).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1f) * sizeBonus;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.color = Color.LightGreen;
            }
            Owner.Calamity().GeneralScreenShakePower = 6f * (big ? 2.5f : 1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/StarofDestructionGhost");
            Asset<Texture2D> portal = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

            if (Projectile.Calamity().stealthStrike)
            {
                Main.EntitySpriteDraw(portal.Value, Projectile.Center - Main.screenPosition, null, Color.LightGreen with { A = 0 } * Utils.GetLerpValue(45, 0, stealthHitCooldown), 0, portal.Size() * 0.5f, 1.1f * Utils.GetLerpValue(45, 0, stealthHitCooldown), SpriteEffects.None);
            }
            for (int i = 0; i < 3; i++)
            {
                Color auraColor = Color.Black;
                Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 17f).ToRotationVector2();
                rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 6f) * 0.5f);
                Vector2 rotationalDrawOffset2 = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 17f).ToRotationVector2();
                rotationalDrawOffset2 *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 6f) * 0.5f - 2);
                Main.EntitySpriteDraw(tex2.Value, Projectile.Center - Main.screenPosition + rotationalDrawOffset, null, auraColor, Projectile.rotation, tex2.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                for (int b = 0; b < 2; b++)
                {
                    Main.EntitySpriteDraw(tex2.Value, Projectile.Center - Main.screenPosition + rotationalDrawOffset2, null, Color.LightGreen with { A = 0 }, Projectile.rotation, tex2.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
            }

            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex2.Value, Projectile.Center - Main.screenPosition, null, Color.LightGreen with { A = 0 } * fade, Projectile.rotation, tex2.Size() / 2f, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Radius, targetHitbox);
        public override bool? CanDamage() => stealthHitCooldown > 0 ? false : Projectile.numHits > 0 ? false : null;
    }
}
