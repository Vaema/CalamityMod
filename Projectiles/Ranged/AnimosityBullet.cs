using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Systems;
using Microsoft.CodeAnalysis;

namespace CalamityMod.Projectiles.Ranged
{
    internal class AnimosityBullet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 300;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }
        public override void AI()
        {
            Projectile.scale = 1.4f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Time++;

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.9f, 0f, 0.15f);

            // Visuals
            if (Time > 3f)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 1.8f, -Projectile.velocity * 0.01f, false, 11, 1.6f, (Main.zenithWorld ? Color.MediumPurple : Color.Red) * 0.65f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            
            for (int i = 0; i <= 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextBool(3) ? 90 : 60, -Projectile.velocity.RotatedBy(-0.5) * Main.rand.NextFloat(0.05f, 0.2f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.5f, 1.1f);
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextBool(3) ? 90 : 60, -Projectile.velocity.RotatedBy(0.5) * Main.rand.NextFloat(0.05f, 0.2f));
                dust2.noGravity = true;
                dust2.scale = Main.rand.NextFloat(0.5f, 1.1f);
            }
        }
        public override void Kill(int timeLeft)
        { 
            SoundEngine.PlaySound(SoundID.NPCDeath55 with { Pitch = -0.7f }, Projectile.Center);
            //DesertProwelerSkullParticle was a placeholder, but honestly it fits too well
            for (int i = 0; i <= 11; i++)
            {
                Particle skull = new DesertProwlerSkullParticle(Projectile.Center, new Vector2(2.5f, 2.5f).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f), Main.rand.NextBool() ? Color.Crimson : Color.DarkRed, Color.Red, Main.rand.NextFloat(0.2f, 0.9f), 175);
                GeneralParticleHandler.SpawnParticle(skull);
            }
            for (int i = 0; i <= 25; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 90 : 60, new Vector2(0, -5).RotatedByRandom(MathHelper.ToRadians(35f)) * Main.rand.NextFloat(0.1f, 1.9f));
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(0.9f, 1.5f);
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 90 : 60, new Vector2(0, -2).RotatedByRandom(MathHelper.ToRadians(35f)) * Main.rand.NextFloat(0.1f, 1.9f));
                dust2.noGravity = false;
                dust2.scale = Main.rand.NextFloat(0.9f, 1.5f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300);
            target.AddBuff(ModContent.BuffType<WhisperingDeath>(), 120);

            // Music easter egg in GFB, and more!
            if (Main.zenithWorld)
            {
                target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 45);
                target.AddBuff(BuffID.ShadowFlame, 120);
                GungeonMusicSystem.GUN();
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Crimson * 0.45f, 1);
            return true;
        }
    }
}
