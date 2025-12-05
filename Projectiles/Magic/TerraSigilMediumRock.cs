using System;
using CalamityMod.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Particles;
using Terraria.Audio;
using CalamityMod.Dusts;

namespace CalamityMod.Projectiles.Magic
{
    public class TerraSigilMediumRock : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public ref float Time => ref Projectile.ai[0];
        public bool goToCursor = false;
        public Vector2 mousePos;
        public float CenterX;
        public float CenterY;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 75;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Time < 69)
                Projectile.friendly = false;
            else
                Projectile.friendly = true;

            if (Time == 0)
            {
                Projectile.scale = 0.5f;
            }

            // If not moving to cursor, slow down
            if (!goToCursor)
            {
                Projectile.velocity *= 0.96f;
            }

            if (Time < 11)
                Projectile.scale *= 1.08f;

            // After 1 second, rapidly move to cursor
            if (Time == 60)
            {
                Projectile.velocity = Vector2.Zero;
                mousePos = Main.player[Projectile.owner].ClampedMouseWorld();
                goToCursor = true;
            }

            if (goToCursor)
            {
                if (Time == 60)
                {
                    CenterX = Projectile.Center.X;
                    CenterY = Projectile.Center.Y;
                }
                if (Time > 60)
                    Projectile.Center = new Vector2(MathHelper.Lerp(CenterX, mousePos.X, Utils.GetLerpValue(60, 73, Time, true)), MathHelper.Lerp(CenterY, mousePos.Y, Utils.GetLerpValue(60, 73, Time, true)));
            }

            Projectile.rotation += 0.05f;
            Time++;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/Ravager/RavagerJump2") { Volume = 0.7f, PitchVariance = 0.1f }, Projectile.Center);

            int rockCount = Main.rand.Next(1, 3);
            for (int i = 0; i < rockCount; i++)
            {
                Vector2 randomVelocity = new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-3f, -10f));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randomVelocity, ModContent.ProjectileType<TerraSigilSmallRock>(), 0, 0, Projectile.owner);
            }

            // Dust and smoke particles
            for (int i = 0; i < 14; i++)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? ModContent.DustType<TerraSigilDust>() : 262, new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.8f)); // Reduced velocity
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.6f, 1f); 
                    if (dust.type == 262)
                    {
                        dust.noGravity = true;
                        dust.fadeIn = 0.3f; 
                        dust.velocity *= 1.5f; 
                    }
                    dust.alpha = 100;
                }
                else
                {
                    Color clr = Color.Lerp((Main.rand.NextBool() ? Color.Peru : Color.PeachPuff), Color.Black, Main.rand.NextFloat(0.25f, 0.45f));
                    Particle sand = new CustomSpark(Projectile.Center, (Vector2.One * Main.rand.NextFloat(3, 8)).RotatedByRandom(MathHelper.TwoPi), "CalamityMod/Particles/SmallSmoke", true, Main.rand.Next(15, 30 + 1), Projectile.scale * Main.rand.NextFloat(0.05f, 0.1f) * 4, clr, new Vector2(1, Main.rand.NextFloat(0.2f, 1f)), false, extraRotation: Main.rand.NextFloat(-2, 2), spin: Main.rand.NextFloat(-0.5f, 0.5f), affectedByLight: true);
                    GeneralParticleHandler.SpawnParticle(sand);
                }
            }

            for (int i = 0; i < 3; i++) 
            {
                Vector2 randVel = new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.4f, 1f); // Reduced velocity
                Particle smoke = new HeavySmokeParticle(Projectile.Center + randVel, randVel, Color.Peru, Main.rand.Next(15, 25 + 1), Main.rand.NextFloat(0.6f, 1.5f), 0.3f); // Reduced duration and scale
                GeneralParticleHandler.SpawnParticle(smoke);
                MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center, randVel * 0.5f, Color.SandyBrown, Color.Beige, Main.rand.NextFloat(0.2f, 0.8f), 80f, Main.rand.NextFloat(0.02f, -0.02f)); // Reduced duration and scale
                GeneralParticleHandler.SpawnParticle(SandCloud);
            }
        }
    }
}
