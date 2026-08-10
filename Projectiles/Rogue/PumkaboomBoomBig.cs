using Microsoft.Xna.Framework;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue;

public class PumpkaboomBoomBig : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 230, targetHitbox);
}
