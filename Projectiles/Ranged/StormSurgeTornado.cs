using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged;

public class StormSurgeTornado : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Ranged";
    public override string Texture => "CalamityMod/Projectiles/Melee/BrinySpout";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 6;
    }

    public override void SetDefaults()
    {
        Projectile.width = 160;
        Projectile.height = 42;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = 2;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0f, 1.25f, 1.25f);

        Projectile.ai[0]++;
        Projectile.scale = 0.25f * MathF.Pow(1.03f, Projectile.ai[0]);

        if (Projectile.scale >= 1f)
            Projectile.Kill();

        Projectile.frameCounter++;
        Projectile.frame = Projectile.frameCounter / 3 % Main.projFrames[Type];
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);
}
