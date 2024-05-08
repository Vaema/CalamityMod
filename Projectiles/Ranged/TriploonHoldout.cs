using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TriploonHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Triploon>();
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.38f - Vector2.UnitY.RotatedBy(Projectile.rotation) * 3f;
        public override float MaxOffsetLengthFromArm => 25f;
        public override float OffsetXUpwards => -7f;
        public override float OffsetXDownwards => 2f;
        public override float BaseOffsetY => -8f;
        public override float OffsetYUpwards => -3f;
        public override float OffsetYDownwards => 7f;

        public ref float Time => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
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
                // Set the spears to return if the weapon is not being used
                for (int proj = 0; proj < Main.maxProjectiles; proj++)
                {
                    var projectile = Main.projectile[proj];
                    if (!projectile.active)
                        continue;

                    // We don't wanna make ALL spears return, because multiplayer certainly does exist!
                    if (projectile.type == ModContent.ProjectileType<TriploonSpear>() && projectile.owner == Main.myPlayer)
                        projectile.ai[2] = 1f;
                }

                // The holdout is only killed once all the spears return
                if (Owner.ownedProjectileCounts[ModContent.ProjectileType<TriploonSpear>()] <= 0)
                    Projectile.Kill();
            }

            if (Time == 30f)
            {
                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, GunTipPosition);

                for (int i = 0; i < 3; i++)
                {
                    // This offsets the side spears so that they're not just stacked on top of each other
                    float offsetMult = i * 1.5f - 1.5f;
                    Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * Owner.ActiveItem().shootSpeed;
                    Vector2 offset = velocity.RotatedBy(MathHelper.Pi / 10 * offsetMult);

                    // The initial ai0 value is used to offset when each spear hits
                    if (Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition + offset, velocity, ModContent.ProjectileType<TriploonSpear>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, i * 5f - 5f);
                }
            }
        }
    }
}
