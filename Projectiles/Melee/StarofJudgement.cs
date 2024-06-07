using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using CalamityMod.Particles;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CalamityMod.Projectiles.Melee
{
    public class StarofJudgement : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public ref float time => ref Projectile.ai[0];
        public Color mainColor;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (time == 0)
            {
                Projectile.scale = (Projectile.ai[2] == 1 ? 0.75f : 0.5f);
                mainColor = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
                if (Projectile.ai[2] == 1)
                {
                    Projectile.localNPCHitCooldown = 7;
                    Projectile.penetrate = 4;
                }
            }
            Projectile.rotation += (Projectile.velocity.X + MathF.Abs(Projectile.velocity.Y) * Projectile.direction) * 0.01f;

            if (Projectile.ai[2] == 1)
            {
                CalamityUtils.HomeInOnNPC(Projectile, true, 900f, 25, MathHelper.Clamp(30 - time, 15, 30));
                if (Main.rand.NextBool(4) && Projectile.numHits < 1)
                {
                    Particle spark = new GlowOrbParticle(Projectile.Center + Main.rand.NextVector2Circular(7, 7) - Projectile.velocity, -Projectile.velocity * Main.rand.NextFloat(0.5f, 0.8f), false, 13, 1f, mainColor * 0.5f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            else if (time > 15)
            {
                CalamityUtils.HomeInOnNPC(Projectile, true, 700f * (Projectile.numHits > 1 ? 2 : 1), 20, 20f * (Projectile.numHits > 1 ? 5 : 1));
                if (Main.rand.NextBool(3))
                {
                    Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5) - Projectile.velocity, 66);
                    trailDust.scale = Main.rand.NextFloat(0.7f, 0.85f);
                    trailDust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.5f);
                    trailDust.color = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
                    trailDust.noGravity = true;
                }
            }

            time++;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return lightColor;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/StarofJudgement").Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Color.White;
            float drawRotation = Projectile.rotation;
            Vector2 rotationPoint = texture.Size() * 0.5f;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Lerp(mainColor, Color.White, 0.3f) * 0.4f, 1, texture, true, true);
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[2] == 0)
            {
                Projectile.timeLeft = 180;
                time = 0;
                Projectile.velocity *= 1.2f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[2] == 1)
            {
                int points = 5;
                float radians = MathHelper.TwoPi / points;
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f).RotatedByRandom(100));
                for (int k = 0; k < points; k++)
                {
                    Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f);
                    Particle spark = new PointParticle((Projectile.Center + velocity * 7.5f), velocity * 5.5f, false, 9, 1.2f, mainColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            else
            {
                for (int k = 0; k < 3; k++)
                {
                    GlowOrbParticle orb = new GlowOrbParticle(Projectile.Center, new Vector2(4, 4).RotatedByRandom(100f) * Main.rand.NextFloat(0.3f, 0.8f), false, 15, Main.rand.NextFloat(0.6f, 0.95f), mainColor);
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
        }
    }
}
