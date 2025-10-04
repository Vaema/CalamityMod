using System;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using CalamityMod.Particles;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class M1GarandShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/AMRShot";
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.light = 0.5f;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            float num107 = (float)Math.Sqrt(Projectile.velocity.X * Projectile.velocity.X + Projectile.velocity.Y * Projectile.velocity.Y);
            if (Projectile.alpha > 0)
                Projectile.alpha -= (byte)(num107 * 0.9);
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            if (Projectile.timeLeft == 597)
            {
                for (int i = 0; i <= 15; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.position, 87, (Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.6f, 1.1f));
                    dust.noGravity = true;
                }
            }
            Projectile.scale = 1.38f;
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

            if (Projectile.timeLeft < 597 && Projectile.timeLeft > 450)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 9, 1f, Color.Coral * 0.3f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.alpha < 140)
                return new Color(255, 255, 255, 100);

            return Color.Transparent;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.Center, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            return true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

            for (int i = 0; i <= 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.position, 87, (Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.6f, 1.1f));
                dust.noGravity = true;
            }
        }
    }
}
