using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
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
        public const float ChargeupTime = 150f;

        // Controls the saw visually disappearing from the holdout when it fires.
        public bool NoSawOnHoldout = false;
        // The current recoil of the weapon.
        public float Recoil = 0f;


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
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Vector2 armPos = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 weaponTipPos = armPos + Projectile.velocity * Projectile.width * 0.5f;

            Time++;

            if (player.CantUseHoldout())
            {
                if (Projectile.ai[1] < 1f)
                {
                    Projectile.ai[1] = 1f;
                    Projectile.timeLeft = 30;
                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, weaponTipPos);

                    float sawDamageMult = MathHelper.Clamp(MathHelper.Lerp(1f, 4f, Time / ChargeupTime), 1f, 4f) / 2f; // The damage must be divided by 2 to offset the holdout having 2x base damage.
                    int sawPierce = (int)MathHelper.Clamp(MathHelper.Lerp(2f, 6f, Time / ChargeupTime), 2f, 6f);

                    bool useSmallSlash = (Time / ChargeupTime) >= 0.25f;
                    bool useLargeSlash = (Time / ChargeupTime) >= 1f;
                    float ai0 = 0;
                    if (useSmallSlash)
                        ai0++;
                    if (useLargeSlash)
                        ai0++;

                    int buzzsaw = Projectile.NewProjectile(Projectile.GetSource_FromThis(), weaponTipPos, Projectile.velocity.SafeNormalize(Vector2.UnitY) * Buzzkill.ShootSpeed, ModContent.ProjectileType<BuzzkillSaw>(), (int)(Projectile.damage * sawDamageMult), (int)(Projectile.knockBack * (sawDamageMult / 2)), Main.myPlayer, ai0);
                    Main.projectile[buzzsaw].penetrate = sawPierce;

                    NoSawOnHoldout = true;
                    Recoil = 4f + 8f * Math.Clamp(Time / ChargeupTime, 0f, 1f);

                    for (int s = 0; s < 3; s++)
                    {
                        Vector2 sparkVelocity = new Vector2(6.5f, 0f);
                        sparkVelocity = sparkVelocity.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0));
                        
                        Particle weaponShootSparks = new AltLineParticle(weaponTipPos, sparkVelocity, false, 40, 0.7f, new Color(250, 250, 107));
                        GeneralParticleHandler.SpawnParticle(weaponShootSparks);
                    }
                    for (int s2 = 0; s2 < 3; s2++)
                    {
                        Vector2 sparkVelocity = new Vector2(6.5f, 0f);
                        sparkVelocity = sparkVelocity.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0));

                        Particle weaponShootSparks2 = new AltSparkParticle(weaponTipPos, sparkVelocity, false, 40, 0.7f, new Color(250, 250, 107));
                        GeneralParticleHandler.SpawnParticle(weaponShootSparks2);
                    }
                }
            }
            else
                Projectile.timeLeft = 2;

            if (Recoil > 0f)
                Recoil--;

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

            if (Time > 30f && !NoSawOnHoldout)
            {
                if (Time % 3 == 0)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                    sparkVel.SafeNormalize(Vector2.Zero);
                    sparkVel *= Main.rand.NextFloat(3f, 4.5f) + (MathHelper.Clamp(Time / ChargeupTime, 0f, 1f) * 4);

                    Particle buzzsawSparks = new AltLineParticle(weaponTipPos, sparkVel, false, 10, Utils.GetLerpValue(0.05f, 0.65f, Time / ChargeupTime, true), new Color(250, 250, 107));
                    GeneralParticleHandler.SpawnParticle(buzzsawSparks);
                }
            }

            if (Time < ChargeupTime)
            {
                if (Time == 1f)
                {
                    // Insert charge-up sound
                    Main.NewText("Insert charge-up sound");
                }

                if (Time > 30f && Projectile.frame == 0)
                    Projectile.frame = 1;

                if (Time == ChargeupTime - 1)
                {
                    // Insert full charge sound
                    Main.NewText("Insert full-charge sound maybe?");
                }
            }
            else
            {
                if (Time % 3 == 0)
                {
                    Vector2 smokeVelocity = new Vector2(0f, Main.rand.NextFloat(-7f, -12f));
                    smokeVelocity = smokeVelocity.RotatedByRandom(MathHelper.Pi / 8);
                    Particle fullChargeSmoke = new HeavySmokeParticle(weaponTipPos + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), smokeVelocity, Color.Gray, 30, 0.65f, 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), true);
                    GeneralParticleHandler.SpawnParticle(fullChargeSmoke);
                }
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

        // The holdout can deal damage; you're literally spinning up a buzzsaw at the end, after all.
        public override bool? CanDamage() => Time > 30f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Laceration>(), 180);

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Vector2 weaponTipPos = player.RotatedRelativePoint(player.MountedCenter, true) + Projectile.velocity * Projectile.width * 0.5f;
            hitbox = new Rectangle((int)weaponTipPos.X - 19, (int)weaponTipPos.Y - 20, 38, 40);

            if (Time / ChargeupTime >= 1f && !NoSawOnHoldout)
                hitbox.Inflate(60, 60);
            else if (Time / ChargeupTime >= 0.25f && !NoSawOnHoldout)
                hitbox.Inflate(25, 25);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 weaponTipPos = player.RotatedRelativePoint(player.MountedCenter, true) + Projectile.velocity * Projectile.width * 0.5f;
            if (Time > 30f && !NoSawOnHoldout)
            {
                if (Time / ChargeupTime >= 1f)
                {
                    Texture2D largeSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillSawLargeSlash").Value;
                    Color drawColorLarge = new Color(200, 200, 200, 100);
                    Main.EntitySpriteDraw(largeSlashTexture, weaponTipPos - Main.screenPosition + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), null, drawColorLarge, -(Time * 7f), largeSlashTexture.Size() / 2, 1f, SpriteEffects.None);
                }

                if (Time / ChargeupTime >= 0.25f)
                {
                    Texture2D smallSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillSawSmallSlash").Value;
                    Color drawColorSmall = new Color(200, 200, 200, 100);
                    Main.EntitySpriteDraw(smallSlashTexture, weaponTipPos - Main.screenPosition + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), null, drawColorSmall, Time * 7f, smallSlashTexture.Size() / 2, 1f, SpriteEffects.None);
                }
            }
            return true;
        }
    }
}
