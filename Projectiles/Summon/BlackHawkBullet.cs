using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon;

public class BlackHawkBullet : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Summon";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionShot[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 6;
        Projectile.light = 0.5f;
        Projectile.alpha = 0;
        Projectile.scale = 1.18f;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        Projectile.tileCollide = false;
        Projectile.MaxUpdates = 6;
        Projectile.penetrate = 5;
        Projectile.timeLeft = 60 * Projectile.MaxUpdates;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        AIType = ProjectileID.BulletHighVelocity;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 200);
}
