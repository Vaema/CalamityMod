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
                Projectile.scale = 0.5f;
                mainColor = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
            }
            Projectile.rotation += (Projectile.velocity.X + MathF.Abs(Projectile.velocity.Y) * Projectile.direction) * 0.01f;

            if (time > 15)
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
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Lerp(mainColor, Color.White, 0.5f) * 0.3f, 1, texture, true, true);
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            time = 0;
            Projectile.velocity *= 1.2f;
            Projectile.timeLeft = 180;
        }
        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
                GlowOrbParticle orb = new GlowOrbParticle(Projectile.Center, new Vector2(4, 4).RotatedByRandom(100f) * Main.rand.NextFloat(0.3f, 0.8f), false, 15, Main.rand.NextFloat(0.6f, 0.95f), mainColor);
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }
    }
}
