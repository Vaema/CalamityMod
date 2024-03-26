using System;
using System.Runtime.InteropServices;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class TheHiveHoldout : ModProjectile, ILocalizedModType
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<TheHive>();

        public override string Texture => "CalamityMod/Items/Weapons/Ranged/TheHive";

        // The type of dust used in the effects and the color of the particles that it'll use depending on the type of rocket used.
        // It'll carry over to the projectiles that it shoots.
        public static int DustEffectsID { get; set; }
        public static Color EffectsColor { get; set; }
        public static Color StaticEffectsColor = Color.Lime;
        private ref float ShootingTimer => ref Projectile.ai[0];
        private bool FireNuke;
        private float PostFireCooldown = 0;
        private bool HasLetGo = false;
        private SlotId HiveHum;

        private ref float OffsetLength => ref Projectile.localAI[0];

        private Player Owner;

        private const float MaxOffsetLength = 15f;
        private const float MaxCharge = 90f;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 66;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            FireNuke = ShootingTimer > MaxCharge;
            Item heldItem = Owner.ActiveItem();

            if (ShootingTimer == 0)
            {
                SoundStyle charge = new("CalamityMod/Sounds/Item/LowHum");
                HiveHum = SoundEngine.PlaySound(charge with { Volume = 1.6f, IsLooped = true }, Projectile.Center);
            }
            // If there's no player, or the player is the server, or the owner's stunned, there'll be no holdout.
            if (Owner.CantUseHoldout() && !HasLetGo || heldItem.type != ItemType<TheHive>())
            {
                if (SoundEngine.TryGetActiveSound(HiveHum, out var hum) && hum.IsPlaying)
                {
                    hum?.Stop();
                }
                ShootRocket(heldItem);
                NetUpdate();
                HasLetGo = true;
            }
            if (HasLetGo)
            {
                PostFiringCooldown();
            }

            // The center of the player, taking into account if they have a mount or not.
            Vector2 ownerPosition = Owner.MountedCenter;

            // The vector between the player and the mouse.
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - ownerPosition;

            // Deals with the holdout's rotation and direction, the owner's arms, etc.
            ManageHoldout(ownerPosition, ownerToMouse);

            // When we change the distance of the gun from the arms for the recoil,
            // recover to the original position smoothly.
            if (OffsetLength != MaxOffsetLength)
                OffsetLength = MathHelper.Lerp(OffsetLength, MaxOffsetLength, 0.1f);

            if (!HasLetGo)
            {
                ShootingTimer++;
                if (SoundEngine.TryGetActiveSound(HiveHum, out var hum) && hum.IsPlaying)
                {
                    hum.Position = Projectile.Center;
                    hum.Pitch = MathHelper.Lerp(0f, 0.8f, Utils.GetLerpValue(0f, MaxCharge, ShootingTimer, true));
                }
            }

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 33f;
            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero) * 20;
            if (ShootingTimer >= MaxCharge && !HasLetGo)
            {
                for (int k = 0; k < 2; k++)
                {
                    GlowOrbParticle spark = new GlowOrbParticle(tipPosition + Projectile.velocity * 1.5f, Vector2.Zero, false, 2, Main.rand.NextFloat(0.5f, 1.1f), Color.Red);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                for (int e = 0; e < 2; e++)
                {
                    Dust dust2 = Dust.NewDustPerfect(tipPosition + Projectile.velocity * 1.5f, 90, shootDirection * Main.rand.NextFloat(0.01f, 0.8f));
                    dust2.scale = Main.rand.NextFloat(0.45f, 0.75f);
                    dust2.noGravity = true;
                }
            }

            if (ShootingTimer == MaxCharge && !HasLetGo)
            {
                SoundStyle fullCharge = new("CalamityMod/Sounds/Custom/PlagueSounds/PBGAttackSwitchShort");
                SoundEngine.PlaySound(fullCharge with { Volume = 0.9f}, Projectile.Center);
                for (int k = 0; k < 15; k++)
                {
                    Dust dust2 = Dust.NewDustPerfect(tipPosition, 90, new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.8f));
                    dust2.scale = Main.rand.NextFloat(0.85f, 1.15f);
                    dust2.noGravity = true;
                }
            }
        }

        private void ManageHoldout(Vector2 mountedCenter, Vector2 ownerToMouse)
        {
            Vector2 rotationVector = Projectile.rotation.ToRotationVector2();
            float velocityRotation = Projectile.velocity.ToRotation();
            float proximityLookingUpwards = Vector2.Dot(ownerToMouse.SafeNormalize(Vector2.Zero), -Vector2.UnitY);
            int direction = MathF.Sign(ownerToMouse.X);

            Vector2 armPosition = Owner.RotatedRelativePoint(mountedCenter, true);
            Vector2 lengthOffset = rotationVector * OffsetLength;
            Vector2 armOffset = new Vector2(Utils.Remap(proximityLookingUpwards, -1f, 1f, 0f, -12f) * direction, -10f + Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? 15f : 0f));
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

            // Rumble (only while channeling)
            float rumble = Utils.GetLerpValue(0f, MaxCharge, ShootingTimer, true) * ShootingTimer >= MaxCharge ? 2 : 0.8f;
            if (!Owner.CantUseHoldout())
                Projectile.Center += Main.rand.NextVector2Circular(rumble, rumble);
        }

        private void ShootRocket(Item item)
        {
            // We use the velocity of this projectile as its direction vector.
            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
            float VelocityMultiplier = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0f, MaxCharge, ShootingTimer, true));

            // The position of the tip of the gun.
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 33f;

            // Every time we shoot we use ammo.
            // With this method we also use the item's stats, like the shoot speed, or the type of ammo it was used.
            Owner.PickAmmo(item, out _, out float projSpeed, out int damage, out float knockback, out int rocketType);

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
                    DustEffectsID = 131;
                    EffectsColor = Color.LawnGreen;
                    break;
            }

            // Spawns the projectile.
            if (FireNuke)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Custom/PlagueSounds/PBGBarrageLaunch");
                SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = 0.1f }, Projectile.Center);

                Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(),
                tipPosition,
                shootDirection * projSpeed * 0.3f,
                ProjectileType<HiveNuke>(),
                damage,
                knockback,
                Projectile.owner,
                rocketType);
                PostFireCooldown = 75;
            }
            else
            {
                SoundStyle fire = new("CalamityMod/Sounds/Custom/PlagueSounds/PBGBarrageLaunch");
                SoundEngine.PlaySound(fire with { Volume = 0.4f, Pitch = 0.7f }, Projectile.Center);

                int numProj = 4;
                float rotation = MathHelper.ToRadians(MathHelper.Clamp(35 - VelocityMultiplier * 26, 2, 25));
                for (int i = 0; i < numProj; i++)
                {
                    Vector2 perturbedSpeed = (shootDirection).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                    Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    tipPosition,
                    perturbedSpeed * projSpeed * VelocityMultiplier,
                    ProjectileType<HiveMissile>(),
                    damage / 4,
                    knockback,
                    Projectile.owner,
                    rocketType);
                }
                PostFireCooldown = 30;
            }

            NetUpdate();

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            if (FireNuke)
            {
                for (int k = 0; k < 10; k++)
                {
                    float pulseScale = Main.rand.NextFloat(0.35f, 0.55f);
                    DirectionalPulseRing pulse = new DirectionalPulseRing(tipPosition, (shootDirection * 25).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f, 1.2f), (Main.rand.NextBool(3) ? EffectsColor : StaticEffectsColor) * 0.8f, new Vector2(1, 1), pulseScale - 0.25f, pulseScale, 0f, 20);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
                for (int i = 0; i <= 15; i++)
                {
                    Dust dust = Dust.NewDustPerfect(tipPosition, Main.rand.NextBool(3) ? DustEffectsID : 303, (shootDirection * 20).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f, 1.2f), 0, default, Main.rand.NextFloat(0.5f, 0.9f));
                    dust.noGravity = false;
                    if (dust.type != DustEffectsID)
                        dust.color = Main.rand.NextBool(3) ? EffectsColor : StaticEffectsColor;
                }
            }
            else
            {
                for (int k = 0; k < 6; k++)
                {
                    float pulseScale = Main.rand.NextFloat(0.2f, 0.4f);
                    DirectionalPulseRing pulse = new DirectionalPulseRing(tipPosition, (shootDirection * 20).RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f, 1.2f), (Main.rand.NextBool(3) ? EffectsColor : StaticEffectsColor) * 0.8f, new Vector2(1, 1), pulseScale - 0.25f, pulseScale, 0f, 20);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            OffsetLength -= FireNuke ? 16f : 6f;
        }
        private void PostFiringCooldown()
        {
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 33f;

            if (PostFireCooldown > 0)
            {
                PostFireCooldown--;
                Vector2 smokeVel = new Vector2(0, -8) * Main.rand.NextFloat(0.1f, 1.1f);
                Particle smoke = new HeavySmokeParticle(tipPosition, smokeVel, StaticEffectsColor, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.3f, 0.6f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                Dust dust = Dust.NewDustPerfect(tipPosition, 303, smokeVel.RotatedByRandom(0.1f), 80, default, Main.rand.NextFloat(0.4f, 1.3f));
                dust.noGravity = false;
                dust.color = StaticEffectsColor;
            }
            else
            {
                if (SoundEngine.TryGetActiveSound(HiveHum, out var hum) && hum.IsPlaying)
                {
                    hum?.Stop();
                }
                Projectile.Kill();
                NetUpdate();
            }
        }

        private void NetUpdate()
        {
            Projectile.netUpdate = true;
            if (Projectile.netSpam >= 10)
                Projectile.netSpam = 9;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Owner = Main.player[Projectile.owner];
            OffsetLength = MaxOffsetLength;
        }

        // Because we use the velocity as a direction, we don't need it to change its position.
        public override bool ShouldUpdatePosition() => false;

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            if (ShootingTimer <= 0)
                return false;

            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glowTexture = Request<Texture2D>(Texture + "_Glow").Value;
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
