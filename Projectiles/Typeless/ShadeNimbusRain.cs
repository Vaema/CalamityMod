using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ShadeNimbusRain : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 50;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 6;
        }

        public override void AI()
        {
            if (Projectile.Center.Y > Main.player[Projectile.owner].Center.Y)
                Projectile.tileCollide = true;
        }

        public override void OnKill(int timeLeft)
        {
            int shadeDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + (float)Projectile.height - 2f), 2, 2, DustID.Demonite, 0f, 0f, 0, default, 1f);
            Dust dust = Main.dust[shadeDust];
            dust.position.X -= 2f;
            dust.alpha = 38;
            dust.velocity *= 0.1f;
            dust.velocity += -Projectile.oldVelocity * 0.25f;
            dust.scale = 0.95f;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(100, 255, 100, Projectile.alpha);

        public override void OnHitNPC(NPC target, NPC.HitInfo info, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrainRot>(), 60);
            if (Projectile.numHits > 0)
            {
                Projectile.damage = (int)(Projectile.damage * 0.75f);
                if (Projectile.damage < 1)
                    Projectile.damage = 1;
            }
        }
    }
}
