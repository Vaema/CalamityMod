using CalamityMod.Cooldowns;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
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

        public static Asset<Texture2D> AuraTexture;
        public static Asset<Texture2D> GlowTexture;

        public ref float DustCounter => ref Projectile.ai[1];

        public override void Load()
        {
            AuraTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarTrail");
            GlowTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
        }

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
                Projectile.rotation = Projectile.velocity.Length() == 0f ? MathHelper.PiOver2 : Projectile.velocity.ToRotation();
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
                if (Owner.velocity.Y == 0f)
                    Owner.velocity.Y += 0.00001f; // Prevent sprint particles from appearing
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

            if (Main.rand.NextBool(4) || Projectile.velocity.Length() > 0.25f)
            {
                Particle foam = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.7f, "CalamityMod/Particles/WaterFoam", false, 9, 0.25f, Color.DeepSkyBlue, Vector2.One, extraRotation: Main.rand.NextFloat(0, MathHelper.TwoPi), shrinkSpeed: 1f);
                GeneralParticleHandler.SpawnParticle(foam);
            }

            DustCounter += Utils.Remap(Projectile.velocity.Length(), 0f, VictideHeadBurrow.BaseBurrowSpeed, 1f, 3f, true);
            if (DustCounter >= 15f)
            {
                DustCounter = 0f;
                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Projectile.rotation.ToRotationVector2().RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(-4.8f, -6f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.65f, 0.8f);
                    dust.color = Color.White;
                    dust.noLightEmittence = true;
                }
            }
            #endregion
        }

        public float JetstreamWidthFunction(float completionRatio) => Utils.GetLerpValue(1f, 0.8f, completionRatio, true) * 3f;

        public Color InnerJetstreamColorFunction(float completionRatio) => Color.Lerp(Color.CornflowerBlue, new Color(169, 100, 237), MathF.Pow(completionRatio, 2.5f)) * Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * 0.5f;
        public Color OuterJetstreamColorFunction(float completionRatio) => Color.Lerp(Color.CornflowerBlue, new Color(100, 237, 237), MathF.Pow(completionRatio, 2.5f)) * Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * 0.2f;

        public override bool PreDraw(ref Color lightColor)
        {
            // Jetstream trails
            for (int direction = -1; direction <= 1; direction += 2)
            {
                List<Vector2> trailPositionsInner = new List<Vector2>();
                List<Vector2> trailPositionsOuter = new List<Vector2>();
                for (int i = 0; i < 20; i++)
                {
                    float mergeFactor = Utils.Remap(i, 0f, 8f, 0f, 1f, true);

                    Vector2 trailPosInner = Projectile.Center - Projectile.rotation.ToRotationVector2() * i * 8f;
                    float spreadLengthInner = 20f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
                    Vector2 sinOffsetInner = (Vector2.UnitY * direction * MathF.Sin(i * MathHelper.Pi * 0.125f) * spreadLengthInner * mergeFactor).RotatedBy(Projectile.rotation);
                    trailPositionsInner.Add(trailPosInner + sinOffsetInner);

                    Vector2 trailPosOuter = Projectile.Center - Projectile.rotation.ToRotationVector2() * i * 12f;
                    float spreadLengthOuter = 32f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
                    Vector2 sinOffsetOuter = (Vector2.UnitY * direction * (8f + MathF.Sin(i * MathHelper.Pi * 0.035f) * spreadLengthOuter * mergeFactor)).RotatedBy(Projectile.rotation);
                    trailPositionsOuter.Add(trailPosOuter + sinOffsetOuter);
                }

                PrimitiveRenderer.RenderTrail(trailPositionsInner, new(JetstreamWidthFunction, InnerJetstreamColorFunction), 20);
                PrimitiveRenderer.RenderTrail(trailPositionsOuter, new(JetstreamWidthFunction, OuterJetstreamColorFunction), 20);
            }

            Texture2D aura = AuraTexture.Value;
            Vector2 drawPos = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            float auraRotation = Projectile.rotation + MathHelper.PiOver2;

            // Outer aura trail
            Vector2 spinPoint = Vector2.UnitY * -3f;
            float rotation = MathHelper.TwoPi * Main.GlobalTimeWrappedHourly;
            for (int o = 0; o < 6; o += 2)
            {
                Vector2 spinStart = drawPos + spinPoint.RotatedBy(rotation - MathHelper.Pi * o / 3f);
                Color outerColor = Color.Lerp(Color.Cyan, Color.RoyalBlue, o / 6f) * 0.25f;
                outerColor.A = 0;
                Main.EntitySpriteDraw(aura, spinStart, null, outerColor, auraRotation, aura.Size() * 0.5f, 0.8f, SpriteEffects.None, 0);
            }

            // Inner aura trail
            Color innerColor = Color.SkyBlue * 0.5f;
            innerColor.A = 0;
            for (float i = 0f; i < 1f; i += 0.5f)
            {
                Main.EntitySpriteDraw(aura, drawPos, null, innerColor * (0.5f + 0.5f * i), auraRotation, aura.Size() * 0.5f, 0.4f + 0.2f * i, SpriteEffects.None, 0);
            }

            // Backing aura glow
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Texture2D glow = GlowTexture.Value;
            Main.EntitySpriteDraw(glow, drawPos + Projectile.rotation.ToRotationVector2() * 8f, null, Color.CornflowerBlue, auraRotation, glow.Size() * 0.5f, new Vector2(0.36f, 0.54f), SpriteEffects.None, 0);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            Owner.velocity *= 0.8f;
            Owner.fullRotation = 0f;
        }
    }
}
