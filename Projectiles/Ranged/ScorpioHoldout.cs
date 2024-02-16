using System;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.Items.Weapons.Ranged.Scorpio;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class ScorpioHoldout : ModProjectile, ILocalizedModType
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Scorpio>();
        public override string Texture => "CalamityMod/Items/Weapons/Ranged/Scorpio";

        public Player Owner { get; set; }
        public ref float ShootingTimer => ref Projectile.ai[0];
        public ref float TimerBetweenBursts => ref Projectile.ai[1];
        public ref float ChargeLV => ref Projectile.ai[2];
        public ref float OffsetLengthScalar => ref Projectile.localAI[0];

        // The maximum amount that the gun is away from the player's hands.
        public const float MaxOffsetScalarLength = 15f;

        // The type of dust used in the effects and the color of the particles that it'll use depending on the type of rocket used.
        // It'll carry over to the projectiles that it shoots.
        public static int DustEffectsID { get; set; }
        public static Color EffectsColor { get; set; }
        public static Color StaticEffectsColor = Color.Turquoise;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 96;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            // If there's no player, or the player is the server, or the owner's stunned, there'll be no holdout.
            bool cantUse = Owner == null || !Owner.active || Owner.dead || !Owner.channel || Main.myPlayer != Projectile.owner || Owner.CCed || Owner.noItems;
            if (cantUse)
            {
                Projectile.Kill();
                NetUpdate();
                return;
            }

            // The center of the player, taking into account if they have a mount or not.
            Vector2 ownerPosition = Owner.MountedCenter;

            // The vector between the player and the mouse.
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - ownerPosition;

            // Deals with the holdout's rotation and direction, the owner's arms, etc.
            ManageHoldout(ownerPosition, ownerToMouse);

            // If the timer reaches Item.useTime, it'll shoot.
            // It'll shoot once immediately as the timer reaches.
            Item heldItem = Owner.ActiveItem();

            // If the player's pressing RMB, it'll shoot the big rocket.
            if (Owner.Calamity().mouseRight && ChargeLV >= 3)
            {
                ShootRocket(heldItem, true);
                ShootingTimer = heldItem.useAnimation; // If you time your nukes well, you can reset the burst rocket cooldown
                ChargeLV = -1;
            }

            if (ShootingTimer >= heldItem.useAnimation)
            {
                if (ShootingTimer == heldItem.useAnimation && ChargeLV < 3)
                    ChargeLV++;

                // Shooting the burst of small rockets.
                // The time is adapted to the speed modifier from the reforge.
                int adaptedTimeBetweenBursts = TimeBetweenBursts * heldItem.useAnimation / OriginalUseTime;

                if (ShootingTimer % adaptedTimeBetweenBursts == 0f)
                    ShootRocket(heldItem, false);

                // Resets the timers when everything has been shot.
                if (ShootingTimer >= heldItem.useAnimation + adaptedTimeBetweenBursts * (ProjectilesPerBurst - 1))
                {
                    ShootingTimer = 0f;
                    TimerBetweenBursts = 0f;
                    NetUpdate();
                }
            }

            // When we change the distance of the gun from the arms for the recoil,
            // recover to the original position smoothly.
            if (OffsetLengthScalar != MaxOffsetScalarLength)
                OffsetLengthScalar = MathHelper.Lerp(OffsetLengthScalar, MaxOffsetScalarLength, 0.1f);

            ShootingTimer++;
        }

        #region AI Methods

        public void ManageHoldout(Vector2 mountedCenter, Vector2 ownerToMouse)
        {
            Vector2 rotationVector = Projectile.rotation.ToRotationVector2();
            float velocityRotation = Projectile.velocity.ToRotation();
            float proximityLookingUpwards = Vector2.Dot(ownerToMouse.SafeNormalize(Vector2.Zero), -Vector2.UnitY);
            int direction = MathF.Sign(ownerToMouse.X);

            Vector2 armPosition = Owner.RotatedRelativePoint(mountedCenter, true);
            Vector2 lengthOffset = rotationVector * OffsetLengthScalar;
            Vector2 armOffset = new Vector2(Utils.Remap(proximityLookingUpwards, -1f, 1f, 0f, -12f) * direction, -10f + Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? 0f : 10f));
            Projectile.Center = armPosition + lengthOffset + armOffset;
            Projectile.velocity = velocityRotation.AngleTowards(ownerToMouse.ToRotation(), 0.2f).ToRotationVector2();
            Projectile.rotation = velocityRotation;
            Projectile.timeLeft = 2;

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            Projectile.spriteDirection = Projectile.direction = direction;
            Owner.ChangeDir(direction);

            float armRotation = Projectile.rotation - MathHelper.PiOver2; // -Pi/2 because the arms rotation starts with arms pointing down.
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation + MathHelper.ToRadians(15f) * direction);

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            // Charge level visuals here
            Vector2 nuzzlePosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 48f + (-Vector2.UnitY.RotatedBy(Projectile.rotation) * 10f) * Projectile.direction;
            if (ChargeLV == 1 && ShootingTimer % 3 == 0)
            {
                Vector2 position = nuzzlePosition - Projectile.velocity * 95;
                Vector2 velocity = (-Projectile.velocity * 5).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.6f, 1.4f);
                SquishyLightParticle energy = new(position, velocity, Main.rand.NextFloat(0.12f, 0.14f), StaticEffectsColor, Main.rand.Next(3, 5 + 1), 1, 1.5f);
                GeneralParticleHandler.SpawnParticle(energy);
                Dust dust = Dust.NewDustPerfect(position, DustEffectsID, velocity, 0, default, Main.rand.NextFloat(1.2f, 1.7f));
                dust.noGravity = true;
            }
            if (ChargeLV == 2 & ShootingTimer % 2 == 0)
            {
                Vector2 position = nuzzlePosition - Projectile.velocity * 95;
                Vector2 velocity = (-Projectile.velocity * 5).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.9f, 1.9f);
                SquishyLightParticle energy = new(position, velocity, Main.rand.NextFloat(0.18f, 0.22f), StaticEffectsColor, Main.rand.Next(3, 5 + 1), 1, 1.5f);
                GeneralParticleHandler.SpawnParticle(energy);
                Dust dust = Dust.NewDustPerfect(position, DustEffectsID, velocity, 0, default, Main.rand.NextFloat(1.2f, 1.7f));
                dust.noGravity = true;
            }
            if (ChargeLV >= 3)
            {
                Vector2 position = nuzzlePosition - Projectile.velocity * 95;
                Vector2 velocity = (-Projectile.velocity * 5).RotatedByRandom(0.75f) * Main.rand.NextFloat(1.2f, 2.3f);
                SquishyLightParticle energy = new(position, velocity, Main.rand.NextFloat(0.24f, 0.34f), StaticEffectsColor, Main.rand.Next(3, 5 + 1), 1, 1.5f);
                GeneralParticleHandler.SpawnParticle(energy);
                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustPerfect(position, DustEffectsID, velocity, 0, default, Main.rand.NextFloat(1.6f, 2.1f));
                    dust.noGravity = true;
                }
            }
            if (ChargeLV == 0 && ShootingTimer % 3 == 0)
            {
                Vector2 position = nuzzlePosition - Projectile.velocity * 95;
                Vector2 velocity = (-Projectile.velocity * 4).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.9f, 2f);
                Particle smoke = new HeavySmokeParticle(position, velocity, Color.SlateGray, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.3f, 0.6f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), true, required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustPerfect(position, 303, velocity * 0.5f, 200, default, Main.rand.NextFloat(0.9f, 1.3f));
                    dust.noGravity = false;
                }
            }
        }

        public void ShootRocket(Item item, bool isRMB)
        {
            // We use the velocity of this projectile as its direction vector.
            Vector2 projectileVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero);

            // The position of the tip of the gun.
            Vector2 nuzzlePosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 48f + (-Vector2.UnitY.RotatedBy(Projectile.rotation) * 10f) * Projectile.direction;

            // Every time we shoot we use ammo.
            // With this method we also use the item's stats, like the shoot speed, or the type of ammo it was used.
            Owner.PickAmmo(item, out _, out float projSpeed, out int damage, out float knockback, out int rocketType, Main.rand.Next(100) > 70); // 70% ammo conservation

            // Decides the color of the effects depending on the type of rocket used.
            switch (rocketType)
            {
                case ItemID.WetRocket:
                    DustEffectsID = 45;
                    EffectsColor = Color.RoyalBlue;
                    break;
                case ItemID.LavaRocket:
                    DustEffectsID = DustID.Torch;
                    EffectsColor = Color.Red;
                    break;
                case ItemID.HoneyRocket:
                    DustEffectsID = DustID.Honey;
                    EffectsColor = Color.Yellow;
                    break;
                default:
                    DustEffectsID = 302;
                    EffectsColor = Color.Aquamarine;
                    break;
            }

            // Spawns the projectile.
            Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(),
                nuzzlePosition,
                projectileVelocity.RotatedByRandom(isRMB ? 0f : MathHelper.PiOver4) * projSpeed * (isRMB ? 1f : Main.rand.NextFloat(0.8f, 1f)),
                isRMB ? ProjectileType<ScorpioLargeRocket>() : ProjectileType<ScorpioRocket>(),
                damage,
                knockback,
                Projectile.owner,
                rocketType,
                projSpeed);

            NetUpdate();

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            SoundStyle RightClickSound = new("CalamityMod/Sounds/Item/RealityRuptureStealth") { Volume = 0.45f };
            if (isRMB)
                SoundEngine.PlaySound(RightClickSound with { Pitch = -0.1f }, Projectile.Center);
            else
                SoundEngine.PlaySound(RocketShoot with { Pitch = ChargeLV * 0.055f }, Projectile.Center);

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            OffsetLengthScalar -= isRMB ? 30f : 5f;

            int dustAmount = Main.rand.Next(10, 15 + 1);
            for (int i = 0; i < dustAmount; i++)
            {
                Dust shootDust = Dust.NewDustPerfect(
                    nuzzlePosition,
                    DustEffectsID,
                    projectileVelocity.RotatedByRandom(MathHelper.PiOver2 - MathHelper.ToRadians(15f)) * Main.rand.NextFloat(6f, 10f));
                shootDust.noGravity = true;
                shootDust.noLight = true;
                shootDust.noLightEmittence = true;
            }

            Particle shootPulse = new DirectionalPulseRing(
                nuzzlePosition,
                Vector2.Zero,
                Color.Gray * 0.7f,
                new Vector2(0.5f, 1f),
                Projectile.rotation,
                0.1f,
                0.4f,
                20);
            GeneralParticleHandler.SpawnParticle(shootPulse);
        }

        public void NetUpdate()
        {
            Projectile.netUpdate = true;
            if (Projectile.netSpam >= 10)
                Projectile.netSpam = 9;
        }

        #endregion

        public override void OnSpawn(IEntitySource source)
        {
            Owner = Main.player[Projectile.owner];
            OffsetLengthScalar = MaxOffsetScalarLength;
        }

        // Because we use the velocity as a direction, we don't need it to change its position.
        public override bool ShouldUpdatePosition() => false;

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glowTexture = Request<Texture2D>("CalamityMod/Projectiles/Ranged/ScorpioHoldout_Glow").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);
            Main.EntitySpriteDraw(glowTexture, drawPosition, null, Color.White, drawRotation, rotationPoint, Projectile.scale, flipSprite);

            return false;
        }
    }
}
