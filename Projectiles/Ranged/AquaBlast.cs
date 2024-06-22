using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class AquaBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 2;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            //Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }

            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(90) * Projectile.direction;


            Lighting.AddLight(Projectile.Center, Color.AliceBlue.ToVector3() * 0.5f);

            if (Projectile.timeLeft <= 580 && Projectile.timeLeft % 5 == 0)
            {
                Particle spark = new LineParticle(Projectile.Center - Projectile.velocity + Main.rand.NextVector2Circular(15, 15), -Projectile.velocity * Main.rand.NextFloat(0.2f, 1.8f), false, Main.rand.Next(9, 20 + 1), Main.rand.NextFloat(0.8f, 1.2f), Color.Lerp(Color.Cyan, Color.AliceBlue, Main.rand.NextFloat(0.35f)) * Main.rand.NextFloat(0.15f, 0.5f));
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 103, -Projectile.velocity.RotatedByRandom(0.1) * Main.rand.NextFloat(0.1f, 0.3f), 0, default, Main.rand.NextFloat(0.5f, 1.2f));
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int lol = 0; lol < 12; lol++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 103, new Vector2(7, 7).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f));
                dust.scale = Main.rand.NextFloat(0.5f, 1.2f);
            }
        }
    }
}
