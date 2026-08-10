using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic;

public class ForbiddenSunburst : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    private static float ExplosionRadius = 190.0f;

    public override void SetDefaults()
    {
        Projectile.width = 220;
        Projectile.height = 220;
        Projectile.friendly = true;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 150;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 8;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 0.75f, 0.5f, 0f);
        if (Projectile.wet && !Projectile.lavaWet)
        {
            Projectile.Kill();
        }
        float projTimer = 25f;
        if (Projectile.ai[0] > 180f)
        {
            projTimer -= (Projectile.ai[0] - 180f) / 2f;
        }
        if (projTimer <= 0f)
        {
            projTimer = 0f;
            Projectile.Kill();
        }
        projTimer *= 0.7f;
        Projectile.ai[0] += 4f;
        float timerCounter = 0;
        while (timerCounter < projTimer)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, Alpha: 100, Scale: 2.5f);
            dust.noGravity = true;
            dust.position = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
            dust.velocity = Main.rand.NextVector2Circular(25f, 25f);
            timerCounter++;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.OnFire3, 300);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, ExplosionRadius, targetHitbox);
}
