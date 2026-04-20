using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class OmnibladeSwing : ModProjectile, ILocalizedModType
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Omniblade>();
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 308;
            Projectile.height = 184;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = TrueMeleeNoSpeedDamageClass.Instance;
            Projectile.ContinuouslyUpdateDamageStats = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 3;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 3;
            if (Projectile.frame >= Main.projFrames[Type])
                Projectile.Kill();

            Vector2 playerRotatedPoint = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            if (Main.myPlayer == Projectile.owner)
            {
                if (!Owner.CantUseHoldout())
                    HandleChannelMovement(playerRotatedPoint);
                else
                    Projectile.Kill();
            }

            // Rotation and directioning.
            Projectile.rotation = Owner.gravDir == -1f ? MathHelper.Pi : 0f;
            Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();

            // Sprite and player directioning.
            Projectile.spriteDirection = Projectile.direction * (int)(Owner.gravDir);
            if (Projectile.direction == 1)
                Projectile.Left = Owner.MountedCenter;
            else
                Projectile.Right = Owner.MountedCenter;
            Projectile.position.X += (Projectile.spriteDirection == 1 ? -92f : 96f) * Owner.gravDir;
            Projectile.position.Y -= 80f * Owner.gravDir;
            Owner.ChangeDir(Projectile.direction);

            // Prevents the projectile from dying
            Projectile.timeLeft = 2;

            // Player item-based field manipulation.
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
        }

        public void HandleChannelMovement(Vector2 playerRotatedPoint)
        {
            Vector2 newVelocity = Vector2.UnitX * (Main.MouseWorld.X > playerRotatedPoint.X).ToDirectionInt();

            // Sync if a velocity component changes.
            if (Projectile.velocity.X != newVelocity.X || Projectile.velocity.Y != newVelocity.Y)
                Projectile.netUpdate = true;

            Projectile.velocity = newVelocity;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 170);

        // Don't suffer from the same issues Murasama did in the past; encouraging people to kill their wrists for some extra DPS is bad lmao
        public override bool? CanDamage() => Projectile.frameCounter > 6;
    }
}
