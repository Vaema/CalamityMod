using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless;

public class ShadeNimbus : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Typeless";
    public override string Texture => "CalamityMod/Projectiles/Boss/ShadeNimbusHostile";

    public const float Lifespan = 210f;
    public ref float RainTimer => ref Projectile.ai[0];

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 6;
    }

    public override void SetDefaults()
    {
        Projectile.width = 54;
        Projectile.height = 24;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 8)
        {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }
        }

        if (Projectile.velocity.Length() > 0.5f)
            Projectile.velocity *= 0.9765f;
        else
            Projectile.velocity = Vector2.Zero;

        // Push away horizontally from other shade nimbuses
        float pushForce = 0.02f;
        for (int k = 0; k < Main.maxProjectiles; k++)
        {
            Projectile otherProj = Main.projectile[k];
            if (!otherProj.active || k == Projectile.whoAmI)
                continue;

            bool sameProjType = otherProj.type == Projectile.type;
            float taxicabDist = Vector2.Distance(Projectile.Center, otherProj.Center);
            float distanceGate = 20f;
            if (sameProjType && taxicabDist < distanceGate)
            {
                if (Projectile.position.X < otherProj.position.X)
                    Projectile.velocity.X -= pushForce;
                else
                    Projectile.velocity.X += pushForce;
            }
        }

        Projectile.ai[1] += 1f;
        if (Projectile.ai[1] >= Lifespan)
        {
            Projectile.alpha += 5;
            if (Projectile.alpha > 255)
            {
                Projectile.alpha = 255;
                Projectile.Kill();
            }
        }
        else if (Projectile.ai[1] >= 15f)
        {
            RainTimer += 1f;
            if (RainTimer > 8f)
            {
                RainTimer = 0f;
                if (Projectile.owner == Main.myPlayer)
                {
                    float rainX = Projectile.position.X + Main.rand.NextFloat(14f, Projectile.width - 28f);
                    float rainY = Projectile.position.Y + Projectile.height + 4f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), rainX, rainY, 0f, 10f, ModContent.ProjectileType<ShadeNimbusRain>(), Projectile.damage, 0f, Projectile.owner, 0f, 0f);
                }
            }
        }
    }

    // The cloud doesn't deal damage, just the rain
    public override bool? CanDamage() => false;
}
