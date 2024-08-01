using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class ThunderstormHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Thunderstorm>();
        public override float MaxOffsetLengthFromArm => 64f;
        public override float BaseOffsetY => -16f;
        public override float OffsetXUpwards => -16f;
        public override float OffsetXDownwards => 12f;
        public override float OffsetYUpwards => 6f;
        public override float OffsetYDownwards => 20f;
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.3f;
        public override string Texture => "CalamityMod/Projectiles/Magic/ThunderstormHoldout";

        public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 15;

        public override void KillHoldoutLogic()
        {
            // Only times out if at the final frame (post-firing resting time)
            bool noMana = !Owner.CheckMana(Owner.ActiveItem()) && Projectile.frame == 14;
            if (HeldItem.type != Owner.ActiveItem().type || noMana)
            {
                Projectile.Kill();
                Projectile.netUpdate = true;
            }
        }

        public override void HoldoutAI()
        {
            // Update damage based on curent magic damage stat (so Mana Sickness affects it)
            Projectile.damage = HeldItem is null ? 0 : Owner.GetWeaponDamage(HeldItem);

            // Time between firing (as the player continues to hold)
            if (Projectile.frame == 14)
            {
                Projectile.frameCounter++;

                // Subtract a flat 42 from the use time because the firing animation consists of 14 frames at 20 FPS
                if (Projectile.frameCounter >= MathHelper.Clamp(Owner.ActiveItem().useAnimation - 42, 0f, Owner.ActiveItem().useAnimation))
                {
                    if (Owner.CantUseHoldout())
                        Projectile.Kill();

                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
            }
            // Firing -- this is the initial state the spawned projectile starts in
            else if (Projectile.frame == 0)
            {
                // Does not fire on frame one due to position updating fuckery
                if (Projectile.frameCounter != 1)
                {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter >= 3)
                    {
                        Projectile.frame++;
                        Projectile.frameCounter = 0;
                    }
                    return;
                }

                Projectile.frameCounter++;
                if (Owner.CheckMana(Owner.ActiveItem(), -1, true))
                {
                    SoundEngine.PlaySound(CommonCalamitySounds.PlasmaBlastSound, GunTipPosition);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitY) * HeldItem.shootSpeed, ModContent.ProjectileType<ThunderstormShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }
            // The firing animation
            else if (Projectile.frame > 0)
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= 3)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 0;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = frame.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
}
