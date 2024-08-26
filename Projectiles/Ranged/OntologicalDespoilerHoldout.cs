using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class OntologicalDespoilerHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<OntologicalDespoiler>();
        public override float MaxOffsetLengthFromArm => 29f;
        public override float OffsetXUpwards => -8f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 8f;
        public override float RecoilResolveSpeed => 0.5f;
        public override Vector2 GunTipPosition => Projectile.Center + (Projectile.velocity * 45).RotatedBy(0.18f * Projectile.direction);

        public int FramesPerLoad = 9;
        public int MaxLoadableShots = 23;
        public float BulletSpeed = 6.5f;
        public SlotId OntologicalChargeSlot;
        public int Time = 0;

        public ref float CurrentChargingFrames => ref Projectile.ai[0];
        public float ShotsLoaded = 1;
        public Color baseColor = Color.White;

        public Color color1 = Color.DarkMagenta;
        public Color color2 = Color.DarkOrchid;
        public Color color3 = Color.Purple;
        public Color color4 = Color.BlueViolet;

        public bool hasBegunFiring = false;
        public bool Positive => Projectile.ai[2] == 0;
        public ref float ShootRecoilTimer => ref Projectile.ai[1]; // Dual functions for rapid fire shooting cooldown and recoil

        public int AftershotCooldownFrames = 8;
        public int Charge1Frames = 156;
        public int Charge2Frames = 308;

        public int fireFrameCounter = 0;
        public int fireFrame = 0;
        public int flashFrameCounter = 0;
        public int flashFrame = 0;
        public bool showFlash = false;
        public Vector2 flashPos;
        public float flashRot;
        public Vector2 voidPlacement;
        public bool ChargeLV1 => CurrentChargingFrames >= Charge1Frames;
        public bool ChargeLV2 => CurrentChargingFrames >= Charge2Frames;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }
        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.ActiveItem().type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (SoundEngine.TryGetActiveSound(OntologicalChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;

            float rate = (Main.GlobalTimeWrappedHourly * 15);
            List<Color> eColors = new List<Color>()
                {
                    color1,
                    color2,
                    color3,
                    color4
                };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            baseColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            if (!Positive)
                baseColor = Color.White;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 1)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5)
            {
                Projectile.frame = 0;
            }

            fireFrameCounter++;
            if (fireFrameCounter > 1)
            {
                fireFrame++;
                fireFrameCounter = 0;
            }
            if (fireFrame > 5)
                fireFrame = 0;

            if (showFlash)
            {
                flashFrameCounter++;
                if (flashFrameCounter > (flashFrame == 2 ? 7 : 1))
                {
                    flashFrame++;
                    flashFrameCounter = 0;
                }
                if (flashFrame > 6)
                    showFlash = false;
            }
            if (Time % 2 == 0)
                voidPlacement = Main.rand.NextVector2Circular(10.5f, 10.5f);
            if (Projectile.ai[2] >= 10) // Changing Mode
            {
                Owner.Calamity().despoilerNerf = false;
                Projectile.frame = 6;
                fireFrame = 6;
                ShotsLoaded = 0;
                if (Time == 0)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        Color useColor = Main.rand.Next(4) switch
                        {
                            0 => color1,
                            1 => color2,
                            2 => color3,
                            _ => color4,
                        };
                        Dust dust = Dust.NewDustPerfect(Owner.Center, Main.rand.NextBool() ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.9f, 1.25f);
                        dust.color = Projectile.ai[2] == 15 ? Color.White : useColor;
                    }
                    Projectile.timeLeft = AftershotCooldownFrames * 2;
                    OffsetLengthFromArm = 25f;
                }
            }

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout() || Projectile.ai[2] >= 10)
            {
                if (Projectile.ai[2] < 10)
                    Owner.Calamity().despoilerNerf = true;
                hasBegunFiring = true;
                KeepRefreshingLifetime = false;

                // Big Shot mode
                if (ChargeLV2)
                {
                    if (ShotsLoaded > 0)
                    {
                        // Set lifespan to double of remaining cooldown so that it can finish the recoil animation
                        Projectile.timeLeft = AftershotCooldownFrames * 2;

                        ChargeSound?.Stop();

                        if (Positive)
                        {

                        }
                        else
                        {

                        }
                        SoundEngine.PlaySound(OntologicalDespoiler.BigShot, Projectile.position);

                        Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                        int charge2Damage = Projectile.damage * 20;
                        float charge2KB = Projectile.knockBack * 3f;
                        //Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<NovaChargedShot>(), charge2Damage, charge2KB, Projectile.owner);
                        Owner.Calamity().GeneralScreenShakePower = 9.5f;
                        flashPos = GunTipPosition + Projectile.velocity * 90;
                        flashRot = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
                        showFlash = true;
                        for (int i = 0; i < 20; i++)
                        {
                            Color useColor = Main.rand.Next(4) switch
                            {
                                0 => color1,
                                1 => color2,
                                2 => color3,
                                _ => color4,
                            };
                            float rand = Main.rand.NextFloat(0, 2.5f);
                            Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, ModContent.DustType<VoidDust>(), shootVelocity.RotatedByRandom(0.12f + rand * 0.2f) * (Main.rand.NextFloat(2.5f, 6.3f) - rand));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(1.55f, 1.85f);
                            dust.color = Positive ? useColor : baseColor;
                        }

                        ShotsLoaded = 0;
                        ShootRecoilTimer = 34f;
                        OffsetLengthFromArm -= 35f;
                    }
                    // Retracting recoil
                    else if (ShootRecoilTimer > 0)
                        ShootRecoilTimer -= 2;
                }
                // Rapid Fire mode
                else if (ShotsLoaded > 0)
                {
                    // While bullets are remaining, refresh the lifespan; it will not refresh again after bullets run out
                    Projectile.timeLeft = AftershotCooldownFrames;

                    // Retract recoil & shoot faster if charged
                    ShootRecoilTimer -= ChargeLV1 ? 2.5f : 2f;

                    if (ShootRecoilTimer <= 0f)
                    {
                        ChargeSound?.Stop();

                        Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                        Vector2 fireVec = shootVelocity * Main.rand.NextFloat(0.9f, 1.1f);
                        if (Positive)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                float angle = i == 0 ? -0.25f : i == 2 ? 0.25f : 0;
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, fireVec.RotatedBy(angle) * (1 - Math.Abs(angle * 0.4f)), ModContent.ProjectileType<OntologicalDespoilerShot>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner, 0, 0, i);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, fireVec.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.8f, 1f), ModContent.ProjectileType<OntologicalDespoilerShot>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner, 0, 0, 5);
                            }
                        }

                        SoundEngine.PlaySound(OntologicalDespoiler.SmallShot, Projectile.position);
                        for (int i = 0; i < 3; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, !Positive ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), shootVelocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 2.3f));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(1.15f, 1.35f);
                            dust.color = baseColor;
                        }

                        ShotsLoaded -= (Positive ? 2 : 1);
                        ShootRecoilTimer = Positive ? 16 : 12f;
                        OffsetLengthFromArm -= 3.2f;
                    }
                }
                // Retracting any remaining recoil
                else if (ShootRecoilTimer > 0)
                    ShootRecoilTimer -= 2;
            }
            else
            {
                // Loads shots until maxed out
                if (ShotsLoaded < MaxLoadableShots && CurrentChargingFrames % FramesPerLoad == 0)
                    ShotsLoaded++;

                CurrentChargingFrames += 2 * (Owner.Calamity().despoilerNerf ? 0.5f : 1);

                // Sounds
                if (ChargeLV1)
                {
                    // Pulse sounds play independently of the loop
                    if (CurrentChargingFrames == Charge2Frames)
                        SoundEngine.PlaySound(OntologicalDespoiler.ChargeLV2, Projectile.Center);
                    else if (CurrentChargingFrames == Charge1Frames)
                    {
                        SoundEngine.PlaySound(OntologicalDespoiler.ChargeLV1, Projectile.Center);
                        ShotsLoaded = MaxLoadableShots;
                    }

                    if ((CurrentChargingFrames - Charge1Frames) % (OntologicalDespoiler.ChargeLoopSoundFrames * 2) == 0)
                        OntologicalChargeSlot = SoundEngine.PlaySound(OntologicalDespoiler.ChargeLoop, Projectile.Center);
                }
                else if (CurrentChargingFrames == 10)
                    OntologicalChargeSlot = SoundEngine.PlaySound(OntologicalDespoiler.ChargeStart, Projectile.Center);

                // Charge-up visuals
                if (CurrentChargingFrames >= 10 && (Positive || (!Positive && !ChargeLV2)))
                {
                    float particleScale = MathHelper.Clamp(CurrentChargingFrames, 0f, Charge2Frames);
                    for (int i = 0; i < (ChargeLV2 ? 3 : ChargeLV1 ? 2 : 1); i++)
                    {
                        //SparkParticle spark2 = new SparkParticle((GunTipPosition - Projectile.velocity * 4) + Main.rand.NextVector2Circular(12, 12), -Projectile.velocity * Main.rand.NextFloat(16.1f, 30.8f), false, Main.rand.Next(2, 7), Main.rand.NextFloat(particleScale / 350f, particleScale / 270f), baseColor);
                        //GeneralParticleHandler.SpawnParticle(spark2);
                        Vector2 dustVel = -Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(1.1f, 15.8f);
                        Vector2 addedPlace = Positive ? Vector2.Zero : dustVel * 3;
                        Dust dust = Dust.NewDustPerfect(GunTipPosition + (Positive ? Vector2.Zero : (dustVel * 15 * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true))), !Positive ? ModContent.DustType<VoidDust>() : ChargeLV1 ? Main.rand.NextBool((int)(50 * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true) + 1)) ? ModContent.DustType<LightDust>() : ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), dustVel - addedPlace * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.5f, 1.25f) * Utils.GetLerpValue(0, Charge1Frames, CurrentChargingFrames, true);
                        dust.color = baseColor;
                    }
                    //Particle orb2 = new GenericBloom(GunTipPosition, Projectile.velocity, Color.Black * 0.5f, (particleScale / 350f) * Main.rand.NextFloat(0.9f, 1.1f), 2, false, false);
                    //GeneralParticleHandler.SpawnParticle(orb2);
                    Particle orb = new GenericBloom(GunTipPosition, Projectile.velocity, Positive ? Color.Lerp(Color.White, Color.Black, Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true)) : Color.Black * Utils.GetLerpValue(0, Charge1Frames, CurrentChargingFrames, true), (particleScale / 400f) * Main.rand.NextFloat(0.9f, 1.1f), 2, false, false);
                    GeneralParticleHandler.SpawnParticle(orb);

                    float strength = particleScale / 45f;
                    Vector3 DustLight = baseColor.ToVector3() * 0.2f;
                    Lighting.AddLight(GunTipPosition, DustLight * strength);
                }
                if (ChargeLV2 && !Positive)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Particle orb = new CustomSpark(GunTipPosition, Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(1.1f, 15.8f), "CalamityMod/Particles/PearlParticleGlow", false, 3, Main.rand.NextFloat(1.8f, 1.9f), Color.Black, new Vector2(0.3f, 1.3f), false, false);
                        GeneralParticleHandler.SpawnParticle(orb);
                        Vector2 dustVel = -Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(1.1f, 15.8f);
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<VoidDust>(), dustVel);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.5f, 1.25f) * Utils.GetLerpValue(0, Charge1Frames, CurrentChargingFrames, true);
                        dust.color = baseColor;
                    }
                }

                // Full charge dusts
                if (CurrentChargingFrames == Charge1Frames)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        Color useColor = Main.rand.Next(4) switch
                        {
                            0 => color1,
                            1 => color2,
                            2 => color3,
                            _ => color4,
                        };
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<LightDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 16f).ToRotationVector2() * 16f * (i % 2 == 0 ? 0.7f : 1);
                        chargefull.scale = Main.rand.NextFloat(1.1f, 1.3f);
                        chargefull.noGravity = true;
                        chargefull.color = Positive ? useColor : baseColor;
                    }
                }
                if (CurrentChargingFrames == Charge2Frames)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        Color useColor = Main.rand.Next(4) switch
                        {
                            0 => color1,
                            1 => color2,
                            2 => color3,
                            _ => color4,
                        };
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<LightDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 25f).ToRotationVector2() * 22f * (i % 2 == 0 ? 0.7f : 1);
                        chargefull.scale = Main.rand.NextFloat(1.45f, 1.55f);
                        chargefull.noGravity = true;
                        chargefull.color = Positive ? useColor : baseColor;
                    }
                }
            }
            if (!hasBegunFiring)
            {
                Projectile.frame = 6;
                fireFrame = 6;
            }
            else
            {
                Lighting.AddLight(Projectile.Center, baseColor.ToVector3() * 0.75f);
            }
            Time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(OntologicalChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Time < 3)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerHoldout").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 rotationPoint = frame.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D fireTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerFlame").Value;
            Texture2D fireTexture2 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerFlame2").Value;
            Rectangle frame2 = fireTexture.Frame(1, 7, 0, fireFrame);
            Vector2 rotationPoint2 = frame2.Size() * 0.5f;

            Texture2D flashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerFlash").Value;
            Rectangle frame3 = fireTexture.Frame(1, 6, 0, flashFrame);
            Vector2 rotationPoint3 = frame3.Size() * 0.5f;

            Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/FlameExplosion").Value;
            Texture2D rechargeTexture2 = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;
            // Glow Orb
            float randSize = Main.rand.NextFloat(0.9f, 1.1f);
            if (!hasBegunFiring)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(rechargeTexture, GunTipPosition - Main.screenPosition, null, baseColor with { A = 0 }, Projectile.rotation + Main.rand.NextFloat(-30, 30), rechargeTexture.Size() * 0.5f, 0.028f * Utils.GetLerpValue(0, Charge2Frames, CurrentChargingFrames, true) * randSize, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(rechargeTexture2, GunTipPosition - Main.screenPosition, null, Positive ? Color.Lerp(Color.White, Color.Black, Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true)) : Color.Black, Projectile.rotation + Main.rand.NextFloat(-30, 30), rechargeTexture2.Size() * 0.5f, 0.58f * Utils.GetLerpValue(0, Charge2Frames, CurrentChargingFrames, true) * randSize, SpriteEffects.None, 0);
            }
            if (!Owner.CantUseHoldout())
            {
                float rumble = MathHelper.Clamp(CurrentChargingFrames, 0f, Charge2Frames);
                drawPosition += Main.rand.NextVector2Circular(rumble / 120f, rumble / 120f);
            }

            if (ChargeLV1 && !Positive && !hasBegunFiring)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 placement = drawPosition + voidPlacement;
                    Main.EntitySpriteDraw(texture, placement, frame, baseColor with { A = 0 } * 0.3f * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true), drawRotation, rotationPoint, new Vector2(Main.rand.NextFloat(0.7f, 1f), Main.rand.NextFloat(0.8f, 1.2f)) * Projectile.scale * Owner.gravDir * 1.1f, flipSprite);
                    Main.EntitySpriteDraw(texture, placement, frame, Color.Black * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true), drawRotation, rotationPoint, new Vector2(Main.rand.NextFloat(0.7f, 1f), Main.rand.NextFloat(0.6f, 1f)) * Projectile.scale * Owner.gravDir, flipSprite);
                }
            }
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            if (Projectile.frame < 6)
            {
                if (Positive && false)
                    Main.EntitySpriteDraw(fireTexture, drawPosition, frame2, baseColor with { A = 0 }, drawRotation, rotationPoint2, Projectile.scale * Owner.gravDir, flipSprite);
                else
                {
                    for (int i = 0; i< 4; i++)
                        Main.EntitySpriteDraw(fireTexture, drawPosition + Main.rand.NextVector2Circular(4.5f, 4.5f), frame2, baseColor with { A = 0 } * 0.3f, drawRotation, rotationPoint2, Projectile.scale * Owner.gravDir, flipSprite);
                    Main.EntitySpriteDraw(Positive ? fireTexture : fireTexture2, drawPosition, frame2, Positive ? baseColor with { A = 0 } : baseColor, drawRotation, rotationPoint2, Projectile.scale * Owner.gravDir, flipSprite);
                }
            }
            if (showFlash)
            {
                for (int i = 0; i < 4; i++)
                    Main.EntitySpriteDraw(flashTexture, flashPos - Main.screenPosition + Main.rand.NextVector2Circular(4.5f, 4.5f), frame3, baseColor with { A = 0 } * 0.3f, flashRot, rotationPoint3, new Vector2(3, 0.8f) * Projectile.scale * Owner.gravDir, flipSprite);
                Main.EntitySpriteDraw(flashTexture, flashPos - Main.screenPosition, frame3, Color.Black, flashRot, rotationPoint3, new Vector2(3, 0.8f) * Projectile.scale * Owner.gravDir, flipSprite);
            }

            return false;
        }
    }
}
