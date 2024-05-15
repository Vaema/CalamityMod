using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class GenesisBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public ref float isSplit => ref Projectile.ai[1];
        public bool splitShot = false;

        public Color mainColor = Color.MediumSlateBlue;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 31;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 75;
            Projectile.timeLeft = 20;
        }
        public override void AI()
        {
            if (isSplit == 0)
                splitShot = true;
            if (time == 0)
            {
                if (splitShot)
                    Projectile.penetrate = 1;
                else
                    Projectile.timeLeft = 50;
            }

            if (Projectile.timeLeft % 2 == 0 && (!splitShot && time > 2 || (splitShot && time > 3 && Projectile.timeLeft > 2)))
            {
                Particle spark = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 17, 0.06f, mainColor, new Vector2(0.5f, 1.3f));
                GeneralParticleHandler.SpawnParticle(spark);
            }

            Vector2 dustVel = new Vector2(2, 2).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + dustVel, Main.rand.NextBool(4) ? 264 : 66, dustVel, 0, default, Main.rand.NextFloat(0.9f, 1.2f));
            dust.noGravity = true;
            dust.color = Main.rand.NextBool() ? Color.Lerp(mainColor, Color.White, 0.5f) : mainColor;

            time++;
        }
        public override void OnKill(int timeLeft)
        {
            int numProj = 2;
            float rotation = MathHelper.ToRadians(20);
            if (splitShot)
            {
                for (int i = 0; i < numProj; i++)
                {
                    Vector2 perturbedSpeed = Projectile.velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProj - 1)));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<GenesisBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 1f);
                    for (int k = 0; k < 3; k++)
                    {
                        Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, mainColor, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.5f, 0.4f, 35);
                        GeneralParticleHandler.SpawnParticle(blastRing);
                        Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.4f, 0.3f, 35);
                        GeneralParticleHandler.SpawnParticle(blastRing2);
                    }
                }

                for (int i = 0; i < 20; i++)
                {
                    Vector2 dustVel = new Vector2(13, 13).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.8f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + dustVel, Main.rand.NextBool(4) ? 264 : 66, dustVel, 0, default, Main.rand.NextFloat(0.9f, 1.2f));
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool() ? Color.Lerp(mainColor, Color.White, 0.5f) : mainColor;
                }
            }
            else
            {
                for (int i = 0; i < 18; i++)
                {
                    Vector2 dustVel = Projectile.velocity * Main.rand.NextFloat(0.1f, 1.5f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + dustVel + Main.rand.NextVector2Circular(6, 6), Main.rand.NextBool(4) ? 264 : 66, dustVel, 0, default, Main.rand.NextFloat(0.9f, 1.2f));
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool() ? Color.Lerp(mainColor, Color.White, 0.5f) : mainColor;
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (splitShot) // This is the sweet spot
            {
                modifiers.SourceDamage *= 1.5f;

                int points = 10;
                float radians = MathHelper.TwoPi / points;
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
                for (int k = 0; k < points; k++)
                {
                    Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f * rotRando);
                    Particle subTrail = new GlowSparkParticle(Projectile.Center + velocity * 10.5f, velocity * 5, false, 10, 0.045f, mainColor, new Vector2(2, 0.4f), true);
                    GeneralParticleHandler.SpawnParticle(subTrail);
                }

                SoundStyle fire = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeImpact");
                SoundEngine.PlaySound(fire with { Volume = 1.25f, Pitch = 0.4f, PitchVariance = 0.15f }, Projectile.Center);
            }
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.95f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
    }
}
