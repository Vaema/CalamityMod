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
            Projectile.scale = 0f;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.timeLeft = 120;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 150 && target.CanBeChasedBy(Projectile);

        public override void AI()
        {
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.2f);

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
            if (Projectile.frameCounter == 0)
            {
                GeneralParticleHandler.SpawnParticle(new SmallSmokeParticle(
                Projectile.Center, new Vector2(2, 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)), Color.White, Color.LightSkyBlue, (1f + Main.rand.NextFloat(0.3f)) * Projectile.scale, 50f, affectedByLight: true));
            }
            Projectile.ai[1]++;
            if (Projectile.ai[1] > 40)
            {
                if (Projectile.Calamity().stealthStrike)
                {
                    CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 450f, 18, 0.1f);
                }
                else
                {
                    CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 450f, 10f, 0.02f);
                }
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
