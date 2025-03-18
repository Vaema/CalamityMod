using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class ScattershotLaser : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private int dust = 127;
        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 7;
            Projectile.height = 7;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 9;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.4f);
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (time > 15 && targetDist < 1400)
            {
                if (Projectile.timeLeft % 6 == 0)
                {
                    Particle spark = new LineParticle(Projectile.Center, Projectile.velocity * 0.01f, false, 8, 1.7f, Color.Red);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Projectile.timeLeft % 5 == 0)
                {
                    SparkParticle spark2 = new SparkParticle(Projectile.Center, Projectile.velocity * 0.01f, false, 3, 0.7f, Color.White);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
            
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = (Projectile.velocity * 5).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 0.8f);
                Particle nanoDust = new NanoParticle(Projectile.Center, velocity, Main.rand.NextBool(3) ? Color.Crimson : Color.Red, Main.rand.NextFloat(0.4f, 1f), 20, Main.rand.NextBool(), true);
                GeneralParticleHandler.SpawnParticle(nanoDust);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 278, (Projectile.velocity * 5).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
                dust.color = Color.Red;
            }
        }
    }
}
