using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Items.Weapons.Summon.SquirrelSquireStaff;

namespace CalamityMod.Projectiles.Summon
{
    public class SquirrelSquireAcorn : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";

        public override void SetStaticDefaults() => ProjectileID.Sets.SentryShot[Type] = true;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = ProjectileTimeAlive;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.velocity.Y += ProjectileGravity;
            Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.X * 3f);
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.ExpandHitboxBy(ProjectileAoERadiusSize * 2);
            if (Main.myPlayer == Projectile.owner)
                Projectile.Damage();

            if (Main.dedServ)
                return;

            for (int k = 0; k < 5; k++)
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.WoodFurniture, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
        }
    }
}
