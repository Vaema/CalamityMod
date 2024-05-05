using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;

namespace CalamityMod.Projectiles.Healing
{
    public class AltTransfusionTrail : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Healing";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public float opacity = 0.3f;

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 5;
        }

        public override void AI()
        {
            Projectile.HealingProjectile((int)Projectile.ai[1], (int)Projectile.ai[0], 15f, MathHelper.Clamp(80f - time, 5, 80));
            if (time == 1)
            {
                bool direction = Main.rand.NextBool();
                Projectile.velocity = (Projectile.velocity * 10).RotatedBy(direction ? Main.rand.NextFloat(0.6f, 1.8f) : Main.rand.NextFloat(-0.6f, -1.8f)) * 2f;
            }

            SparkParticle orb = new SparkParticle(Projectile.Center, -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.1f), false, 5, 0.6f, Color.White * opacity);
            GeneralParticleHandler.SpawnParticle(orb);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(5) ? 63 : 91, -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.25f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.75f, 0.9f);
                dust.alpha = 100;
                dust.noLight = true;
            }
            
            if (Projectile.timeLeft < 300)
            {
                Projectile.extraUpdates = 12;
                if (opacity > 0)
                    opacity -= 0.01f;
            }
            time++;
        }
        public override void OnKill(int timeLeft)
        {
            Player Owner = Main.player[Projectile.owner];
            for (int i = 0; i < 2; ++i)
            {
                Particle Plus = new HealingPlus(Owner.Center - new Vector2(Main.rand.Next(-4, 24 + 1), 2), Main.rand.NextFloat(0.2f, 0.5f), new Vector2(0, Main.rand.NextFloat(-2f, -4.5f)) + Owner.velocity, Color.SlateGray * 0.75f, Color.White * 0.75f, Main.rand.Next(15, 20));
                GeneralParticleHandler.SpawnParticle(Plus);
            }
        }
    }
}
