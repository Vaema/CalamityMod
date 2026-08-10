using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic;

public class PlasmaRifleExplosion : BaseMassiveExplosionProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override int Lifetime => 40;
    public override bool UsesScreenshake => true;
    public override float GetScreenshakePower(float pulseCompletionRatio) => CalamityUtils.Convert01To010(pulseCompletionRatio) * 3f;
    public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Chartreuse;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        Projectile.timeLeft = Lifetime;
        Projectile.DamageType = DamageClass.Magic;
    }

    public override void PostAI() => Lighting.AddLight(Projectile.Center, Color.Chartreuse.ToVector3() * Projectile.Opacity * 0.7f);
}
