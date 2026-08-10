using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic;

public class CosmicRainbowTrail : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override string Texture => "Terraria/Images/Projectile_251";
    public const int Lifetime = 25;

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.alpha = 255;
        Projectile.light = 0.3f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = Lifetime;
        Projectile.ignoreWater = true;
        Projectile.scale = 1.25f;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 6;
    }

    public override void AI()
    {
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.spriteDirection = (Projectile.velocity.X <= 0f).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Projectile.localAI[0] = 1f;
        }

        Projectile.velocity *= 0.98f;
        if (Projectile.timeLeft < 6)
            Projectile.alpha = 255 - (int)(255f * Projectile.timeLeft / 6f);
        else if (Projectile.timeLeft > Lifetime - 6)
            Projectile.alpha = 255 - (int)(255f * (Lifetime - Projectile.timeLeft) / 6f);
        else
            Projectile.alpha = 0;
    }

    public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
}
