using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Projectiles.Ranged
{
    public class AquashardSplit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 280 && target.CanBeChasedBy(Projectile);

        public override void AI()
        {
            Projectile.velocity.X *= 0.9995f;
            Projectile.velocity.Y += 0.01f;

            if (Projectile.timeLeft < 280)
                CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 450f, Projectile.ai[1] == 1f ? 8f : 6f, 20f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.5f, PitchVariance = 0.3f }, Projectile.Center);
            Particle Star = new CritSpark(Projectile.Center, Vector2.Zero, Color.SkyBlue, Color.DarkTurquoise, Main.rand.NextFloat(1f, 1.1f), 30, 0.1f, 3f);
            GeneralParticleHandler.SpawnParticle(Star);
            int num190 = Main.rand.Next(5, 9);
            for (int i = 0; i < num190; i++)
            {
                int bubbly = Dust.NewDust(Projectile.Center, 0, 0, DustID.UnusedWhiteBluePurple, 0f, 0f, 100, default, 1.4f);
                Main.dust[bubbly].velocity *= 0.8f;
                Main.dust[bubbly].position = Vector2.Lerp(Main.dust[bubbly].position, Projectile.Center, 0.5f);
                Main.dust[bubbly].noGravity = true;
            }
        }
    }
}
