using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class GaussRifleExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 12;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (target.CanBeMoved(true) && target.CanBeChasedBy(Projectile, false))
                {
                    if (target != null && !CalamityPlayer.areThereAnyDamnBosses)
                    {
                        if (Vector2.Distance(target.Center, Projectile.Center) > 5 && Vector2.Distance(target.Center, Projectile.Center) < 500)
                        {
                            target.velocity += target.Center.DirectionTo(Projectile.Center).SafeNormalize(Vector2.UnitX) * 0.4f;
                            target.Center += target.Center.DirectionTo(Projectile.Center).SafeNormalize(Vector2.UnitX) * 0.8f;
                        }
                    }
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.width * 0.5f * Projectile.scale, targetHitbox);
    }
}
