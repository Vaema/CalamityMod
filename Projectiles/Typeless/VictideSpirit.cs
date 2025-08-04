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
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class VictideSpirit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];

        public static Asset<Texture2D> GlowTexture;

        public ref float SubmergedTimer => ref Projectile.ai[0];
        public ref float DustCounter => ref Projectile.ai[1];

        public override void Load() => GlowTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = MathHelper.PiOver2;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
                Projectile.oldRot[i] = MathHelper.PiOver2;

            for (int i = 0; i < 3; i++)
            {
                Color squareColor = Color.Lerp(Color.Cyan, Color.RoyalBlue, i * 0.5f) * 0.6f;
                float squareScale = 0.6f + i * 0.2f;
                float squareRot = (i % 2 == 0 ? MathHelper.PiOver4 : 0f);
                Particle square = new CustomSpark(Projectile.Center, Vector2.Zero, "CalamityMod/Particles/GlowSquareParticleThick", false, 18, squareScale, squareColor, Vector2.One, true, true, squareRot, true, glowOpacity: 0.5f, glowCenterScale: squareScale * 1.15f);
                GeneralParticleHandler.SpawnParticle(square);
            }
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

                // Rotation is less sudden so that the visuals are smoother -- velocity is not affected so that it also plays natural
                float idealRotation = (Projectile.velocity.Length() == 0f ? MathHelper.PiOver2 : Projectile.velocity.ToRotation());
                if (MathF.Abs(idealRotation - Projectile.rotation) < MathHelper.ToRadians(1f) || MathF.Abs(idealRotation - Projectile.rotation) > MathHelper.ToRadians(90f))
                    Projectile.rotation = idealRotation;
                else
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, idealRotation, 0.25f);
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
                {
                    // Excluded explicitly-anti cheesed bosses
                    if (k == ImmunityCooldownID.Bosses)
                        continue;

                    Owner.hurtCooldowns[k] = Owner.immuneTime;
                }

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
            if (Collision.DrownCollision(Projectile.Center - Vector2.One, 2, 2, Owner.gravDir))
                SubmergedTimer++;
            else
                SubmergedTimer = 0f;

            Color BubblyBlue = new Color(97, 200, 255);
            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 8f;
            Lighting.AddLight(headPosition, BubblyBlue.ToVector3() * Projectile.Opacity * (0.5f + 0.3f * Utils.GetLerpValue(0f, 60f, SubmergedTimer, true)));

            if (SubmergedTimer == 1f)
            {
                Particle ring = new DirectionalPulseRing(headPosition, Projectile.velocity.SafeNormalize(Vector2.UnitY) * 0.4f, BubblyBlue, new Vector2(0.5f, 1f), Projectile.rotation, 0.95f, 0f, 30);
                GeneralParticleHandler.SpawnParticle(ring);
            }

            float lifetimeRatio = Projectile.timeLeft / (float)VictideHeadBurrow.BurrowCooldown;
            if (Projectile.soundDelay <= 0)
            {
                Projectile.soundDelay = Main.rand.Next(20, 36) + (int)(lifetimeRatio * 30);
                SoundEngine.PlaySound(Main.rand.NextBool() ? SoundID.ShimmerWeak1 : SoundID.ShimmerWeak2, Projectile.Center);
            }

            if ((Main.rand.NextBool(4) || Projectile.velocity.Length() > 0.25f) && SubmergedTimer == 0f)
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
        public float SpiritWidthFunction(float completionRatio) => MathF.Min(MathHelper.SmoothStep(64f, 8f, completionRatio), Utils.GetLerpValue(0f, 0.15f, completionRatio, true) * 60f);

        public Color InnerJetstreamColorFunction(float completionRatio) => Color.Lerp(Color.CornflowerBlue, new Color(169, 100, 237), MathF.Pow(completionRatio, 2.5f)) * Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * 0.5f;
        public Color OuterJetstreamColorFunction(float completionRatio) => Color.Lerp(Color.CornflowerBlue, new Color(100, 237, 237), MathF.Pow(completionRatio, 2.5f)) * Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * 0.2f;
        public Color SpiritColorFunction(float completionRatio) => Color.Lerp(Color.Lerp(Color.White, Color.Cyan, 0.4f + completionRatio), Color.RoyalBlue, completionRatio) * Utils.GetLerpValue(0.8f, 0.54f, completionRatio, true);

        public override bool PreDraw(ref Color lightColor)
        {
            // Jetstream trails
            for (int direction = -1; direction <= 1; direction += 2)
            {
                List<Vector2> trailPositionsInner = new List<Vector2>();
                List<Vector2> trailPositionsOuter = new List<Vector2>();
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    float mergeFactor = Utils.Remap(i, 0f, 8f, 0f, 1f, true);

                    Vector2 trailPosInner = Projectile.Center - Projectile.oldRot[i].ToRotationVector2() * i * 8f;
                    float spreadLengthInner = 20f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
                    Vector2 sinOffsetInner = (Vector2.UnitY * direction * MathF.Sin(i * MathHelper.Pi * 0.125f) * spreadLengthInner * mergeFactor).RotatedBy(Projectile.oldRot[i]);
                    trailPositionsInner.Add(trailPosInner + sinOffsetInner);

                    Vector2 trailPosOuter = Projectile.Center - Projectile.oldRot[i].ToRotationVector2() * i * 12f;
                    float spreadLengthOuter = 32f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
                    Vector2 sinOffsetOuter = (Vector2.UnitY * direction * (6f + MathF.Sin(i * MathHelper.Pi * 0.035f) * spreadLengthOuter * mergeFactor)).RotatedBy(Projectile.oldRot[i]);
                    trailPositionsOuter.Add(trailPosOuter + sinOffsetOuter);
                }

                PrimitiveRenderer.RenderTrail(trailPositionsInner, new(JetstreamWidthFunction, InnerJetstreamColorFunction), 20);
                PrimitiveRenderer.RenderTrail(trailPositionsOuter, new(JetstreamWidthFunction, OuterJetstreamColorFunction), 20);
            }

            // Oops all trails?
            List<Vector2> spiritBodyPositions = new List<Vector2>();
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 3 segments ahead so that it's a little offset forward
                Vector2 spiritPos = Projectile.Center - Projectile.oldRot[i].ToRotationVector2() * (i - 3) * 8f;
                spiritBodyPositions.Add(spiritPos);
            }
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
            PrimitiveRenderer.RenderTrail(spiritBodyPositions, new(SpiritWidthFunction, SpiritColorFunction, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 60);

            // Glow pulses when the ability is about to run out
            if (Projectile.timeLeft < 180)
            {
                Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
                Texture2D glow = GlowTexture.Value;
                Vector2 drawPos = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
                Color glowColor = new Color(255, 170, 204) * (0.2f * MathF.Sin(Projectile.timeLeft * MathHelper.Pi * 0.25f) + Utils.Remap(Projectile.timeLeft, 180f, 0f, 0.2f, 0.6f, true));
                Main.EntitySpriteDraw(glow, drawPos, null, glowColor, Projectile.rotation, glow.Size() * 0.5f, new Vector2(0.6f, 0.5f), SpriteEffects.None);
                Main.spriteBatch.ExitShaderRegion();
            }
            return false;
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            Owner.velocity *= 0.8f;
            Owner.fullRotation = 0f;

            SoundEngine.PlaySound(SoundID.Drown, Projectile.Center);
            for (int i = 0; i < 3; i++)
            {
                Color squareColor = Color.Lerp(Color.Cyan, new Color(255, 170, 204), i * 0.5f) * 0.6f;
                float squareScale = 0.6f + i * 0.2f;
                float squareRot = (i % 2 == 0 ? MathHelper.PiOver4 : 0f);
                Particle square = new CustomSpark(Projectile.Center, Vector2.Zero, "CalamityMod/Particles/GlowSquareParticleThick", false, 18, squareScale, squareColor, Vector2.One, true, true, squareRot, true, glowOpacity: 0.5f, glowCenterScale: squareScale * 1.15f);
                GeneralParticleHandler.SpawnParticle(square);
            }
        }
    }
}
