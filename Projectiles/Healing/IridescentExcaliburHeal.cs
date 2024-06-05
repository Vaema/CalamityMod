using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Healing
{
    public class IridescentExcaliburHeal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Healing";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            Projectile.HealingProjectile((int)Projectile.ai[1], (int)Projectile.ai[0], 10f, 15f);

            Color dustColor = Color.White;
            switch (Main.rand.Next(3))
            {
                default:
                case 0:
                    dustColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
                    break;

                case 1:
                    dustColor = new Color(255 - Main.DiscoR, 255 - Main.DiscoG, 255 - Main.DiscoB);
                    break;

                case 2:
                    dustColor = new Color(Math.Abs(128 - Main.DiscoR) * 2 - 1, Math.Abs(128 - Main.DiscoG) * 2 - 1, Math.Abs(128 - Main.DiscoB) * 2 - 1);
                    break;
            }

            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 278, 0f, 0f, 100);
            Main.dust[dust].color = dustColor;
            Main.dust[dust].fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0f;
            Main.dust[dust].position.X -= Projectile.velocity.X * 0.2f;
            Main.dust[dust].position.Y += Projectile.velocity.Y * 0.2f;
        }
    }
}
