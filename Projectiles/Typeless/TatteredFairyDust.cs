using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class TatteredFairyDust : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.9f, 0.9f, 0.9f);
            if (Projectile.timeLeft % 2 == 0)
            {
                Dust dustEffect = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BatScepter, Main.rand.NextFloat(0.1f), Main.rand.NextFloat(0.1f));
                dustEffect.noGravity = true;
                Dust lightedDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.TintableDustLighted, Main.rand.NextFloat(0.1f), Main.rand.NextFloat(0.1f), 0, new Color(190, 3, 252));
                lightedDust.noGravity = true;
            }
        }
    }
}
