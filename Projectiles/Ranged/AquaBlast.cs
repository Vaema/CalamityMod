using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class AquaBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 20;
        }
        public int spreadDust = 0;
        public static int Lifetime = 600;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = Lifetime;
            Projectile.extraUpdates = 2;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            //Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 21)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 19)
            {
                Projectile.frame = 0;
            }
            Projectile.frame = Projectile.frameCounter / 4 % Main.projFrames[Type];

            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(90) * Projectile.direction;


            Lighting.AddLight(Projectile.Center, Color.AliceBlue.ToVector3() * 0.5f);
            //Trailing effects
            if (Projectile.timeLeft <= Lifetime - 7)
            {
                for (int i = 0; i < 2; i++)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity / 0.18f, Projectile.velocity * 0.01f, false, 5, 1.9f, Color.SeaGreen, true);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            if (Main.rand.NextBool(2))
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble.timeLeft = 6 + Main.rand.Next(7);
                bubble.scale = Main.rand.NextFloat(0.6f, 0.8f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }
            if (Main.rand.NextBool())
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6 + spreadDust, 6 + spreadDust), !Main.rand.NextBool(5) ? 278 : 267, -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.35f), 0, default, Main.rand.NextFloat(0.4f, 0.6f));
                dust.noGravity = true;
                dust.color = !Main.rand.NextBool(5) ? Color.Aquamarine : Color.Aqua;
                if (dust.type == 278)
                    dust.scale *= 0.7f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 60);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 60);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;
            int smokeAmount = Main.rand.Next(8, 12 + 1);
            for (int i = 0; i < smokeAmount; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.2f, 2f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.3f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.ShimmerWeak1 with { Pitch = 0.35f }, Projectile.Center);
            for (int i = 0; i < 5; ++i)
            {
                int bloodLifetime = Main.rand.Next(22, 25);
                float bloodScale = Main.rand.NextFloat(0.6f, 0.8f);
                Color bloodColor = Color.Lerp(Color.SkyBlue, Color.Aquamarine, Main.rand.NextFloat());
                bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                if (Main.rand.NextBool(20))
                    bloodScale *= 2f;

                float randomSpeedMultiplier = Main.rand.NextFloat(1.25f, 2.25f);
                Vector2 bloodVelocity = Main.rand.NextVector2Unit() * 2 * randomSpeedMultiplier;
                bloodVelocity.Y -= 5f;
                BloodParticle blood = new BloodParticle(Projectile.Center, bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                GeneralParticleHandler.SpawnParticle(blood);
            }
            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Aquamarine * 0.8f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.05f, 22);
            GeneralParticleHandler.SpawnParticle(blastRing);
            Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Aqua * 0.8f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.04f, 22);
            GeneralParticleHandler.SpawnParticle(blastRing2);
            Particle blastRing3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DeepSkyBlue, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.2f, 22);
            GeneralParticleHandler.SpawnParticle(blastRing3);
            Particle orb2 = new GenericBloom(Projectile.Center, Vector2.Zero, Color.SkyBlue, 0.6f, 11, false, true);
            GeneralParticleHandler.SpawnParticle(orb2);
        }
    }
}
