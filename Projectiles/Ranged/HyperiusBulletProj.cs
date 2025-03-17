using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class HyperiusBulletProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        private Color currentColor = Color.Black;
        public float dustAngle = 0f;
        public bool growing = false;
        public bool dustWave = false;
        public float variance = 0.8f;
        public Vector2 lastPos;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1200;
            Projectile.extraUpdates = 7;
            AIType = ProjectileID.Bullet;
            Projectile.ignoreWater = true;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (currentColor == Color.Black)
            {
                variance = Main.rand.NextFloat(0.7f, 1f);
                dustWave = Main.rand.NextBool();
                Projectile.scale = 1.5f;
                Projectile.velocity *= 0.3f;
                lastPos = Projectile.velocity;
                switch (Main.rand.Next(0, 4 +1))
                {
                    case 4: // Yellow shot
                        currentColor = Color.Yellow * 0.65f;
                        break;
                    case 3: // Magenta shot
                        currentColor = Color.Magenta * 0.65f;
                        break;
                    case 2: // Red shot
                        currentColor = Color.Red * 0.65f;
                        break;
                    case 1: // Blue shot
                        currentColor = Color.Cyan * 0.65f;
                        break;
                    default: // Green shot
                        currentColor = Color.Lime * 0.65f;
                        break;
                }
            }
            if (dustAngle <= -0.5f)
            {
                growing = true;
            }
            if (dustAngle >= 0.5f)
            {
                growing = false;
            }
            dustAngle += (growing ? 0.07f * variance : -0.07f * variance);

            Projectile.localAI[0] += 1f;
            Vector2 orbPos = Projectile.Center + (Projectile.velocity.RotatedBy((dustWave ? 1 : -1) * dustAngle) * 4.5f - Projectile.velocity * 5);
            if (Projectile.localAI[0] > 12f && targetDist < 1200f)
            {
                CustomSpark orb = new CustomSpark(orbPos, Utils.DirectionTo(lastPos, orbPos), "CalamityMod/Particles/BloomCircle", false, 5, (0.55f + MathF.Abs(dustAngle * 0.65f)) * 0.2f, currentColor, new Vector2(1 + MathF.Abs(dustAngle * 0.6f), 1), true, true, 0, false, false, 0.8f - MathF.Abs(dustAngle * 0.7f), 0.7f, 0.8f);
                GeneralParticleHandler.SpawnParticle(orb);

                if (Main.rand.NextBool(7))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Projectile.velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.8f, 1.3f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 0.8f);
                    dust.color = currentColor;
                    dust.noLightEmittence = true;
                }
            }
            lastPos = orbPos;
        }

        // This projectile is always fullbright.
        public override Color? GetAlpha(Color lightColor)
        {
            return currentColor;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.localAI[0] > 25f)
            {
                CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, lightColor);
            }
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int b = 0; b < 3; b++)
                {
                    Vector2 vel = (-oldVelocity.SafeNormalize(Vector2.UnitX) * 5);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel.RotatedByRandom(0.5f) * 0.7f, ModContent.ProjectileType<HyperiusSplit>(), (int)(Projectile.damage * 0.1), 0f, Projectile.owner, 0f, 0f, Main.rand.Next(0, 4 + 1));
                }
            }
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OnHitEffects(target.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            OnHitEffects(target.Center);
        }

        private void OnHitEffects(Vector2 targetPos)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int b = 0; b < 3; b++)
                {
                    Vector2 velocity = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5).RotatedByRandom(0.5f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity * 0.7f, ModContent.ProjectileType<HyperiusSplit>(), (int)(Projectile.damage * 0.1), Projectile.knockBack * 2f, Projectile.owner, 0f, 0f, Main.rand.Next(0, 4 + 1));
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ShadowboltWallHit") with { Volume = 0.25f, Pitch = Main.rand.NextFloat(0.6f, 1f), MaxInstances = -1 }, Projectile.Center);
            for (int b = 0; b < 4; b++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.7f, 1.4f);
                dust.color = currentColor;
                dust.noLightEmittence = true;
            }
        }
    }
}
