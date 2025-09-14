using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class ScorchedEarthClusterBomb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.95f;
            if (Projectile.velocity.Length() < 0.5f && Projectile.timeLeft > 10)
                Projectile.timeLeft = 10;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                int size = 16;
                Vector2 position = Projectile.Center;
                Vector2 velocity = Main.rand.NextVector2Circular(size, size);
                SquishyLightParticle energy = new(position, velocity, Main.rand.NextFloat(0.2f, 0.3f), Color.DarkOrange * 0.5f, Main.rand.Next(6, 9), 1, 1.5f);
                GeneralParticleHandler.SpawnParticle(energy);
                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, velocity, 0, default, Main.rand.NextFloat(1f, 2f));
                dust.noGravity = true;
            }
            int projAmt = Main.rand.Next(2, 4);
            if (Projectile.owner == Main.myPlayer)
            {
                for (int k = 0; k < projAmt; k++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<RocketFire>(), Projectile.damage, 0f, Projectile.owner);
                }
            }
        }
    }
}
