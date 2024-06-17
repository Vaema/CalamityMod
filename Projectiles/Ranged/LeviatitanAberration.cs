using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class LeviatitanAberration : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Summon/GastricBelcher";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 150f, 12f, 20f);

            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(360) * Projectile.direction;

            //Update frames
            if (Projectile.frameCounter++ % 6 == 0)
            {
                Projectile.frame++;
            }
            if (Projectile.frame >= Main.projFrames[Projectile.type])
            {
                Projectile.frame = 0;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            int dustAmt = 36;
            for (int dustIndex = 0; dustIndex < dustAmt; dustIndex++)
            {
                int randomDust = Utils.SelectRandom(Main.rand, new int[]
                {
                        33,
                        89
                });
                Vector2 direction = Vector2.Normalize(Projectile.velocity) * new Vector2(Projectile.width / 2f, Projectile.height) * 0.75f;
                direction = direction.RotatedBy((double)((dustIndex - (dustAmt / 2f - 1f)) * MathHelper.TwoPi / dustAmt), default) + Projectile.Center;
                Vector2 dustVel = direction - Projectile.Center;
                int water = Dust.NewDust(direction + dustVel, 0, 0, randomDust, dustVel.X * 1.75f, dustVel.Y * 1.75f, 100, default, 1.1f);
                Main.dust[water].noGravity = true;
                Main.dust[water].velocity = dustVel;
            }
            Particle shootPulse = new DirectionalPulseRing(Projectile.Center,
            Vector2.Zero,
            Color.LimeGreen * 0.7f,
            new Vector2(0.5f, 1f),
            Projectile.rotation,
            0.1f,
            0.4f,
            20);
            GeneralParticleHandler.SpawnParticle(shootPulse);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 120);
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = SoundID.NPCDeath1.Volume * 0.5f }, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            for (int d = 0; d < 15; ++d)
            {
                int idx = Dust.NewDust(Projectile.Center - Vector2.One * 10f, 50, 50, DustID.Blood, 0f, -2f, 0, default, 1f);
                Dust dust = Main.dust[idx];
                dust.velocity /= 2f;
            }
        }
    }
}
