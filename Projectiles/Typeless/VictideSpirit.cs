using CalamityMod.Cooldowns;
using CalamityMod.Dusts;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class VictideSpirit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                // Validate existence
                bool hasCD = Owner.Calamity().cooldowns.TryGetValue(BubblyBurrow.ID, out CooldownInstance cd);
                if (!hasCD)
                {
                    Owner.AddCooldown(BubblyBurrow.ID, VictideHeadBurrow.BurrowCooldown);
                    Projectile.Kill();
                    return;
                }
                else if (!Owner.Calamity().victideBurrowSet)
                {
                    cd.timeLeft = VictideHeadBurrow.BurrowCooldown;
                    Projectile.Kill();
                    return;
                }
                else if (cd.timeLeft <= VictideHeadBurrow.BurrowCooldown)
                {
                    Projectile.Kill();
                    return;
                }
                // Shimmer interaction (Immediately ends if Chromatic Cloak isn't active)
                else if (Owner.shimmering)
                {
                    cd.timeLeft = VictideHeadBurrow.BurrowCooldown;
                    Projectile.Kill();
                    return;
                }
                Projectile.timeLeft = cd.timeLeft - VictideHeadBurrow.BurrowCooldown;

                #region Movement and Controls
                float MaxSpeed = Collision.DrownCollision(Projectile.Center - Vector2.One, 2, 2, Owner.gravDir) ? VictideHeadBurrow.SubmergedBurrowSpeed : VictideHeadBurrow.BaseBurrowSpeed;
                // Controlling both opposite movement keys brings you to a near immediate stop (faster deceleration than just releasing both buttons)
                // Turnaround acceleration is also twice as fast to aid smoothness of movement
                if (Owner.controlLeft && Owner.controlRight)
                    Projectile.velocity.X *= 0.7f;
                else if (Owner.controlLeft)
                {
                    if (Projectile.velocity.X < -MaxSpeed)
                    {
                        Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, MaxSpeed, 0.05f);
                        if (Projectile.velocity.X > -MaxSpeed - 0.01f)
                            Projectile.velocity.X = -MaxSpeed;
                    }
                    else if (Projectile.velocity.X > 0f)
                        Projectile.velocity.X -= VictideHeadBurrow.BaseAcceleration * 2f;
                    else
                        Projectile.velocity.X = MathF.Max(Projectile.velocity.X - VictideHeadBurrow.BaseAcceleration, -MaxSpeed);
                }
                else if (Owner.controlRight)
                {
                    if (Projectile.velocity.X > MaxSpeed)
                    {
                        Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, MaxSpeed, 0.05f);
                        if (Projectile.velocity.X < MaxSpeed + 0.01f)
                            Projectile.velocity.X = MaxSpeed;
                    }
                    else if (Projectile.velocity.X < 0f)
                        Projectile.velocity.X += VictideHeadBurrow.BaseAcceleration * 2f;
                    else
                        Projectile.velocity.X = MathF.Min(Projectile.velocity.X + VictideHeadBurrow.BaseAcceleration, MaxSpeed);
                }
                else
                    Projectile.velocity.X *= 0.92f;

                if ((Owner.controlUp || Owner.controlJump) && Owner.controlDown)
                    Projectile.velocity.Y *= 0.7f;
                else if (Owner.controlUp || Owner.controlJump)
                {
                    if (Projectile.velocity.Y < -MaxSpeed)
                    {
                        Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, MaxSpeed, 0.05f);
                        if (Projectile.velocity.Y > -MaxSpeed - 0.01f)
                            Projectile.velocity.Y = -MaxSpeed;
                    }
                    else if (Projectile.velocity.Y > 0f)
                        Projectile.velocity.Y -= VictideHeadBurrow.BaseAcceleration * 2f;
                    else
                        Projectile.velocity.Y = MathF.Max(Projectile.velocity.Y - VictideHeadBurrow.BaseAcceleration, -MaxSpeed);
                }
                else if (Owner.controlDown)
                {
                    if (Projectile.velocity.Y > MaxSpeed)
                    {
                        Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, MaxSpeed, 0.05f);
                        if (Projectile.velocity.Y < MaxSpeed + 0.01f)
                            Projectile.velocity.Y = MaxSpeed;
                    }
                    else if (Projectile.velocity.Y < 0f)
                        Projectile.velocity.Y += VictideHeadBurrow.BaseAcceleration * 2f;
                    else
                        Projectile.velocity.Y = MathF.Min(Projectile.velocity.Y + VictideHeadBurrow.BaseAcceleration, MaxSpeed);
                }
                else
                    Projectile.velocity.Y *= 0.92f;
                Projectile.rotation = Projectile.velocity.ToRotation();
                #endregion

                #region Player Manipulation
                // Actively remove mounts and hooks from the player
                Owner.mount?.Dismount(Owner);
                Owner.RemoveAllGrapplingHooks();

                // Give immunity frames and kill suffocation
                Owner.suffocateDelay = 0;
                Owner.immune = true;
                Owner.immuneNoBlink = true;
                Owner.immuneTime = 4;
                for (int k = 0; k < Owner.hurtCooldowns.Length; k++)
                    Owner.hurtCooldowns[k] = Owner.immuneTime;

                Owner.velocity = Projectile.velocity;
                Owner.ChangeDir((Projectile.velocity.X > 0).ToDirectionInt());
                Projectile.direction = Owner.direction;

                // Move the player to the projectile, allowing them to bypass all tiles
                Owner.Center = Projectile.Center;
                #endregion
            }

            #region Visual and Sound Effects
            Color BubblyBlue = new Color(97, 200, 255);
            Lighting.AddLight(Projectile.Center, BubblyBlue.ToVector3() * Projectile.Opacity * 0.5f);

            float lifetimeRatio = Projectile.timeLeft / (float)VictideHeadBurrow.BurrowCooldown;
            if (Projectile.soundDelay <= 0)
            {
                Projectile.soundDelay = Main.rand.Next(25, 41) + (int)(lifetimeRatio * 45);
                SoundEngine.PlaySound(Main.rand.NextBool() ? SoundID.ShimmerWeak1 : SoundID.ShimmerWeak2, Projectile.Center);
            }

            // currently copied from waywasher rn
            // will make them real and not copied pilled later
            if (Main.rand.NextBool())
            {
                // If you're looking at this to use the water effects, this is the main one I recommend you take
                Particle waterfx = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.7f, "CalamityMod/Particles/WaterFoam", false, 9, 0.45f, Color.DeepSkyBlue, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-1, 1), shrinkSpeed: 0.5f);
                GeneralParticleHandler.SpawnParticle(waterfx);
            }

            Particle projBody = new CustomSpark(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 25, -Projectile.velocity * 0.3f, "CalamityMod/Particles/BloomCircle", false, 6, 0.3f, Color.RoyalBlue, new Vector2(1, 1f), true, false, shrinkSpeed: 0.5f);
            GeneralParticleHandler.SpawnParticle(projBody);

            Vector2 tip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 35;
            Particle seafoam = new CustomSpark(tip, Projectile.velocity * Main.rand.NextFloat(0.7f, 0.9f), "CalamityMod/Particles/BloomCircle", false, 2, Main.rand.NextFloat(0.35f, 0.45f), Color.White, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-0.4f, 0.4f));
            GeneralParticleHandler.SpawnParticle(seafoam);
            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle smallSeafoam = new CustomSpark(tip, -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.9f, 1.4f), "CalamityMod/Particles/BloomCircle", true, 15, Main.rand.NextFloat(0.12f, 0.18f), Color.White * 0.7f, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-1, 1));
                    GeneralParticleHandler.SpawnParticle(smallSeafoam);

                    Dust dust = Dust.NewDustPerfect(tip + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<LightDust>(), -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.9f, 1.4f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.65f, 0.8f);
                    dust.color = Color.White;
                    dust.noLightEmittence = true;
                }
            }
            if (Main.rand.NextBool(5))
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble.timeLeft = 12 + Main.rand.Next(8);
                bubble.scale = Main.rand.NextFloat(0.5f, 0.9f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }
            #endregion
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            Owner.velocity *= 0.8f;
            Owner.fullRotation = 0f;
        }
    }
}
