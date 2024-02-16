using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee
{
    public class FloodtideShark : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 28;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.MiniSharkron;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnKill(int timeLeft)
        {
            for (int d = 0; d < 15; ++d)
            {
                int idx = Dust.NewDust(Projectile.Center - Vector2.One * 10f, 50, 50, DustID.Blood, 0f, -2f, 0, default, 1f);
                Dust dust = Main.dust[idx];
                dust.velocity /= 2f;
            }
        }
    }
}
