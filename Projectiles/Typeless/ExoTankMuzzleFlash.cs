using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ExoTankMuzzleFlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3 * Main.projFrames[Type])
                Projectile.Kill();
            Projectile.frame = Projectile.frameCounter / 3;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool ShouldUpdatePosition() => false;

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
