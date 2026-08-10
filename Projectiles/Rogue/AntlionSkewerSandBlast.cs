using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class AntlionSkewerSandBlast : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SandBallGun;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.MaxUpdates = 2;
        Projectile.timeLeft = 60 * Projectile.MaxUpdates;
        Projectile.DamageType = RogueDamageClass.Instance;
    }

    public override void AI()
    {
        Projectile.rotation += 0.1f;
        if (Main.rand.NextBool())
        {
            Dust sand = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.Sand, Main.rand.NextVector2Circular(2f, 2f));
            sand.noGravity = true;
        }
    }
}
