using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class UberBubble : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public Color EffectsColor;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 30;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.975f;

            if (Projectile.alpha > 0)
                Projectile.alpha -= 30;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            Vector2 uber = Projectile.ai[0].ToRotationVector2();
            float projRotation = Projectile.velocity.ToRotation();
            float aiRotation = uber.ToRotation();
            double rotationClamp = aiRotation - projRotation;
            if (rotationClamp > MathHelper.Pi)
                rotationClamp -= MathHelper.TwoPi;
            if (rotationClamp < -MathHelper.Pi)
                rotationClamp -= -MathHelper.TwoPi;

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (Projectile.timeLeft == 26) // Firing effects, we set them on this frame so it comes out of the tip
            {
                for (int i = 0; i <= 10; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(5) ? DustID.HallowSpray : DustID.GemAmethyst, Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 0.5f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.85f, 1.4f);
                }
                for (int i = 0; i <= 2; i++)
                {
                    SquishyLightParticle energy = new(Projectile.Center, Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f, 0.4f), Main.rand.NextFloat(0.2f, 0.4f), Color.Purple, Main.rand.Next(0, 40 + 1), 0.25f, 2f);
                    GeneralParticleHandler.SpawnParticle(energy);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(Effervescence.BurstSound, Projectile.Center);
            Particle Star = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.Purple, Color.PeachPuff, Main.rand.NextFloat(0.5f, 0.6f), 30, 0.1f, 3f);
            GeneralParticleHandler.SpawnParticle(Star);
            Star = new GenericSparkle(Projectile.Center + Projectile.velocity, Vector2.Zero, Color.MediumPurple, Color.LightPink, Main.rand.NextFloat(0.5f, 0.6f), 20, 0.1f, 3f);
            GeneralParticleHandler.SpawnParticle(Star);

            int randDustAmt = Main.rand.Next(4, 6);
            for (int i = 0; i < randDustAmt; i++)
            {
                int purpleDust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Venom, 0f, 0f, 100, default, 1.4f);
                Main.dust[purpleDust].velocity *= 0.8f;
                Main.dust[purpleDust].position = Vector2.Lerp(Main.dust[purpleDust].position, Projectile.Center, 0.5f);
                Main.dust[purpleDust].noGravity = true;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                for (int numBubbles = 0; numBubbles < 3; numBubbles++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(0.5f, 2f), ModContent.ProjectileType<BlueBubble>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
    }
}
