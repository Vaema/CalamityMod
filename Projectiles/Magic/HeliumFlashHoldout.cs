using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using CalamityMod.Projectiles.Ranged;
using System.Reflection.Metadata;

namespace CalamityMod.Projectiles.Magic
{
    internal class HeliumFlashHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<HeliumFlash>();
        public override float MaxOffsetLengthFromArm => 60f;
        public override float BaseOffsetY => 4.5f;
        public override string Texture => "CalamityMod/Projectiles/Magic/HeliumFlashHoldout";
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitX.RotatedBy(Projectile.rotation) * 24;
        
        private ref float CurrentChargingFrames => ref Projectile.ai[0];
        private bool FullyCharged => CurrentChargingFrames >= HeliumFlash.FullChargeFrames;
        public SlotId HeliumChargeSlot;
        public static float BulletSpeed = 40f;

        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.ActiveItem().type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (SoundEngine.TryGetActiveSound(HeliumChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout())
            {
                KeepRefreshingLifetime = false;

                if (Projectile.ai[1] != 1f)
                {
                    Projectile.timeLeft = HeliumFlash.AftershotCooldownFrames;

                    SoundEngine.PlaySound(HeliumFlash.CancelCharge, Projectile.Center);
                    ChargeSound?.Stop();

                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                    if (FullyCharged)
                    {
                        SoundEngine.PlaySound(HeliumFlash.ChargeFire, Projectile.Center);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<VolatileStarcore>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0);
                        Particle pulse = new CustomPulse(GunTipPosition, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.05f, 22);
                        GeneralParticleHandler.SpawnParticle(pulse);
                        Particle pulse2 = new CustomPulse(GunTipPosition, Vector2.Zero, Color.Red, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.08f, 22);
                        GeneralParticleHandler.SpawnParticle(pulse2);
                        Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
                        Particle pulse3 = new GlowSparkParticle(GunTipPosition, shootDirection * 18, false, 6, 0.057f, Color.OrangeRed, new Vector2(1.7f, 0.8f), true);
                        GeneralParticleHandler.SpawnParticle(pulse3);
                        for (int i = 0; i <= 18; i++)
                        {
                            Vector2 sparkVelocity = shootVelocity / 2f;

                            float sparkScale1 = Main.rand.NextFloat(0.3f, 0.8f);
                            Vector2 sparkvelocity1 = sparkVelocity.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.5f, 0.7f);
                            SparkParticle spark1 = new SparkParticle(GunTipPosition, sparkvelocity1, false, 40, sparkScale1, Main.rand.NextBool() ? Color.Red : Color.DarkRed);
                            GeneralParticleHandler.SpawnParticle(spark1);

                            float sparkScale2 = Main.rand.NextFloat(0.4f, 1f);
                            Vector2 sparkvelocity2 = sparkVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.9f, 1.6f);
                            SparkParticle spark2 = new SparkParticle(GunTipPosition, sparkvelocity2, false, 40, sparkScale2, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }
                    }
                    Projectile.ai[1] = 1f;
                }
            }
            else
            {
                CurrentChargingFrames++;

                // Sounds
                if (FullyCharged)
                {
                    if ((CurrentChargingFrames - HeliumFlash.FullChargeFrames) % HeliumFlash.ChargeLoopSoundFrames == 0)
                        HeliumChargeSlot = SoundEngine.PlaySound(HeliumFlash.ChargeLoop, Projectile.Center);
                    if (Main.rand.NextBool(10))
                    {
                        Particle lightning = new ThunderBoltVFX(() => GunTipPosition, Main.rand.NextFloat(MathHelper.TwoPi), 0.10f, Color.Red, 30, 0);
                        GeneralParticleHandler.SpawnParticle(lightning);
                    }
                }
                else if (CurrentChargingFrames == 10)
                    HeliumChargeSlot = SoundEngine.PlaySound(HeliumFlash.Charge, Projectile.Center);

                // Charge-up visuals
                if (CurrentChargingFrames >= 10)
                {
                    if (!FullyCharged)
                    {
                        Particle streak = new ManaDrainStreak(Owner, Main.rand.NextFloat(0.06f + (CurrentChargingFrames / 180), 0.08f + (CurrentChargingFrames / 180)), Main.rand.NextVector2CircularEdge(2.5f, 2.5f) * Main.rand.NextFloat(0.3f * CurrentChargingFrames, 0.3f * CurrentChargingFrames), 0f, Color.Red, Color.Orange, 7, GunTipPosition);
                        GeneralParticleHandler.SpawnParticle(streak);
                        if (Main.rand.NextBool(5))
                        {
                            Particle lightning = new ThunderBoltVFX(() => GunTipPosition, Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextFloat(0.01f + (CurrentChargingFrames / 1200), 0.08f + (CurrentChargingFrames / 1200)), Color.Red, 30, 0);
                            GeneralParticleHandler.SpawnParticle(lightning);
                        }
                    }

                    float orbScale = MathHelper.Clamp(CurrentChargingFrames, 0f, HeliumFlash.FullChargeFrames);
                    Particle orb = new GenericBloom(GunTipPosition, Projectile.velocity, Color.OrangeRed, orbScale / 200f, 2);
                    GeneralParticleHandler.SpawnParticle(orb);
                    Particle orb2 = new CritSpark(GunTipPosition, Projectile.velocity, Color.Red, Color.OrangeRed, orbScale / 25f, 2, 0.5f);
                    GeneralParticleHandler.SpawnParticle(orb2);
                }

                // Full charge effects
                if (CurrentChargingFrames == HeliumFlash.FullChargeFrames)
                {
                    SoundEngine.PlaySound(HeliumFlash.FullCharge, Projectile.Center);
                    Particle pulse = new DetailedExplosion(GunTipPosition, Vector2.Zero, Color.Red, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.18f, 22, false);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }   
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(HeliumChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (!Owner.CantUseHoldout())
            {
                float rumble = MathHelper.Clamp(CurrentChargingFrames, 0f, HeliumFlash.FullChargeFrames);
                drawPosition += Main.rand.NextVector2Circular(rumble / 25f, rumble / 25f);
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            return false;
        }
    }
}
