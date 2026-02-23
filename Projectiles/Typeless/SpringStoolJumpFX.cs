using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class SpringStoolJumpFX : ModProjectile, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 78;
            Projectile.height = 142;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];


            Projectile.rotation -= 0.005f;

            if (Projectile.frame < 4)
            {
                Projectile.frameCounter++;

                if (Projectile.frameCounter >= 5)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;

                    if (Projectile.frame > 4)
                        Projectile.frame = 4; // clamp to last frame
                }
            }

            
            Projectile.ai[0]++;

            /* if (Projectile.ai[0] < 10f) // Stick to bottom of player initially
            {
                Projectile.Center = player.Bottom + new Vector2(0f, -50f);
                Projectile.velocity = Vector2.Zero;
            } */

            if (Projectile.ai[0] >= 20f) // Begin fading
            {
                Projectile.alpha += 8;
                Projectile.scale *= 0.985f;
            }

            if (Projectile.alpha >= 255 || !player.active || player.dead)
                Projectile.Kill();
        }

        public override bool? CanDamage() => false;
    }
}
