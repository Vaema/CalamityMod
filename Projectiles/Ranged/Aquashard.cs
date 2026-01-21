using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod.Tiles.Abyss;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.Actions.Sprites;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Projectiles.Summon;

namespace CalamityMod.Projectiles.Ranged
{
    public class Aquashard : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            //Rotation
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (time == 1)
            {
                Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;
                int smokeAmount = Main.rand.Next(8, 12 + 1);
                for (int i = 0; i < smokeAmount; i++)
                {
                    Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.2f, 2f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.3f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                    GeneralParticleHandler.SpawnParticle(smoke);

                }
                for (int i = 0; i < 5; i++)
                {
                    Particle spark = new GlowSparkParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.2f, 0.4f), false, 12, 0.009f, Color.DarkTurquoise, new Vector2(1.5f, 0.7f), true, false, 1.3f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            if (time >= 2)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 135 : 279, -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.05f, 0.2f) - new Vector2(0, 1));
                dust.scale = Main.rand.NextFloat(0.6f, 0.8f);
                dust.noGravity = false;
            }
            if (time == 14)
            {
                Projectile.Kill();
            }
            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int projAmt = 3;
            for (int i = 0; i < projAmt; i++)
            {
                Vector2 velocity = CalamityUtils.RandomVelocity(50f, 35f, 50f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AquashardSplit>(), (int)(Projectile.damage * 0.3f), 0f, Projectile.owner);
            }
            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2() * 0.5f;;
                Particle cross = new SparkParticle(Projectile.Center + velocity * 15f, velocity, false, 15, 1.5f, Color.DarkTurquoise, true);
                GeneralParticleHandler.SpawnParticle(cross);
            }
            for (int k = 0; k < 3; k++)
            {
                Particle outerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkSlateBlue, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.5f, 0.25f, 25);
                GeneralParticleHandler.SpawnParticle(outerGlow);
                Particle innerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkTurquoise, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.4f, 0.15f, 25);
                GeneralParticleHandler.SpawnParticle(innerGlow);
            }
            Player Owner = Main.player[Projectile.owner];
            if (Owner.Calamity().GeneralScreenShakePower < 1.2f)
                Owner.Calamity().GeneralScreenShakePower = 1.2f;
            target.AddBuff(ModContent.BuffType<Eutrophication>(), 30);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = 0.8f }, Projectile.Center);
            int projAmt = 3;
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < projAmt; i++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(50f, 35f, 50f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AquashardSplit>(), (int)(Projectile.damage * 0.5f), 0f, Projectile.owner);
                }
            }
            // Create Blast
            float blastSize = 80;
            float minMultiplier = 0.25f;
            int hitsToMinMult = 8;
            Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 0.25f), 0f, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
            blast.DamageType = DamageClass.Ranged;
            blast.ArmorPenetration = 5;
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Rain, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkTurquoise, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-12, 12), 0f, 0.08f, 22);
            GeneralParticleHandler.SpawnParticle(blastRing);
            Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DeepSkyBlue * 0.8f, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.06f, 22, true);
            GeneralParticleHandler.SpawnParticle(blastRing2);
            Particle blastRing3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DeepSkyBlue * 0.8f, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.2f, 22, true);
            GeneralParticleHandler.SpawnParticle(blastRing3);
            for (int i = 0; i < (int)(15); i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f), 0, default, Main.rand.NextFloat(1f, 1.3f));
                dust.noGravity = !Main.rand.NextBool();
                dust.color = Main.rand.NextBool(5) ? Color.DeepSkyBlue : Color.DarkTurquoise;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 2)
            {
                return false;
            }
            return base.PreDraw(ref lightColor);
        }
    }
}
