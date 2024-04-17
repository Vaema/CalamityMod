using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class BuzzkillHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Buzzkill>();
        public override float RecoilResolveSpeed => 0.05f;
        public override float MaxOffsetLengthFromArm => 30f;
        public override float OffsetXUpwards => -10f;
        public override float OffsetXDownwards => 5f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 10f;
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.28f;

        public ref float Time => ref Projectile.ai[0];
        public const float ChargeupTime = 150f;

        // Controls the saw visually disappearing from the holdout when it fires.
        public bool NoSawOnHoldout = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void KillHoldoutLogic()
        {
            if (HeldItem.type != Owner.ActiveItem().type)
            {
                Projectile.Kill();
                Projectile.netUpdate = true;
            }
        }

        public override void HoldoutAI()
        {
            Time++;

            if (Owner.CantUseHoldout())
            {
                if (Projectile.ai[1] < 1f)
                {
                    KeepRefreshingLifetime = false;

                    Projectile.ai[1] = 1f;
                    Projectile.timeLeft = 30;
                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, GunTipPosition);

                    float sawDamageMult = MathHelper.Clamp(MathHelper.Lerp(1f, 4f, Time / ChargeupTime), 1f, 4f) / 2f; // The damage must be divided by 2 to offset the holdout having 2x base damage.
                    int sawPierce = (int)MathHelper.Clamp(MathHelper.Lerp(2f, 6f, Time / ChargeupTime), 2f, 6f);

                    bool useSmallSlash = (Time / ChargeupTime) >= 0.25f;
                    bool useLargeSlash = (Time / ChargeupTime) >= 1f;
                    float ai0 = 0;
                    if (useSmallSlash)
                        ai0++;
                    if (useLargeSlash)
                        ai0++;

                    int buzzsaw = Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitY) * Buzzkill.ShootSpeed, ModContent.ProjectileType<BuzzkillSaw>(), (int)(Projectile.damage * sawDamageMult), (int)(Projectile.knockBack * (sawDamageMult / 2)), Main.myPlayer, ai0);
                    Main.projectile[buzzsaw].penetrate = sawPierce;

                    NoSawOnHoldout = true;
                    OffsetLengthFromArm -= 4f + 12f * Math.Clamp(Time / ChargeupTime, 0f, 1f);

                    for (int s = 0; s < 3; s++)
                    {
                        Vector2 sparkVelocity = new Vector2(6.5f, 0f);
                        sparkVelocity = sparkVelocity.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0));

                        Particle weaponShootSparks = new AltLineParticle(GunTipPosition, sparkVelocity, false, 40, 0.7f, new Color(250, 250, 107));
                        GeneralParticleHandler.SpawnParticle(weaponShootSparks);
                    }
                    for (int s2 = 0; s2 < 3; s2++)
                    {
                        Vector2 sparkVelocity = new Vector2(6.5f, 0f);
                        sparkVelocity = sparkVelocity.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0));

                        Particle weaponShootSparks2 = new AltSparkParticle(GunTipPosition, sparkVelocity, false, 40, 0.7f, new Color(250, 250, 107));
                        GeneralParticleHandler.SpawnParticle(weaponShootSparks2);
                    }
                }
            }

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

            if (Time > 30f && !NoSawOnHoldout)
            {
                if (Time % 3 == 0)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                    sparkVel.SafeNormalize(Vector2.Zero);
                    sparkVel *= Main.rand.NextFloat(3f, 4.5f) + (MathHelper.Clamp(Time / ChargeupTime, 0f, 1f) * 4);

                    Particle buzzsawSparks = new AltLineParticle(GunTipPosition, sparkVel, false, 10, Utils.GetLerpValue(0.05f, 0.65f, Time / ChargeupTime, true), new Color(250, 250, 107));
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
                    Particle fullChargeSmoke = new HeavySmokeParticle(GunTipPosition + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), smokeVelocity, Color.Gray, 30, 0.65f, 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), true);
                    GeneralParticleHandler.SpawnParticle(fullChargeSmoke);
                }
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            ExtraBackArmRotation = MathHelper.ToRadians(15f);
        }

        // The holdout can deal damage; you're literally spinning up a buzzsaw at the end, after all.
        public override bool? CanDamage() => !NoSawOnHoldout;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Laceration>(), 180);

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = new Rectangle((int)GunTipPosition.X - 19, (int)GunTipPosition.Y - 20, 38, 40);

            if (Time / ChargeupTime >= 1f)
                hitbox.Inflate(60, 60);
            else if (Time / ChargeupTime >= 0.25f)
                hitbox.Inflate(25, 25);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillHoldout").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = frame.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (!NoSawOnHoldout)
            {
                float shake = Utils.Remap(Time, 0f, ChargeupTime, 0f, 3f);
                drawPosition += Main.rand.NextVector2Circular(shake, shake);
            }

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale, flipSprite);

            if (Time > 30f && !NoSawOnHoldout)
            {
                if (Time / ChargeupTime >= 1f)
                {
                    Texture2D largeSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillSawLargeSlash").Value;
                    Color drawColorLarge = new Color(200, 200, 200, 100);
                    Main.EntitySpriteDraw(largeSlashTexture, GunTipPosition - Main.screenPosition + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), null, drawColorLarge, -(Time * 7f), largeSlashTexture.Size() / 2, 1f, SpriteEffects.None);
                }

                if (Time / ChargeupTime >= 0.25f)
                {
                    Texture2D smallSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillSawSmallSlash").Value;
                    Color drawColorSmall = new Color(200, 200, 200, 100);
                    Main.EntitySpriteDraw(smallSlashTexture, GunTipPosition - Main.screenPosition + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), null, drawColorSmall, Time * 7f, smallSlashTexture.Size() / 2, 1f, SpriteEffects.None);
                }
            }

            return false;
        }
    }
}
