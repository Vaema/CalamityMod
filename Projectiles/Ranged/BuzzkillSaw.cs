using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class BuzzkillSaw : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            Projectile.ai[1]++;
            if (Projectile.ai[1] == 1f)
                Projectile.rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);

            if (Projectile.frame < 1)
                Projectile.frame = 1;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                    Projectile.frame = 1;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item178, Projectile.Center); // Probably placeholder sound?

            for (int d = 0; d < 8; d++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                dustVel.SafeNormalize(Vector2.Zero);
                dustVel *= Main.rand.NextFloat(3f, 7f);

                Dust collisionDust = Dust.NewDustPerfect(Projectile.Center, 84, dustVel);
                collisionDust.noGravity = true;
            }

            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int p = 0; p < 6; p++)
            {
                Particle hitSparks = new AltLineParticle(target.Center, new Vector2(Main.rand.NextFloat(-6.5f, 6.5f), Main.rand.NextFloat(-6.5f, 6.5f)), false, 30, 0.6f, new Color(112, 16, 16));
                GeneralParticleHandler.SpawnParticle(hitSparks);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/CeramicImpact", 2), Projectile.Center);

            for (int d = 0; d < 8; d++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                dustVel.SafeNormalize(Vector2.Zero);
                dustVel *= Main.rand.NextFloat(5f, 9f);

                Dust collisionDust = Dust.NewDustPerfect(Projectile.Center, 84, dustVel);
                collisionDust.noGravity = true;
            }

            int goreToExclude = Main.rand.Next(3);
            switch (goreToExclude)
            {
                case 0:
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw2").Type, 0.8f);
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw3").Type, 0.8f);
                    break;
                case 1:
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw1").Type, 0.8f);
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw3").Type, 0.8f);
                    break;
                case 2:
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw1").Type, 0.8f);
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw2").Type, 0.8f);
                    break;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[1] > 10)
            {
                Texture2D slashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillSawSlash").Value;
                Color drawColor = Color.White;
                Main.EntitySpriteDraw(slashTexture, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f)), null, drawColor, Projectile.ai[1] * 7f, slashTexture.Size() / 2, 1f, SpriteEffects.None);

                if (Projectile.ai[1] % 4 == 0)
                {
                    Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width, Projectile.width), Main.rand.NextFloat(-Projectile.width, Projectile.width));
                    float randomParticleScale = Main.rand.NextFloat(0.35f, 0.65f);
                    Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : new Color(112, 16, 16), randomParticleScale, randomParticleScale, 4, false);
                    GeneralParticleHandler.SpawnParticle(bloomCircle);
                }
            }
            return true;
        }
    }
}
