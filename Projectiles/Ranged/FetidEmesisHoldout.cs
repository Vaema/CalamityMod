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

        public override void KillHoldoutLogic()
        {
            if ( !isTired && (Owner.CantUseHoldout(false) || HeldItem.type != Owner.ActiveItem().type))
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (!isTired && (Owner.CantUseHoldout() || HeldItem.type != Owner.ActiveItem().type))
                cooldownTimer = (int)(Utils.Remap(revFrames, 0, 350, 40, 180, true));
            if (isTired)
            {
                PostFiringCooldown();
                return;
            }

            revSpeed = Utils.Remap(revFrames, 0, 200, 1, 20, true);
            if (shootingTimer >= 60)
            {
                int bulletAMMO = ProjectileID.Bullet;
                Owner.PickAmmo(Owner.ActiveItem(), out bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _, Main.rand.NextBool(4));

                SoundStyle fire = new("CalamityMod/Sounds/Item/StrongGunShot");
                SoundEngine.PlaySound(fire with { Volume = 0.7f }, Projectile.Center);

                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 19;
                float spread = 0.045f * Utils.GetLerpValue(0, 300, revFrames, true);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(spread), bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
                
                for (int i = 0; i <= 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 5, Main.rand.NextBool(5) ? 28 : 215, shootVelocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.2f, 1.5f), 0, default, Main.rand.NextFloat(0.6f, 1.4f));
                    dust.noGravity = true;
                }
                GenericSparkle sparker = new GenericSparkle(GunTipPosition, Vector2.Zero, Color.DarkGoldenrod, Color.Gold, Main.rand.NextFloat(1.1f, 1.8f), 2, Main.rand.NextFloat(-0.01f, 0.01f), 2.68f);
                GeneralParticleHandler.SpawnParticle(sparker);

                OffsetLengthFromArm -= 5f;
                shootingTimer = 0;
            }
            
            shootingTimer += revSpeed;
            revFrames++;
            if (revFrames >= 300)
            {
                Owner.Calamity().GeneralScreenShakePower = 6.5f;
                OffsetLengthFromArm -= 35f;
                cooldownTimer = 180;
                SoundStyle bigShot = new("CalamityMod/Sounds/Custom/Perforator/PerfHiveIchorShoot");
                SoundStyle bigShotGun = new("CalamityMod/Sounds/Item/FlakKrakenShoot");
                SoundEngine.PlaySound(bigShot with { Pitch = -0.2f, Volume = 0.6f }, Projectile.Center);
                SoundEngine.PlaySound(bigShotGun with { Volume = 0.6f }, Projectile.Center);

                int chunkDamage = (int)(Projectile.damage * 1.3f);
                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 13;
                for (int i = 0; i <= 4; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedBy(-0.025f * i) * (1 - i * 0.05f), ModContent.ProjectileType<EmesisGore>(), chunkDamage, Projectile.knockBack * 3, Projectile.owner);
                }
                for (int i = 0; i <= 4; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedBy(0.025f * i) * (1 - i * 0.05f), ModContent.ProjectileType<EmesisGore>(), chunkDamage, Projectile.knockBack * 3, Projectile.owner);
                }
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<EmesisGore>(), chunkDamage, Projectile.knockBack * 3, Projectile.owner);
                for (int i = 0; i <= 18; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition, 66, shootVelocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 1.5f), 0, default, Main.rand.NextFloat(0.6f, 1.4f));
                    dust.noGravity = true;
                    dust.color = Color.Chartreuse;
                }
                Particle pulse = new GlowSparkParticle(GunTipPosition - shootVelocity, shootVelocity, false, 12, 0.035f, Color.Chartreuse, new Vector2(2.5f, 0.9f), true);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }
        private void PostFiringCooldown()
        {
            Owner.channel = true;
            
            if (cooldownTimer > 1)
            {
                Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;

                if (cooldownTimer % 40 == 0)
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

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
}
