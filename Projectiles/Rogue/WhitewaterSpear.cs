using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class WhitewaterSpear : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";

    public ref float time => ref Projectile.ai[0];
    public Vector2 startVel;
    public Vector2 startPos;
    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 64;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 230;
        Projectile.extraUpdates = 5;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20 * Projectile.MaxUpdates;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 0.4f);
        if (time == 0)
        {
            startVel = Projectile.velocity;
            startPos = Projectile.Center;
        }

        if (Projectile.timeLeft % 3 == 0)
        {
            Particle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 2, -Projectile.velocity * 0.05f, false, 14, 1f, Color.LightBlue * 0.2f);
            GeneralParticleHandler.SpawnParticle(spark);
        }

        if (time > 80)
        {
            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, -startVel.X, 0.02f);
            Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, -startVel.Y, 0.02f);
        }
        else
            Projectile.velocity *= 0.995f;

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Wet, 180);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 15, targetHitbox);
}
