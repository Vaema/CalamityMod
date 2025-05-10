using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Steamworks;
using CalamityMod.Projectiles.Turret;
using Mono.Cecil;

namespace CalamityMod.Projectiles.Ranged
{
    public class ArcherfishHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Archerfish>();
        public override float MaxOffsetLengthFromArm => 24f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -1f;
        public override float OffsetYDownwards => 5f;

        public int Time = 0;
        public int shotCounter = 0;
        public int framesBetweenShots = 0;
        public bool swapType = false;

        public override void KillHoldoutLogic()
        {
            //If the player is dead, kill the holdout.
            if (Owner.CantUseHoldout() || HeldItem.type != Owner.ActiveItem().type && shotCounter <= 19)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (Time >= 10)
            {
                if (framesBetweenShots == 0 && shotCounter <= 31)
                {
                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 15;
                    #region Debug Text Display
                    //Main.NewText(shotCounter);
                    //Main.NewText(Owner.Calamity().sharkGunDamageScaling);
                    #endregion
                    #region Visuals and Sounds
                    SoundEngine.PlaySound(SoundID.Item85, Projectile.Center);
                    Particle sparker = new CritSpark(GunTipPosition, Vector2.Zero, Color.Gold, Color.LightGoldenrodYellow, 1.7f, 3, 0.5f, 3f);
                    GeneralParticleHandler.SpawnParticle(sparker);
                    for (int i = 0; i <= 4; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(3) ? 303 : 244, (shootVelocity * Main.rand.NextFloat(0.2f, 1.1f)).RotatedByRandom(0.4f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    }
                    if (shotCounter > 14)
                    {
                        for (int i = 0; i < 3; ++i)
                        {
                            int bloodLifetime = Main.rand.Next(12, 15);
                            float bloodScale = Main.rand.NextFloat(0.4f, 0.6f);
                            Color bloodColor = Color.Lerp(Color.LightBlue * 0.7f, Color.LightBlue, Main.rand.NextFloat());
                            bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                            if (Main.rand.NextBool(20))
                                bloodScale *= 1.5f;

                            float randomSpeedMultiplier = Main.rand.NextFloat(0.8f, 1.5f);
                            Vector2 bloodVelocity = Main.rand.NextVector2Unit() * 2 * randomSpeedMultiplier;
                            bloodVelocity.Y -= 3f;
                            BloodParticle blood = new BloodParticle(Projectile.Center + (Projectile.velocity * 14f).RotatedBy(-0.8f * Projectile.direction), bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                            GeneralParticleHandler.SpawnParticle(blood);
                        }
                    }
                    #endregion

                    //How many frames between firing projectiles, and how far the gun moves backward to give the effect of recoil. Change this number to edit fire rate
                    framesBetweenShots = 6;
                    OffsetLengthFromArm -= 3f;
                    //Here we detect which ammo the bullets will use
                    Owner.PickAmmo(Owner.ActiveItem(), out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _, !Main.rand.NextBool(4));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), ModContent.ProjectileType<ArcherfishShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    shotCounter++;
                    //Allow a pause before firing the rocket. This allows the final bullet a chance to hit before the rocket is fired. Lowering this number reduces the delay, but may also cause the gun to become inconsistent
                    if (shotCounter == 20)
                        framesBetweenShots = 80;
                }
                if (framesBetweenShots > 0)
                    framesBetweenShots--;
            }
            if (shotCounter >= 19 && framesBetweenShots > 0)
            {
                Owner.channel = true;
                if (framesBetweenShots % 6 == 0)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        int bloodLifetime = Main.rand.Next(12, 15);
                        float bloodScale = Main.rand.NextFloat(0.4f, 0.6f);
                        Color bloodColor = Color.Lerp(Color.LightBlue * 0.7f, Color.LightBlue, Main.rand.NextFloat());
                        bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                        if (Main.rand.NextBool(20))
                            bloodScale *= 1.5f;

                        float randomSpeedMultiplier = Main.rand.NextFloat(0.8f, 1.5f);
                        Vector2 bloodVelocity = Main.rand.NextVector2Unit() * 2 * randomSpeedMultiplier;
                        bloodVelocity.Y -= 3f;
                        BloodParticle blood = new BloodParticle(Projectile.Center + (Projectile.velocity * 14f).RotatedBy(-0.8f * Projectile.direction), bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                        GeneralParticleHandler.SpawnParticle(blood);
                    }
                }
                if (framesBetweenShots % 25 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 4;
                        Particle smoke = new HeavySmokeParticle(GunTipPosition, shootVelocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.2f, 1.5f), Color.White, Main.rand.Next(20, 40 + 1), Main.rand.NextFloat(0.2f, 0.3f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                    SoundEngine.PlaySound(SoundID.Item111 with { Pitch = 0.05f, PitchVariance = 0.25f, MaxInstances = -1 }, Projectile.Center);
                    OffsetLengthFromArm -= 4.5f;
                }

            }
            if (shotCounter == 20 && framesBetweenShots == 0)
            {
                if (framesBetweenShots == 0)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.Center);
                    //After firing the rocket, kill the projectile to allow left click to be held down
                    Projectile.Kill();
                }
                else
                {
                    framesBetweenShots--;
                }
            }
            Time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Time < 2)
                return false;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
}
