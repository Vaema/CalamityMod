using System;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Graphics.Renderers;

namespace CalamityMod.Projectiles.Magic
{
    public class WingmanHoldout : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Wingman>();
        public override string Texture => "CalamityMod/Items/Weapons/Magic/Wingman";

        public Color StaticEffectsColor = Color.HotPink;
        private ref float ShootingTimer => ref Projectile.ai[0];
        private float FiringTime = 40;
        private float PostFireCooldown = 0;
        private Vector2 MovementOffset;
        public bool MovingUp = true;
        public float xOffset = 1;
        public float yOffset = 0;
        public int time = 0;
        public int firingDelay = 45;

        private ref float OffsetLength => ref Projectile.localAI[0];

        private Player Owner;

        private float MaxOffsetLength = 5f;

        public bool recharging = false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 142;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, StaticEffectsColor.ToVector3() * 0.2f);
            if (time == 0)
            {
                MovingUp = Projectile.ai[2] == 1 ? true : false;
            }
            firingDelay--;
            Item heldItem = Owner.ActiveItem();

            // Update damage based on curent magic damage stat (so Mana Sickness affects it)
            Projectile.damage = heldItem is null ? 0 : Owner.GetWeaponDamage(heldItem);

            if (PostFireCooldown > 0)
            {
                PostFiringCooldown();
            }

            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-0.05f * Projectile.direction) * 12f;
            // If there's no player, or the player is the server, or the owner's stunned, there'll be no holdout.
            if (Owner.CantUseHoldout() || heldItem.type != ModContent.ItemType<Wingman>() )
            {
                NetUpdate();
                if (PostFireCooldown <= 0)
                    Projectile.Kill();
            }
            else if (PostFireCooldown <= 0 && (Owner.Calamity().mouseRight || (firingDelay <= 0 && Projectile.ai[2] == 1 || Projectile.ai[2] == -1)))
            {
                // If the player's pressing RMB, it'll shoot the grenade.
                if (Owner.Calamity().mouseRight)
                {
                    if ((Owner.CheckMana(Owner.ActiveItem(), (int)(heldItem.mana * Owner.manaCost) * 5, true, false)))
                    {
                        Shoot(heldItem, true);
                        PostFireCooldown = 35 + 55 * Utils.GetLerpValue(10, 40, FiringTime, true);
                        ShootingTimer = 0;
                        FiringTime = 40;
                    }
                    else
                    {
                        if (Projectile.soundDelay <= 0)
                        {
                            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                            Projectile.soundDelay = 50;
                        }
                        ShootingTimer = 0;
                    }
                }
                else if (ShootingTimer >= FiringTime)
                {
                    if (Owner.CheckMana(Owner.ActiveItem(), -1, true, false))
                    {
                        Shoot(heldItem, false);
                        ShootingTimer = 0;

                        if (FiringTime > 10)
                            FiringTime -= 5;
                        else
                            FiringTime = 10;
                    }
                    else
                    {
                        if (Projectile.soundDelay <= 0)
                        {
                            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                            Projectile.soundDelay = 50;
                        }
                        ShootingTimer = 0;
                    }

                }
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

            ShootingTimer++;

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero) * 20;
            time++;
            Projectile.soundDelay--;
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

            if (time % 40 == 0)
            {
                MovingUp = !MovingUp;
            }

            //xOffset = MathHelper.Lerp(xOffset, placementOffset.X, 0.01f);
            yOffset = MathHelper.Lerp(yOffset, 150 * (MovingUp ? -1 : 1), (0.085f - (FiringTime * 0.0012f)));

            Vector2 placementOffset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * (yOffset);

            Vector2 location = Owner.MountedCenter + placementOffset;
            Projectile.Center = lengthOffset + location;


            Projectile.velocity = velocityRotation.AngleTowards(ownerToMouse.ToRotation(), 0.2f).ToRotationVector2();
            Projectile.rotation = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX).ToRotation();
            Projectile.timeLeft = 2;

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            Projectile.spriteDirection = Projectile.direction = direction;
            Owner.ChangeDir(direction);

            float armRotation = (Projectile.rotation - MathHelper.PiOver2); // -Pi/2 because the arms rotation starts with arms pointing down.
            if (Projectile.ai[2] == 1)
            {
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
            }
            else
            {
                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
            }
        }

        private void Shoot(Item item, bool isGrenade)
        {
            Vector2 shootDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);

            // The position of the tip of the gun.
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-0.05f * Projectile.direction) * 12f;

            // Spawns the projectile.

            Vector2 firingVelocity = shootDirection * 10;
            if (isGrenade)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/DeadSunExplosion");
                SoundEngine.PlaySound(fire with { Volume = 0.35f, Pitch = -0.4f, PitchVariance = 0.2f }, Projectile.Center);
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, firingVelocity * Utils.GetLerpValue(60, 10, FiringTime, true), ModContent.ProjectileType<WingmanGrenade>(), (int)(Projectile.damage * 4.5), Projectile.knockBack * 5, Projectile.owner, 0);
            }
            else
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/MagnaCannonShot");
                SoundEngine.PlaySound(fire with { Volume = 0.25f, Pitch = 1f, PitchVariance = 0.35f }, Projectile.Center);
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, firingVelocity * Utils.GetLerpValue(80, 10, FiringTime, true), ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0);
            }

            NetUpdate();

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            for (int k = 0; k < 6; k++)
            {
                Vector2 shootVel = (shootDirection * 10).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 1.8f);

                Dust dust2 = Dust.NewDustPerfect(tipPosition, Main.rand.NextBool(4) ? 264 : 66, shootVel);
                dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                dust2.noGravity = true;
                dust2.color = Main.rand.NextBool() ? Color.Lerp(StaticEffectsColor, Color.White, 0.5f) : StaticEffectsColor;
            }
            Particle pulse = new GlowSparkParticle((tipPosition - shootDirection * 14), shootDirection * 20, false, Main.rand.Next(7, 11 + 1), 0.035f, StaticEffectsColor, new Vector2(1.5f, 0.9f), true);
            GeneralParticleHandler.SpawnParticle(pulse);

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            if (isGrenade)
                OffsetLength -= 27f;
            else
                OffsetLength -= 5f;
        }
        private void PostFiringCooldown()
        {
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-0.05f * Projectile.direction) * 12f;

            if (PostFireCooldown > 0 && Main.rand.NextBool())
            {
                Vector2 smokeVel = new Vector2(0, -8) * Main.rand.NextFloat(0.1f, 1.1f);
                Particle smoke = new HeavySmokeParticle(tipPosition, smokeVel, StaticEffectsColor, Main.rand.Next(30, 50 + 1), Main.rand.NextFloat(0.1f, 0.4f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                Dust dust = Dust.NewDustPerfect(tipPosition, 303, smokeVel.RotatedByRandom(0.1f), 80, default, Main.rand.NextFloat(0.2f, 0.8f));
                dust.noGravity = false;
                dust.color = StaticEffectsColor;
            }
            ShootingTimer = 0;
            firingDelay = 45;
            PostFireCooldown--;
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
            if (time <= 0)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/Wingman").Value;

            if (Projectile.ai[2] == 1)
                texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/Wingman").Value;
            else
                texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/WingmanAlt").Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (Projectile.spriteDirection == -1 ? MovingUp : !MovingUp)
                flipSprite |= SpriteEffects.FlipVertically;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Lerp(StaticEffectsColor, Color.White, 0.5f) * 0.2f, 1, texture);
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);

            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
    }
}
