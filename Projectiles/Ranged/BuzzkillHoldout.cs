using System;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class BuzzkillHoldout : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Buzzkill>();

        public ref float Time => ref Projectile.ai[0];
        public const float ChargeupTime = 90f;

        // These variables control the saw visually disappearing from the holdout when it fires.
        public bool NoSawOnHoldout = false;
        public int NoSawDuration = 0;

        public int Recoil = 0;

        Player player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Vector2 armPos = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 weaponTipPos = armPos + Projectile.velocity * Projectile.width * 0.5f;

            Time++;

            if (player.CantUseHoldout())
                Projectile.Kill();
            else
                Projectile.timeLeft = 2;

            if (Projectile.frame > 0)
            {
                if (NoSawOnHoldout)
                    Projectile.frame = 4;
                else
                {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter >= 3)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame++;
                        if (Projectile.frame > 3)
                            Projectile.frame = 1;
                    }
                }
            }

            if (Time < ChargeupTime)
            {
                if (Time == 1f)
                {
                    // Insert charge-up sound
                    Main.NewText("Insert charge-up sound");

                    // Reset recoil
                }
                if (Time > 30f && Projectile.frame == 0)
                    Projectile.frame = 1;
            }
            else
            {
                if (NoSawDuration > 0)
                {
                    NoSawDuration--;
                    if (NoSawDuration == 0)
                        NoSawOnHoldout = false;
                }

                if (Time % player.ActiveItem().useTime == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, weaponTipPos);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), weaponTipPos, Projectile.velocity.SafeNormalize(Vector2.UnitY) * Buzzkill.ShootSpeed, ModContent.ProjectileType<BuzzkillSaw>(), Projectile.damage, Projectile.knockBack, Main.myPlayer);
                    NoSawOnHoldout = true;
                    NoSawDuration = player.ActiveItem().useTime / 2;
                    Recoil = 10;
                }
                if (Recoil > 0)
                    Recoil--;
            }


            UpdateProjectileHeldValues(armPos);
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        private void UpdateProjectileHeldValues(Vector2 armPosition)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                float interpolant = Utils.GetLerpValue(5f, 40f, Projectile.Distance(Main.MouseWorld), true);
                Vector2 oldVel = Projectile.velocity;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(Main.MouseWorld), interpolant);
                if (Projectile.velocity != oldVel)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }
            Vector2 holdoutOffset = Projectile.velocity * MathHelper.Clamp(20f - Recoil, 0f, 20f) + new Vector2(0f, -5f);
            Projectile.Center = armPosition + holdoutOffset;
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Projectile.spriteDirection = Projectile.direction;
        }

        public override bool? CanDamage() => false;
    }
}
