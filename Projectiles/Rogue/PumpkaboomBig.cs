using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Ravager;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Sounds;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    [PierceResistException]
    public class PumpkaboomBig : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Rogue/PumpkaboomBig";
        public ref float time => ref Projectile.ai[0];
        public ref float skipExplosionSound => ref Projectile.ai[1];
        public Player Owner => Main.player[Projectile.owner];
        public int tileHits = 0;
        public bool flung = false;
        public float charge = 0;
        public bool hasReachedFullCharge = false;
        public bool hasStoppedHolding = false;
        public bool doneHitting = false;

        public bool stuck = false;
        public int stuckNPC = -1;
        Vector2 placementDistance;
        Vector2 placementVelocity;

        private bool beginStretchAnim = false;
        private float progress = -1;

        public Color mainColor = Color.White;
        public Color c1 = new Color(255, 117, 24); // Orange
        public Color c2 = new Color(168, 47, 57); // Maroon / Darkish Red
        public float glowProgress = -1;
        public Color shiftColor;

        public NPC markedTarget;
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 1200;
            Projectile.DamageType = RogueDamageClass.Instance;
        }
        public override bool ShouldUpdatePosition() => flung && !stuck;
        public override bool? CanDamage() => (flung && tileHits == 0 ? null : false);

        private bool hasDealtDamage = false;

        public override bool? CanHitNPC(NPC target)
        {
            if (hasDealtDamage)
                return false;

            return null;
        }

        public override void AI()
        {
            // Outline color shifting
            float rate = (Main.GlobalTimeWrappedHourly * 6);
            List<Color> eColors = new List<Color>()
            {
                c1,
                c2
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            mainColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            // When an NPC is hit by the bomb, stick to it.
            if (stuck)
            {
                if (stuckNPC >= 0 && Main.npc[stuckNPC].active)
                {
                    Projectile.Center = Main.npc[stuckNPC].Center + placementVelocity * placementDistance;
                }

                else
                {
                    Projectile.Kill();
                }

                if (Projectile.timeLeft == 109)
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/PumpkaboomStealthTicking") with { Pitch = 0f, Volume = 1f, MaxInstances = 4 }, Projectile.Center);
                }

                if (Projectile.timeLeft < 110)
                {
                    // Pull in small NPCs
                    float pullStrength = 9f;
                    float centerShift = 1f;
                    float maxPullDistance = 240f;

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC target = Main.npc[i];
                        if (target != null && target.CanBeMoved(true) && Collision.CanHit(Projectile.Center, 1, 1, target.Center, 1, 1) && Vector2.Distance(target.Center, Projectile.Center) < maxPullDistance)
                        {
                            // Avoid pulling the NPC the bomb is stuck to
                            if (target.whoAmI == stuckNPC)
                                continue;

                            Vector2 moveDir = target.Center.DirectionTo(Projectile.Center).SafeNormalize(Vector2.UnitX);
                            target.velocity = Vector2.Lerp(target.velocity, moveDir * pullStrength, 0.15f);
                            target.Center += moveDir * centerShift;
                        }
                    }
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 dustVel2 = (Vector2.UnitX).RotatedByRandom(100) * Main.rand.NextFloat(9.5f, 13f);
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center + dustVel2.SafeNormalize(Vector2.UnitX) * 160, ModContent.DustType<SquashDust>(), -dustVel2 * 1.2f, 0, default, Main.rand.NextFloat(0.9f, 1.4f));
                        dust2.noGravity = true;
                        dust2.fadeIn = 0.8f;
                        dust2.color = c1;
                    }
                }
                return;
            }

            if (flung)
            {
                // stuckNPC index
                Projectile.localAI[0]++;
                Projectile.localAI[1] = 5; // The item can be used again once flung.

                Projectile.velocity.Y += 0.22f;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // While in hand
            else
            {
                Projectile.velocity = Owner.velocity;
                float completion = time / (Owner.HeldItem.useAnimation * 0.7f); // The completion of the throw animation. Scales with speed.
                if (completion >= 1 || hasReachedFullCharge) // The moment of being thrown.
                {
                    time = -1;
                    Projectile.Center = Owner.Center;
                    Vector2 velocity = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld);
                    Projectile.velocity = velocity * 16;

                    Projectile.tileCollide = true;
                    flung = true;
                }

                else
                {
                    if (Main.mouseLeft && !hasStoppedHolding) // The charging up.
                    {
                        if (completion >= 0.7f && completion <= 0.8f)
                        {
                            time--;
                            if (charge < 1)
                                charge += 0.5f;

                            if (charge >= 1 && !hasReachedFullCharge)
                            {
                                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                                hasReachedFullCharge = true;
                            }
                        }
                    }
                    else
                    {
                        if (!hasReachedFullCharge)
                        {
                            Projectile.localAI[1] = 5;
                            Projectile.ai[1] = 1; // Skip explosion fx
                            Projectile.Kill();
                        }
                        hasStoppedHolding = true;
                    }

                    Owner.direction = Math.Sign(Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).X);
                    float grenadeRot = 0;
                    if (completion >= 0.7f)
                    {
                        float completionLerp = (float)Math.Pow(Utils.GetLerpValue(0.7f, 1f, completion, true), 7);
                        grenadeRot = MathHelper.ToRadians(MathHelper.Lerp(-75, 130f, completionLerp) * Owner.direction);
                    }
                    else
                    {
                        float completionLerp = (float)Math.Pow(Utils.GetLerpValue(0f, 0.7f, completion, true), 2);
                        grenadeRot = MathHelper.ToRadians(MathHelper.Lerp(120, -75f, completionLerp) * Owner.direction);
                    }
                    grenadeRot += Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).ToRotation();
                    Vector2 grenadePos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, grenadeRot) + new Vector2(Owner.direction == 1 ? 5 : -3, Owner.direction == 1 ? -24 : -4).RotatedBy(grenadeRot);
                    float completionLerp2 = (float)Math.Pow(Utils.GetLerpValue(0f, 0.7f, completion, true), 2);
                    float grenadeHalfRot = MathHelper.ToRadians(MathHelper.Lerp(120, -75f, completionLerp2) * Owner.direction);

                    Projectile.Center = grenadePos;
                    Projectile.rotation = grenadeRot - MathHelper.ToRadians(25 * grenadeHalfRot) + (Owner.direction == 1 ? MathHelper.ToRadians(180) : 0);

                    Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).ToRotation() - MathHelper.ToRadians(90));
                    Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, grenadeRot - (Owner.direction == 1 ? MathHelper.ToRadians(180) : MathHelper.ToRadians(0)));
                }
            }
            time++;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            stuck = true;
            stuckNPC = target.whoAmI;

            Projectile.localAI[1] = 5; // Reset firing state

            // See PreDraw
            beginStretchAnim = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundStyle w = new("CalamityMod/Sounds/Item/WulfrumScrewdriverThud");
            SoundEngine.PlaySound(w with { Volume = 0.7f, Pitch = 0f, MaxInstances = 6 }, Projectile.Center);

            stuck = true;
            stuckNPC = target.whoAmI;

            placementDistance = new(-Vector2.Distance(target.Center, Projectile.Center));
            placementVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.oldVelocity.ToRotation() + MathHelper.PiOver2;
            Projectile.ai[1] = Projectile.rotation;

            hasDealtDamage = true;
            Projectile.localAI[1] = 5; // Reset firing state

            glowProgress = 0; // Start glowing until death

            Projectile.timeLeft = 110; // Lasts as long as sfx

            for (int i = 0; i < 6; i++)
            {
                int sparkLifetime = Main.rand.Next(13, 22);
                float sparkScale = Main.rand.NextFloat(0.55f, 1.1f);
                Color sparkColor = Main.rand.NextBool() ? c1 : c2;

                Vector2 burstDirection = Projectile.oldVelocity + new Vector2(Main.rand.NextFloat(-2.25f, 2.25f), Main.rand.NextFloat(-6.75f, 6.75f));
                Vector2 spawnPos = target.Center + burstDirection * target.width * 0.15f;
                Vector2 sparkVelocity = burstDirection * Main.rand.NextFloat(0.7f, 1.3f);

                if (Main.rand.NextBool())
                {
                    AltSparkParticle spark = new AltSparkParticle(spawnPos, sparkVelocity, false, sparkLifetime, sparkScale, sparkColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                else
                {
                    LineParticle spark = new LineParticle(spawnPos, sparkVelocity, false, sparkLifetime, sparkScale, sparkColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.localAI[1] = 5; // Reset firing state

            if (Projectile.ai[1] != 1)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/PumpkinExplode1") with { Pitch = 0f, Volume = 0.75f }, Projectile.Center);
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/FlakKrakenShoot") with { Pitch = 0f, Volume = 0.6f }, Projectile.Center);
                Owner.Calamity().GeneralScreenShakePower = 5f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PumpkaboomBoomBig>(), Projectile.damage * 2, Projectile.knockBack * 2, Projectile.owner, 0);

                float scale = 0.18f;
                Vector2 spawnPos = Projectile.Center;
                Particle blastRing = new CustomPulse(spawnPos, Vector2.Zero, c1, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0.07f, scale, 10);
                GeneralParticleHandler.SpawnParticle(blastRing);
                Particle blastRing2 = new CustomPulse(spawnPos, Vector2.Zero, c1 * 0.33f, "CalamityMod/Particles/HighResHollowCircleHardEdge", Vector2.One, Main.rand.NextFloat(-10, 10), 0.07f, scale * 1.66f, 11);
                GeneralParticleHandler.SpawnParticle(blastRing2);
                for (int k = 0; k < 16; k++)
                {
                    Vector2 velocity = new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.7f, 1.3f);
                    Particle spark = new SparkParticle(Projectile.Center + velocity, velocity, false, 45, Main.rand.NextFloat(0.95f, 1.35f), c1);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                for (int k = 0; k < 30; k++)
                {
                    Vector2 velocity = new Vector2(10, 10).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.2f);
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + velocity, 259, velocity);
                    dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                    dust2.noGravity = true;
                }
            }

        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 21, targetHitbox);
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (beginStretchAnim)
            {
                progress = 0f;
                beginStretchAnim = false;
            }

            if (progress >= 0f)
            {
                progress += 0.02f;

                float stretchFactorX, stretchFactorY;

                if (progress < 0.5f) // Stretch
                {
                    float completion = progress / 0.5f;
                    stretchFactorX = MathHelper.Lerp(1.7f, 0.7f, completion);
                    stretchFactorY = MathHelper.Lerp(0.7f, 1.7f, completion);
                }
                else // Squash
                {
                    float completion = (progress - 0.5f) / 0.5f;
                    stretchFactorX = MathHelper.Lerp(0.7f, 1f, completion);
                    stretchFactorY = MathHelper.Lerp(1.7f, 1f, completion);
                }

                if (progress >= 1f)
                {
                    stretchFactorX = 1f;
                    stretchFactorY = 1f;
                    progress = -1f;
                }

                Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
                Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                Vector2 finalSyringeScale = new Vector2(stretchFactorX, stretchFactorY);
                Vector2 finalPos = Projectile.Center - Main.screenPosition;

                Main.spriteBatch.Draw(texture, finalPos, null, lightColor, Projectile.rotation, origin, finalSyringeScale, SpriteEffects.None, 0f);

                return false;
            }


            Texture2D mainTexture = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects spriteEffects = SpriteEffects.None;

            Vector2 drawOrigin = mainTexture.Size() / 2f;
            float drawScale = Projectile.scale;
            float drawRotation = Projectile.rotation;
            Vector2 drawPosition;

            if (stuck && glowProgress >= 0f)
            {
                glowProgress += 0.03f;

                float glowSine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 25f); // Period of full pulse
                float pulse = MathHelper.Lerp(0.7f, 1f, glowSine); // Least protruding to most protruding
                float lifeFadeIn = Utils.GetLerpValue(110, 0, Projectile.timeLeft, true); // How much the glow overall fades in as it gets closer to exploding
                float finalGlowIntensity = pulse * lifeFadeIn;
                drawPosition = Projectile.Center - Main.screenPosition;

                // Create 20 copies of the projectile that draw behind with mainColor to create a glowy outline effect
                for (int i = 0; i < 20; i++)
                {
                    float rotationOffset = (MathHelper.TwoPi * i / 15);
                    Vector2 glowOffset = rotationOffset.ToRotationVector2() * (3f + glowSine * 1f) * finalGlowIntensity;

                    Main.spriteBatch.Draw(mainTexture, drawPosition + glowOffset, null, mainColor with { A = 0 } * finalGlowIntensity * 0.4f, drawRotation, drawOrigin, drawScale, spriteEffects, 0f);
                }
            }

            if (!flung)
            {
                spriteEffects = SpriteEffects.FlipVertically;

                float completion = time / (Owner.HeldItem.useAnimation * 0.7f);
                float completionLerp2 = (float)Math.Pow(Utils.GetLerpValue(0f, 0.7f, completion, true), 2);
                float grenadeRot = MathHelper.ToRadians(MathHelper.Lerp(120, -75f, completionLerp2) * Owner.direction);
                grenadeRot += Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).ToRotation();

                Vector2 grenadePos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, grenadeRot) + new Vector2(Owner.direction == 1 ? 5 : 5, Owner.direction == 1 ? -38 : 22).RotatedBy(grenadeRot);
                drawPosition = grenadePos - Main.screenPosition;
                drawRotation = grenadeRot - MathHelper.ToRadians(25 * completionLerp2) + (Owner.direction == 1 ? MathHelper.ToRadians(180) : 0);
            }
            else 
            {
                if (stuck)
                {
                    drawRotation = Projectile.ai[1];
                }
                drawPosition = Projectile.Center - Main.screenPosition;
            }

            // Draw it
            Main.spriteBatch.Draw(mainTexture, drawPosition, null, lightColor * (1f - Projectile.alpha / 255f), drawRotation, drawOrigin, drawScale, spriteEffects, 0f);

            return false;
        }
    }
}
