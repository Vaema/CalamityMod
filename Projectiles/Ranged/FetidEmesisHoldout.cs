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
using System;
using CalamityMod.Dusts;

namespace CalamityMod.Projectiles.Ranged
{
    public class FetidEmesisHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<FetidEmesis>();
        public override float MaxOffsetLengthFromArm => 24f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYDownwards => 5f;

        public ref float revFrames => ref Projectile.ai[0];
        public ref float cooldownTimer => ref Projectile.ai[1];
        public ref float shootingTimer => ref Projectile.ai[2]; // Dual functions for rapid fire shooting cooldown and recoil
        public bool isTired => cooldownTimer > 0;
        public float revSpeed = 1;
        public bool isHoldingIn = false;
        public bool holdingInBonus = false;
        public bool secondShot = true;
        public int maxFrames = 420;
        public int initialFireTime = 45;
        public float shineScale = 0;

        public override void KillHoldoutLogic()
        {
            if ( !isTired && (Owner.CantUseHoldout(false) || HeldItem.type != Owner.ActiveItem().type))
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            isHoldingIn = Owner.Calamity().mouseRight;

            if (!isTired && (Owner.CantUseHoldout() || HeldItem.type != Owner.ActiveItem().type))
                cooldownTimer = (int)(Utils.Remap(revFrames, 0, 350, 40, 180, true));
            if (isTired)
            {
                PostFiringCooldown();
                return;
            }

            revSpeed = Utils.Remap(revFrames, 0, maxFrames - 120, 1, 20, true);
            if (shootingTimer >= initialFireTime && revFrames < maxFrames && secondShot)
            {
                Owner.PickAmmo(Owner.ActiveItem(), out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _, !Main.rand.NextBool(4));
                
                SoundStyle fire = new("CalamityMod/Sounds/Item/GunShotSmallAlt");
                SoundEngine.PlaySound(fire with { Volume = 0.7f, Pitch = -0.1f + revSpeed * 0.01f }, Projectile.Center);

                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 19;

                float spread = 0.045f * Utils.GetLerpValue(0, maxFrames, revFrames, true);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, (shootVelocity).RotatedByRandom(spread), bulletAMMO, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                
                for (int i = 0; i <= 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 5, Main.rand.NextBool(5) ? 28 : 215, shootVelocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.2f, 1.5f), 0, default, Main.rand.NextFloat(0.6f, 1.4f));
                    dust.noGravity = true;
                }
                //GenericSparkle sparker = new GenericSparkle(GunTipPosition, Vector2.Zero, Color.DarkGoldenrod, Color.Gold, Main.rand.NextFloat(1.1f, 1.8f), 2, Main.rand.NextFloat(-0.01f, 0.01f), 2.68f);
                //GeneralParticleHandler.SpawnParticle(sparker);

