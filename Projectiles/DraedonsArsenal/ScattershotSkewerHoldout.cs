using System;
using CalamityMod.Cooldowns;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class ScattershotSkewerHoldout : BaseGunHoldoutProjectile
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override int AssociatedItemID => ModContent.ItemType<ScattershotSkewer>();
        public override float MaxOffsetLengthFromArm => 30f;
        public override float RecoilResolveSpeed => 0.1f;
        public override float OffsetXUpwards => -0f;
        public override float BaseOffsetY => -0f;
        public override float OffsetYDownwards => 0f;

        public static float BulletSpeed = 12f;
        public SlotId ChargeSlot;
        public SlotId StartSlot;
        public Color mainColor = Color.Lerp(Color.Red, Color.White, 0f);
        public int time = 0;
        public int chargeAttackTimer = 10;
        public bool doingChargeAttack = false;

        public ref float CurrentChargingFrames => ref Projectile.ai[0];
        public int chargeMax = 0;
        public int downtime = 0;
        public bool ChargeLV1 => CurrentChargingFrames >= chargeMax;

        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.ActiveItem().type)
                Projectile.Kill();
        }
        public override void OnSpawn(IEntitySource source)
        {
            OffsetLengthFromArm = 30;
            Player Owner = Main.player[Projectile.owner];
            chargeMax = (int)(Owner.itemAnimationMax * 2.1f);

            if (Owner.Calamity().arsenalCooldown <= 0)
            {
                SoundStyle sound = new("CalamityMod/Sounds/Item/ScattershotCharge");
                StartSlot = SoundEngine.PlaySound(sound with { Volume = 0.9f }, Projectile.Center);
            }
        }

        public override void HoldoutAI()
        {
            CalamityGlobalItem modItem = Owner.ActiveItem().Calamity();
            Vector2 mountedCenter = Owner.MountedCenter;
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - mountedCenter;
            if (SoundEngine.TryGetActiveSound(ChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;
            if (SoundEngine.TryGetActiveSound(StartSlot, out var StartSound) && StartSound.IsPlaying)
                StartSound.Position = Projectile.Center;

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (((Owner.CantUseHoldout() || (Owner.Calamity().mouseRight || Owner.Calamity().arsenalCooldown > 0)) && downtime == 0 && time >= 1) || doingChargeAttack)
            {
                KeepRefreshingLifetime = false;

                Projectile.timeLeft = Owner.Calamity().mouseRight ? Owner.itemAnimationMax * 2 : Owner.itemAnimationMax;
                downtime = Owner.itemAnimationMax;

                // Big Shot mode
                if ((ChargeLV1 && Owner.Calamity().arsenalCooldown <= 0) || doingChargeAttack)
                {
                    if (chargeAttackTimer == 10)
                    {
                        doingChargeAttack = true;
                        Projectile.timeLeft += chargeAttackTimer;
                        OffsetLengthFromArm -= 20;
                    }
                    else
                    {
                        if (chargeAttackTimer == 3)
                        {
                            OffsetLengthFromArm += 35;
                            ChargeSound?.Stop();
                            SoundStyle sound = new("CalamityMod/Sounds/Item/ScattershotChargeShoot");
                            SoundEngine.PlaySound(sound with { Volume = 0.9f }, Projectile.Center);
                            Owner.Calamity().GeneralScreenShakePower = 6f;
                        }
                        if (chargeAttackTimer <= 0)
                        {
                            Owner.Calamity().arsenalCooldown = 500;
                            Owner.AddCooldown(ArsenalPower.ID, 500);

                            Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                            int charge2Damage = (int)(Projectile.damage * 60); // Seems high but its on a cooldown and is a melee attack so might be okay
                            float charge2KB = Projectile.knockBack * 3f;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity * 2, ModContent.ProjectileType<ScattershotLance>(), charge2Damage, charge2KB, Projectile.owner);

                            for (int i = 0; i <= 25; i++)
                            {
                                Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, 267, shootVelocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.2f, 1.2f), 0, default, Main.rand.NextFloat(0.5f, 1.3f));
                                dust.noGravity = true;
                                dust.color = mainColor;
                            }
                            CurrentChargingFrames = 0;
                            if (modItem.Charge > 0)
                                modItem.Charge -= 0.25f;
                            else
                                Projectile.Kill();

                            chargeAttackTimer = 10;
                            doingChargeAttack = false;
                        }
                    }
                    if (doingChargeAttack)
                        chargeAttackTimer--;
                }
                // Spread Fire mode
                else
                {
                    StartSound?.Stop();
                    OffsetLengthFromArm -= 10;
                    ChargeSound?.Stop();
                    SoundStyle sound = new("CalamityMod/Sounds/Item/ScattershotShoot");
                    SoundEngine.PlaySound(sound with { Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);

                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed * 0.3f;

                    float angle1 = MathHelper.ToRadians(3f);
                    for (int i = 0; i < 2; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - shootVelocity * 8, shootVelocity.RotatedBy(angle1), ModContent.ProjectileType<ScattershotLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - shootVelocity * 4, shootVelocity.RotatedBy(angle1 * 0.7f) * 0.9f, ModContent.ProjectileType<ScattershotLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        angle1 *= -1;
                    }

                    float angle2 = MathHelper.ToRadians(5f);
                    for (int i = 0; i < 2; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - shootVelocity * 8, shootVelocity.RotatedBy(angle2) * 0.95f, ModContent.ProjectileType<ScattershotLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        angle2 *= -1;
                    }
                    CurrentChargingFrames = 0;
                    if (modItem.Charge > 0)
                        modItem.Charge -= 0.1f;
                    else
                        Projectile.Kill();
                }
            }
            else if (downtime == 0)
            {
                CurrentChargingFrames++;
                // Charge-up visuals
                if (CurrentChargingFrames >= 10)
                {
                    float particleScale = Utils.GetLerpValue(0, chargeMax, CurrentChargingFrames, true);
                    float strength = particleScale;
                    Vector3 DustLight = mainColor.ToVector3();
                    Lighting.AddLight(GunTipPosition, DustLight * strength);
                }

                // Full charge dusts
                if (CurrentChargingFrames == chargeMax)
                {
                    OffsetLengthFromArm += 10;
                    ChargeSound?.Stop();
                    for (int i = 0; i < 20; i++)
                    {
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, 66);
                        chargefull.velocity = (MathHelper.TwoPi * i / 20f).ToRotationVector2() * 4f + Owner.velocity * 0.5f;
                        chargefull.scale = Main.rand.NextFloat(0.6f, 0.8f);
                        chargefull.noGravity = true;
                        chargefull.color = mainColor;
                    }
                    SoundStyle sound = new("CalamityMod/Sounds/Item/ScattershotChargeLoop");
                    ChargeSlot = SoundEngine.PlaySound(sound with { Volume = 0.7f, IsLooped = true }, Projectile.Center);
                }

                if (CurrentChargingFrames > 10)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(25, 25);

                        Particle spark = new LineParticle(spawnPos, (GunTipPosition - spawnPos).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1, 4), false, 4, Main.rand.NextFloat(0.3f, 0.45f), Color.Red);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }
            }

            if (!doingChargeAttack)
            {
                if (downtime == 1 && !Owner.Calamity().mouseRight)
                    Projectile.Kill();

                if (downtime > 0)
                    downtime--;

                if ((Owner.Calamity().mouseRight || Owner.Calamity().arsenalCooldown > 0))
                    CurrentChargingFrames = 0;

                if (Owner.Calamity().mouseRight)
                {
                    StartSound?.Stop();
                }
            }
           
            time++;
        }
        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(ChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
            if (SoundEngine.TryGetActiveSound(StartSlot, out var StartSound))
                StartSound?.Stop();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 2)
                return false;

            float fade = Utils.GetLerpValue(chargeMax / 3, chargeMax, CurrentChargingFrames, true);

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/ScattershotSkewer").Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/ScattershotSkewerGlow").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0, -3);

            if (CurrentChargingFrames > 10 && !Owner.CantUseHoldout() && !Owner.Calamity().mouseRight && CurrentChargingFrames < chargeMax)
            {
                float rumble = Utils.GetLerpValue(10, chargeMax, CurrentChargingFrames, true);
                drawPosition += Main.rand.NextVector2Circular(5 * rumble, 5 * rumble);
            }

            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Texture2D pointTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);

            if (CurrentChargingFrames > 10 && downtime == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Color auraColor = Color.White with { A = 0 } * Utils.GetLerpValue(10, chargeMax, CurrentChargingFrames, true) * 0.6f;
                    Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 40f).ToRotationVector2();
                    rotationalDrawOffset *= 3;
                    Main.EntitySpriteDraw(glowTexture, drawPosition + rotationalDrawOffset, null, auraColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);
                }

                float randSize = Main.rand.NextFloat(0.4f, 0.9f);
                Main.EntitySpriteDraw(rechargeTexture, GunTipPosition - Main.screenPosition + Projectile.velocity * 5, null, mainColor with { A = 0 }, Projectile.rotation, rechargeTexture.Size() * 0.5f, 0.25f * Utils.GetLerpValue(0, chargeMax, CurrentChargingFrames, true) * randSize, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(rechargeTexture, GunTipPosition - Main.screenPosition + Projectile.velocity * 5, null, Color.White with { A = 0 }, Projectile.rotation, rechargeTexture.Size() * 0.5f, 0.15f * Utils.GetLerpValue(0, chargeMax, CurrentChargingFrames, true) * randSize, SpriteEffects.None, 0);

                //Main.EntitySpriteDraw(pointTexture, tipPosition - Main.screenPosition + (Projectile.velocity * 15 * fade), null, mainColor with { A = 0 } * fade, Projectile.velocity.RotatedBy(MathHelper.ToRadians(90f)).ToRotation(), pointTexture.Size() * 0.5f, new Vector2(0.5f, 0.9f) * 0.035f * fade * randSize * (CurrentChargingFrames == chargeMax ? 1.5f : 1), flipSprite);
            }

            Main.EntitySpriteDraw(glowTexture, drawPosition, null, Color.White, drawRotation, rotationPoint, Projectile.scale, flipSprite);
            return false;
        }
    }
}
