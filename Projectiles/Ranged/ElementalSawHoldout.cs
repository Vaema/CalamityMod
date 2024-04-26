using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ElementalSawHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<ElementalSaw>();
        public override float RecoilResolveSpeed => 0.05f;
        public override float MaxOffsetLengthFromArm => 36f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYUpwards => -5f;
        public override float OffsetYDownwards => 5f;
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.25f;

        public ref float Time => ref Projectile.ai[0];
        public const float ChargeupTime = 120f;
        public SlotId ChargeIdle;

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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
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
            if (Main.myPlayer == Projectile.owner)
                Owner.Calamity().rightClickListener = true;

            if (Owner.CantUseHoldout())
            {
                if (Projectile.ai[1] < 1f)
                {
                    Projectile.ai[1] = 1f;
                    KeepRefreshingLifetime = false;
                    Projectile.timeLeft = 30;

                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, GunTipPosition); // Placeholder?
                    float sawDamageMult = MathHelper.Clamp(MathHelper.Lerp(1f, 5f, Time / ChargeupTime), 1f, 5f) / 2f; // The damage must be divided by 2 to offset the holdout having 2x base damage.
                    int sawPierce = (int)MathHelper.Clamp(MathHelper.Lerp(2f, 7f, Time / ChargeupTime), 2f, 7f);

                    bool useSmallSlash = (Time / ChargeupTime) >= 0.25f;
                    bool useLargeSlash = (Time / ChargeupTime) >= 1f;
                    float ai0 = 0;
                    if (useSmallSlash)
                        ai0++;
                    if (useLargeSlash)
                        ai0++;

                    // ai[0] determines which slashes are drawn. ai[1] is the saw's timer variable. ai[2] stores the saw's pierce.
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitY) * ElementalSaw.ShootSpeed, ModContent.ProjectileType<ElementalSawProj>(), (int)(Projectile.damage * sawDamageMult), Projectile.knockBack, Main.myPlayer, ai0, 0f, sawPierce);

                    NoSawOnHoldout = true;
                    OffsetLengthFromArm -= 4f + 12f * Math.Clamp(Time / ChargeupTime, 0f, 1f);
                }
            }

            // Handle the right-click dash
            if (Owner.Calamity().mouseRight && !Owner.HasCooldown(ElementalSawBoost.ID))
            {
                Owner.AddCooldown(ElementalSawBoost.ID, ElementalSaw.DashCooldown);
                Owner.Calamity().sBlasterDashActivated = true;

                // TODO - Lingering saw on right click

                // If moving, make particle effects when the dash activates
                if (Owner.velocity != Vector2.Zero)
                {
                    int particleAmt = 7;
                    for (int c = 0; c < particleAmt; c++)
                    {
                        Color sparkColor = Color.Lerp(new Color(122, 240, 58), new Color(32, 186, 171), c / (particleAmt - 1));
                        Particle spark = new CritSpark(Owner.Center, Owner.velocity.RotatedByRandom(MathHelper.ToRadians(13f)) * Main.rand.NextFloat(-2.1f, -4.5f), Color.White, sparkColor, 2f, 45, 2.25f, 2f);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    for (int e = 0; e < particleAmt * 2; e++)
                    {
                        Color sparkColor2 = Color.Lerp(new Color(122, 240, 58), new Color(32, 186, 171), e / (particleAmt - 1));
                        Particle spark2 = new NanoParticle(Owner.Center, Owner.velocity.RotatedByRandom(MathHelper.ToRadians(-MathHelper.PiOver4)) * Main.rand.NextFloat(2.5f, 4.5f), sparkColor2, 1f, 45, Main.rand.NextBool(3));
                        GeneralParticleHandler.SpawnParticle(spark2);
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
                        Projectile.frame = 0;
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

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = new Rectangle((int)GunTipPosition.X - 23, (int)GunTipPosition.Y - 23, 46, 46);

            if (Time / ChargeupTime >= 1f)
                hitbox.Inflate(70, 70);
            else if (Time / ChargeupTime >= 0.25f)
                hitbox.Inflate(32, 32);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), 300);
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 150);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SwiftSlice") { Volume = 0.7f }, GunTipPosition);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawHoldout").Value;
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
                    Texture2D largeSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawLargeSlash").Value;
                    Color drawColorLarge = new Color(200, 200, 200, 100);
                    Main.EntitySpriteDraw(largeSlashTexture, GunTipPosition - Main.screenPosition + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), null, drawColorLarge, -(Time * 7f), largeSlashTexture.Size() / 2, 1f, SpriteEffects.None);
                }

                if (Time / ChargeupTime >= 0.25f)
                {
                    Texture2D smallSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawSmallSlash").Value;
                    Color drawColorSmall = new Color(200, 200, 200, 100);
                    Main.EntitySpriteDraw(smallSlashTexture, GunTipPosition - Main.screenPosition + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), null, drawColorSmall, Time * 7f, smallSlashTexture.Size() / 2, 1f, SpriteEffects.None);
                }
            }
            return false;
        }
    }
}
