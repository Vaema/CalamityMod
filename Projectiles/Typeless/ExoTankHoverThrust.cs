using CalamityMod.Items.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ExoTankHoverThrust : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults() => Main.projFrames[Type] = 5;

        public override void SetDefaults()
        {
            Projectile.width = 186;
            Projectile.height = 74;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            if (Owner is null || Owner.dead || Owner.mount is null || !Owner.mount.Active)
                Projectile.Kill();

            if (Owner.mount._type != ModContent.MountType<ExoTank>())
                Projectile.Kill();

            var data = (ExoTank.ExoTankData)Owner.mount._mountSpecificData;
            if (!data.Hovering)
                Projectile.Kill();

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 3 % Main.projFrames[Type];

            Projectile.Center = Owner.Bottom + Vector2.UnitY * (Projectile.height / 2) + Vector2.UnitX * (Owner.direction == 1 ? 0f : 8f);
            Projectile.timeLeft = 2;
        }

        public override bool ShouldUpdatePosition() => false;

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
