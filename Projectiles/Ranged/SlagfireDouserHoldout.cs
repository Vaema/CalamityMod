using System;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class SlagfireDouserHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<SlagfireDouser>();

        private static readonly Vector2 CustomHoldoutOffset = new Vector2(25f, -5f);

        public static Asset<Texture2D> pistilTexture;

        public override string Texture => "CalamityMod/Projectiles/Ranged/SlagfireDouserHoldout";
        public static string TexturePathPistil => "CalamityMod/Projectiles/Ranged/SlagfireDouserPistil";

        private float pistilJigglePhysicsTimer = 0;
        public override Vector2 GunTipPosition // There is likely a cleaner way to do this with the base gun holdout proj
        {
            get
            {
                Vector2 baseTip = base.GunTipPosition;

                baseTip.X += CustomHoldoutOffset.X * Owner.direction;
                baseTip.Y += CustomHoldoutOffset.Y;

                baseTip -= Vector2.UnitX.RotatedBy(Projectile.rotation) * 24f * Owner.gravDir;

                return baseTip;
            }
        }

        public override float RecoilResolveSpeed => 0.12f;
        public override float MaxOffsetLengthFromArm => 12f;
        public override float OffsetXUpwards => -25f;
        public override float OffsetXDownwards => -25f;
        public override float OffsetYUpwards => -24f;
        public override float OffsetYDownwards => 24f;
        public override float BaseOffsetY => -2f;
        public ref float ShootingTimer => ref Projectile.ai[0];

        private const int BurstProjectiles = 4; // 4 Slagfire shots per burst
        private const int DelayBetweenShotsInBurst = 4;

        public static int DustEffectsID { get; set; } = DustID.Ice_Red;
        public static Color EffectsColor { get; set; } = Color.MediumVioletRed;
        public static Color StaticEffectsColor { get; set; } = Color.MediumVioletRed;

        public override void HoldoutAI()
        {
            if (!Owner.channel)
            {
                Projectile.Kill();
                return;
            }
            if (ShootingTimer % (HeldItem.useAnimation + BurstProjectiles * DelayBetweenShotsInBurst) == 0 && ShootingTimer > 0)
            {
                ShootingTimer = 0;
            }

            // Calc current shot index
            int currentShotInBurst = (int)((ShootingTimer % (HeldItem.useAnimation + BurstProjectiles * DelayBetweenShotsInBurst) - HeldItem.useAnimation) / DelayBetweenShotsInBurst);

            if (ShootingTimer >= HeldItem.useAnimation && currentShotInBurst >= 0 && currentShotInBurst < BurstProjectiles)
            {
                if ((ShootingTimer - HeldItem.useAnimation) % DelayBetweenShotsInBurst == 0)
                {
                    Shoot(HeldItem);
                }

                if (Main.rand.NextBool(3))
                {
                    Vector2 projectileDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);

                    Vector2 position = GunTipPosition - Projectile.velocity * 5; // Position near the gun tip
                    Vector2 velocity = projectileDirection.RotatedByRandom(MathHelper.ToRadians(0.25f)) * Main.rand.NextFloat(0.9f, 1.9f);

                    SquishyLightParticle energy = new(position, velocity, Main.rand.NextFloat(0.18f, 0.22f), StaticEffectsColor, Main.rand.Next(3, 5 + 1), 1, 1.5f);
                    GeneralParticleHandler.SpawnParticle(energy);

                    Dust dust = Dust.NewDustPerfect(position, DustEffectsID, velocity, 0, default, Main.rand.NextFloat(1.2f, 1.7f));
                    dust.noGravity = true;
                }
            }

            ShootingTimer++; // Once per tick

            pistilJigglePhysicsTimer = MathHelper.Clamp(pistilJigglePhysicsTimer - 0.04f, 0, 1);
        }

        public void Shoot(Item item)
        {
            Vector2 projectileDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);

            int damage = Owner.GetWeaponDamage(item);
            float knockback = Owner.GetWeaponKnockback(item, item.knockBack);
            int projectileType = ProjectileType<Slagfire>();
            float projectileSpeed = item.shootSpeed;

            // Random spread 
            Vector2 finalProjectileVelocity = projectileDirection.RotatedByRandom(MathHelper.ToRadians(11f)) * projectileSpeed;

            // Slagfire
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, finalProjectileVelocity, projectileType, damage, knockback, Projectile.owner);

            // Saves resources for dedicated servers from here
            if (Main.dedServ)
                return;

            SoundEngine.PlaySound(SoundID.Item61, Projectile.Center);

            // Apply recoil by decreasing the offset length from the arm
            OffsetLengthFromArm -= 1f;
            pistilJigglePhysicsTimer += 1;


            int dustAmount = Main.rand.Next(10, 15 + 1);
            for (int i = 0; i < dustAmount; i++)
            {
                Dust shootDust = Dust.NewDustPerfect(
                GunTipPosition,
                DustEffectsID,
                projectileDirection.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(3f, 8f)); // Dust spread for visual effect
                shootDust.noGravity = true;
                shootDust.noLight = true;
                shootDust.noLightEmittence = true;
            }

            // Pulse FX
            Particle shootPulse = new DirectionalPulseRing(
            GunTipPosition,
      Vector2.Zero, // Pulse doesn't need initial velocity
            Color.Gray * 0.7f,
            new Vector2(0.5f, 1f),
            Projectile.rotation,
      0.1f,
            0.4f,
            20);
            GeneralParticleHandler.SpawnParticle(shootPulse);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            pistilTexture ??= Request<Texture2D>(TexturePathPistil);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float pistilPow = MathF.Pow(pistilJigglePhysicsTimer, 2);
            Vector2 pistilJiggleScale = new Vector2(1 - 0.25f * pistilPow, 1 + 0.5f * pistilPow);

            drawPosition.X += 25 * Owner.direction;
            drawPosition.Y += -5f;

            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            float pistilJiggleRotOffset = (MathF.Sin(MathF.Pow(pistilJigglePhysicsTimer * 3.4f, 2)) * 0.2f + MathF.Sin(Main.LocalPlayer.miscCounter * MathHelper.Pi) * 0.1f) * Projectile.spriteDirection;
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            Main.EntitySpriteDraw(pistilTexture.Value, drawPosition, null, drawColor, drawRotation + pistilJiggleRotOffset, pistilTexture.Size() * 0.5f, Projectile.scale * Owner.gravDir * pistilJiggleScale, flipSprite);

            return false;
        }
    }
}
