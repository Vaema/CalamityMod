using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class DestructionExplosion : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
    public Player Owner => Main.player[Projectile.owner];
    private float ExplosionRadius = 400f;

    public override void SetDefaults()
    {
        //These shouldn't matter because its circular
        Projectile.width = 200;
        Projectile.height = 200;
        Projectile.friendly = true;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }
    public override void AI()
    {
        if (Projectile.ai[0] == 1)
        {
                ExplosionRadius = 800;
        }
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Projectile.numHits > 0)
            Projectile.damage = (int)(Projectile.damage * 0.88f);
        if (Projectile.damage < 1)
            Projectile.damage = 1;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, ExplosionRadius, targetHitbox);
}
