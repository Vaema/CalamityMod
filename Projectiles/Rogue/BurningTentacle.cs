using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class BurningTentacle : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 40;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = 3;
        Projectile.MaxUpdates = 3;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI()
    {
        Projectile.scale = 1f - Projectile.localAI[0];
        Projectile.height = Projectile.width = (int)(20f * Projectile.scale);

        if (Projectile.localAI[0] < 0.1f)
            Projectile.localAI[0] += 0.01f;
        else
            Projectile.localAI[0] += 0.025f;

        if (Projectile.localAI[0] >= 0.95f)
            Projectile.Kill();

        Projectile.velocity.X += Projectile.ai[0];
        Projectile.velocity.Y += Projectile.ai[1];
        if (Projectile.velocity.Length() > 10f)
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10f;

        Projectile.ai[0] *= 1.04f;
        Projectile.ai[1] *= 1.04f;
        if (Projectile.scale < 1f)
        {
            for (int i = 0; i < Projectile.scale * 8f; i++)
            {
                Dust shadowflameDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1.1f);
                shadowflameDust.position = (shadowflameDust.position + Projectile.Center) / 2f;
                shadowflameDust.noGravity = true;
                shadowflameDust.velocity *= 0.1f;
                shadowflameDust.velocity -= Projectile.velocity * (1.3f - Projectile.scale);
                shadowflameDust.fadeIn = 100;
                shadowflameDust.scale += Projectile.scale * 0.75f;
            }
        }
    }
}
