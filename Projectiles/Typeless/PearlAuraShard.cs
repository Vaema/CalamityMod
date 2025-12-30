using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class PearlAuraShard : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Projectile.timeLeft < 140)
                CalamityUtils.HomeInOnNPC(Projectile, true, 200f, 10f, 20f);

            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0.2f / 255f, (255 - Projectile.alpha) * 0.2f / 255f);
            Projectile.rotation = Projectile.velocity.ToRotation() - (MathHelper.PiOver2 * Projectile.spriteDirection);
            for (int i = 0; i < 4; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Flare_Blue, 0f, 0f, 100, default, 0.6f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
                Main.dust[dust].velocity += Projectile.velocity * 0.1f;
            }
        }
    }
}