                OffsetLengthFromArm -= 5f;
                secondShot = false;
                shineScale = 1;
            }
            if (shootingTimer >= 60 && revFrames < maxFrames)
            {
                Owner.PickAmmo(Owner.ActiveItem(), out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _, !Main.rand.NextBool(4));
                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 19;
                float spread = 0.045f * Utils.GetLerpValue(0, maxFrames, revFrames, true);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, (shootVelocity).RotatedByRandom(spread).RotatedByRandom(0.04f), bulletAMMO, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                shootingTimer = 0;
                secondShot = true;
            }

            shootingTimer += revSpeed;
            revFrames++;
            shineScale *= 0.77f;

            if (revFrames >= maxFrames && !isTired)
            {
                if (isHoldingIn)
                {
                    if (revFrames == maxFrames + 120)
                    {
                        holdingInBonus = true;
                        SoundEngine.PlaySound(SoundID.NPCHit14 with { Volume = 1f, Pitch = 0.3f }, Projectile.Center);
                        for (int i = 0; i < 25; i++)
                        {
                            Vector2 dustVel = Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(4, 8);
                            Dust dust2 = Dust.NewDustPerfect(GunTipPosition + dustVel, 278, dustVel * 0.7f);
                            dust2.scale = Main.rand.NextFloat(0.45f, 0.9f);
                            dust2.noGravity = true;
                            dust2.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Chartreuse : Color.Green, 0.7f);
                        }
                        Particle pulse = new CustomPulse(GunTipPosition, Vector2.Zero, Color.Chartreuse, "CalamityMod/Particles/HighResFoggyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.05f, 14);
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }

                    Dust dust = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(5) ? 28 : 215, (Projectile.velocity * 2).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 1.5f), 0, default, Main.rand.NextFloat(0.6f, 1.4f));
                    dust.noGravity = true;
                    if (revFrames % 20 == 0)
                    {
                        SoundStyle tired = new("CalamityMod/Sounds/Custom/OldDukeHuff");
                        SoundEngine.PlaySound(tired with { Volume = 0.3f, Pitch = 0.3f }, Projectile.Center);
                        for (int i = 0; i <= 3; i++)
                        {
                            Dust dust5 = Dust.NewDustPerfect(GunTipPosition, 267, (Projectile.velocity * 5f).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.1f, 1.5f), 0, default, Main.rand.NextFloat(0.9f, 1.2f));
                            dust5.noGravity = true;
                            dust5.color = Color.Chartreuse;
                        }
                    }
                }
                else
                {
                    Owner.Calamity().GeneralScreenShakePower = 6.5f;
                    OffsetLengthFromArm -= 35f;
                    cooldownTimer = holdingInBonus ? 230 : 180;
                    SoundStyle bigShot = new("CalamityMod/Sounds/Custom/Perforator/PerfHiveIchorShoot");
                    SoundStyle bigShotGun = new("CalamityMod/Sounds/Item/FlakKrakenShoot");
                    SoundEngine.PlaySound(bigShot with { Pitch = -0.2f, Volume = 0.6f }, Projectile.Center);
                    SoundEngine.PlaySound(bigShotGun with { Volume = 0.6f }, Projectile.Center);

                    int chunkDamage = (int)(Projectile.damage * (2f + (holdingInBonus ? 1 : 0)));
                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 13;
                    if (holdingInBonus)
                    {
                        Particle pulse2 = new CustomPulse(GunTipPosition, Projectile.velocity * 14.5f, Color.Chartreuse * 0.7f, "CalamityMod/Particles/DustyCircleHardEdge", new Vector2(0.4f, 1f), Projectile.velocity.ToRotation(), 0.13f, 0, 34);
                        GeneralParticleHandler.SpawnParticle(pulse2);
                        Particle pulse = new CustomPulse(GunTipPosition, Projectile.velocity * 9f, Color.Chartreuse * 0.7f, "CalamityMod/Particles/FlameExplosion", new Vector2(0.4f, 1f), Projectile.velocity.ToRotation(), 0.25f, 0, 34);
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
                    for (int y = 0; y < (holdingInBonus ? 3 : 1); y++)
                    {
                        float velBonus = 1 - y * 0.2f;
                        for (int i = 0; i <= 5; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedBy(-0.03f * i * velBonus) * (1 - i * 0.08f) * velBonus, ModContent.ProjectileType<EmesisGore>(), chunkDamage, Projectile.knockBack * 3, Projectile.owner, -i, 0, velBonus);
                        }
                        for (int j = 0; j <= 5; j++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedBy(0.03f * j * velBonus) * (1 - j * 0.08f) * velBonus, ModContent.ProjectileType<EmesisGore>(), chunkDamage, Projectile.knockBack * 3, Projectile.owner, j, 0, velBonus);
                        }
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity * velBonus, ModContent.ProjectileType<EmesisGore>(), chunkDamage, Projectile.knockBack * 3, Projectile.owner, 20, 0, velBonus);

                        for (int i = 0; i <= 18; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(GunTipPosition, 66, shootVelocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 1.5f), 0, default, Main.rand.NextFloat(0.6f, 1.4f));
                            dust.noGravity = true;
                            dust.color = Color.Chartreuse;
                        }
                        Particle pulse = new GlowSparkParticle(GunTipPosition - shootVelocity, shootVelocity * velBonus, false, 12, 0.035f, Color.Chartreuse, new Vector2(2.5f, 0.9f), true);
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
                }
            }
        }
        private void PostFiringCooldown()
        {
            Owner.channel = true;
            if (revFrames > 3)
                revFrames *= 0.7f;
            if (cooldownTimer > 1)
            {
                Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;

                if (cooldownTimer % 29 == 0)
                {
                    SoundStyle tired = new("CalamityMod/Sounds/Custom/OldDukeHuff");
                    SoundEngine.PlaySound(tired with { Pitch = -0.2f }, Projectile.Center);
                    for (int i = 0; i <= 12; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, 303, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 1f), 80, default, Main.rand.NextFloat(0.4f, 1.3f));
                        dust.noGravity = false;
                        dust.color = Color.White;
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 1f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }
            }
            else
            {
                Projectile.Kill();
            }
            
            cooldownTimer--;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (revFrames < 2)
                return false;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 1; i <= 24; i++)
            {
                float attackLerp = (float)Math.Pow((double)(Utils.GetLerpValue(60, 200, revFrames, true)), (double)(8));
                float mult = MathHelper.Max(Utils.GetLerpValue(7, 0, i), Utils.GetLerpValue(17, 24, i));
                float outspace = 6 * attackLerp;
                Vector2 drawOffset = (((MathHelper.TwoPi * i / 24f).ToRotationVector2().RotatedBy(Projectile.rotation) * outspace) + Main.rand.NextVector2Circular(2, 2));
                Color auraColor = Color.Chartreuse with { A = 0 } * mult * (0.4f) * Utils.GetLerpValue(90, 135, revFrames, true);
                float aimAngle = drawRotation;
                Main.EntitySpriteDraw(texture, drawPosition + drawOffset, null, auraColor, aimAngle, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/FullStar").Value;
            float rot = Main.GlobalTimeWrappedHourly * 4;

            if (!isTired && (revFrames < maxFrames) && revFrames > 20)
            {
                for (int i = -2; i <= 2; i++) // 5 times
                    Main.EntitySpriteDraw(rechargeTexture, GunTipPosition - Main.screenPosition, null, Color.Chartreuse with { A = 0 } * 0.35f, Projectile.rotation + (shineScale + rot), rechargeTexture.Size() * 0.5f, new Vector2(1 - i * 0.2f, 1 + i * 0.2f) * shineScale * 3, SpriteEffects.None, 0);
                for (int i = -2; i <= 2; i++) // 5 times
                    Main.EntitySpriteDraw(rechargeTexture, GunTipPosition - Main.screenPosition, null, Color.Chartreuse with { A = 0 } * 0.35f, Projectile.rotation - (shineScale + rot), rechargeTexture.Size() * 0.5f, new Vector2(1 - i * 0.2f, 1 + i * 0.2f) * shineScale * 3, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
