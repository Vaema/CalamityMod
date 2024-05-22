using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class TurbulanceWindSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 0;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.timeLeft = 180;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 150 && target.CanBeChasedBy(Projectile);

        public override void AI()
        {

            float rot = Main.rand.NextFloat(MathHelper.TwoPi);

            Projectile.velocity *= 0.9f;
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.scale += 0.005f;
            }
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 30;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 1)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            if (Projectile.Calamity().stealthStrike) //stealth strike
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] % 3 == 1)
                {
                    GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Projectile.Center, CalamityUtils.RandomVelocity(1f, 2, 5), Color.SkyBlue, 50, 1f, 255));
                }
                if (Projectile.ai[1] > 10) CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 450f, 8f, 20f);
            }
            else
            {
                if (Projectile.ai[1] % 8 == 1)
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(
                        Projectile.Center + new Vector2(50, 0).RotatedBy(rot + MathHelper.ToRadians(180f)), new Vector2(10, 0).RotatedBy(rot), false, 10, 2f, Color.LightSkyBlue, true
                        ));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {

        }
    }
}
