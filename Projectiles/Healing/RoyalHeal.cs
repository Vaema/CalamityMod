using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Healing;

public class RoyalHeal : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Healing";

    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 4;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.MaxUpdates = 5;
        Projectile.timeLeft = 120 * Projectile.MaxUpdates;
    }

    public override void AI()
    {
        Projectile.HealingProjectile((int)Projectile.ai[1], (int)Projectile.ai[0], 12f, 10f);
        int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurificationPowder, 0f, 0f, 100);
        Main.dust[dust].noGravity = true;
        Main.dust[dust].velocity *= 0f;
        Main.dust[dust].position.X -= Projectile.velocity.X * 0.2f;
        Main.dust[dust].position.Y += Projectile.velocity.Y * 0.2f;
    }
}
