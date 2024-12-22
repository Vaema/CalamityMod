using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class WaywasherProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float time => ref Projectile.ai[0];
        public int dir = 0;
        public bool returning = false;
        public override void SetDefaults()
        {
            Projectile.width = 55;
            Projectile.height = 55;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 225;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            if (time == 0)
                dir = (int)Projectile.ai[1];
            if (time == 4)
            {
                Particle fadeInfx = new CustomSpark(Projectile.Center, Projectile.velocity * 0.3f, "CalamityMod/Particles/BloomCircle", false, 13, 0.75f, Color.White, new Vector2(1, 1f), true, false, shrinkSpeed: 0.3f);
                GeneralParticleHandler.SpawnParticle(fadeInfx);
                for (int i = 0; i < 3; i++)
                {
                    Particle fadeInfx2 = new CustomSpark(Projectile.Center, Projectile.velocity.RotatedByRandom(0.4f), "CalamityMod/Particles/WaterFoam", true, 13, 0.45f, Color.DeepSkyBlue, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-1, 1), shrinkSpeed: 0.1f);
                    GeneralParticleHandler.SpawnParticle(fadeInfx2);
                }
            }
            Lighting.AddLight(Projectile.Center, Color.RoyalBlue.ToVector3() * 0.7f);

            // Slows down and decays if it's inside tiles
            if (Collision.SolidCollision(Projectile.Center, 20, 20))
            {
                Projectile.velocity *= 0.95f;
                Projectile.timeLeft = (int)(Projectile.timeLeft * 0.9f);
            }

            // I hate this but it's the only way to make it move in the very specific way it needs to
            int timeValue = (int)(time / 12);
            float turnMult = timeValue switch
            {
                0 => 0.05f,
                1 => 0.1f,
                2 => 0.2f,
                3 => -0.2f,
                4 => -0.3f,
                5 => -0.4f,
                6 => -0.6f,
                7 => -0.9f,
                8 => -1.13f,
                _ => -1.7f,
            };
            if (timeValue == 9)
            {
                returning = true;
                for (int i = 0; i < Main.maxNPCs; i++)
                    Projectile.localNPCImmunity[i] = 0;
                Projectile.numHits = 0;
            }
            Projectile.velocity = Projectile.velocity.RotatedBy(0.2f * turnMult * dir);

            // Making things look convincingly "watery" takes a lot of visual work...
            Visuals();
            
            time += returning ? -1 : 1;
        }
        public void Visuals()
        {
            if (Main.rand.NextBool())
            {
                // If you're looking at this to use the water effects, this is the main one I recommend you take
                Particle waterfx = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.7f, "CalamityMod/Particles/WaterFoam", false, 9, 0.45f, Color.DeepSkyBlue, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-1, 1), shrinkSpeed: 0.5f);
                GeneralParticleHandler.SpawnParticle(waterfx);
            }

            Particle projBody = new CustomSpark(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 25, -Projectile.velocity * 0.3f, "CalamityMod/Particles/BloomCircle", false, 6, 0.3f, Color.RoyalBlue, new Vector2(1, 1f), true, false, shrinkSpeed: 0.5f);
            GeneralParticleHandler.SpawnParticle(projBody);

            Vector2 tip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 35;
            Particle seafoam = new CustomSpark(tip, Projectile.velocity * Main.rand.NextFloat(0.7f, 0.9f), "CalamityMod/Particles/BloomCircle", false, 2, Main.rand.NextFloat(0.35f, 0.45f), Color.White, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-0.4f, 0.4f));
            GeneralParticleHandler.SpawnParticle(seafoam);
            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle smallSeafoam = new CustomSpark(tip, -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.9f, 1.4f), "CalamityMod/Particles/BloomCircle", true, 15, Main.rand.NextFloat(0.12f, 0.18f), Color.White * 0.7f, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-1, 1));
                    GeneralParticleHandler.SpawnParticle(smallSeafoam);

                    Dust dust = Dust.NewDustPerfect(tip + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<LightDust>(), -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.9f, 1.4f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.65f, 0.8f);
                    dust.color = Color.White;
                    dust.noLightEmittence = true;
                }
            }
            if (Main.rand.NextBool(5))
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble.timeLeft = 12 + Main.rand.Next(8);
                bubble.scale = Main.rand.NextFloat(0.5f, 0.9f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);

            float minMult = 0.1f;
            int hitsToMinMult = 5;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override void OnKill(int timeLeft)
        {
            Particle fadeOutfx = new CustomSpark(Projectile.Center, Projectile.velocity * 0.3f, "CalamityMod/Particles/BloomCircle", false, 13, 0.75f, Color.White, new Vector2(1, 1f), true, false, shrinkSpeed: 0.3f);
            GeneralParticleHandler.SpawnParticle(fadeOutfx);
            for (int i = 0; i < 3; i++)
            {
                Particle fadeOutfx2 = new CustomSpark(Projectile.Center, Projectile.velocity.RotatedByRandom(0.4f), "CalamityMod/Particles/WaterFoam", true, 13, 0.45f, Color.DeepSkyBlue, new Vector2(1, 1f), true, false, extraRotation: Main.rand.NextFloat(-1, 1), shrinkSpeed: 0.1f);
                GeneralParticleHandler.SpawnParticle(fadeOutfx2);
            }
        }
    }
}
