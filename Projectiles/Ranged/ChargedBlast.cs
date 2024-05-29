using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ChargedBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/LaserProj";
        public Color baseColor = Color.White;
        public bool outOfTime = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 6;
            Projectile.timeLeft = 360;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            if (baseColor == Color.White)
                baseColor = (Projectile.ai[2] == 1 ? new Color(229, 49, 39) : Color.DodgerBlue);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.timeLeft == 2)
                outOfTime = true;
        }
        public override void OnKill(int timeLeft)
        {
            if (!outOfTime)
            {
                SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
                int projectiles = 2;
                if (Projectile.owner == Main.myPlayer)
                {
                    for (int k = 0; k < projectiles; k++)
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, -Projectile.velocity.RotatedByRandom(0.9f) * Main.rand.NextFloat(0.6f, 0.7f), ModContent.ProjectileType<ChargedBlastSplit>(), (int)(Projectile.damage * (Projectile.ai[2] == 1 ? 0.3f : 0.5f)), Projectile.knockBack * 0.8f, Main.myPlayer, 0, 0, Projectile.ai[2] == 1 ? 1 : 0);
                }
                for (int k = 0; k < 3; k++)
                {
                    Particle spark2 = new LineParticle(Projectile.Center, (-Projectile.velocity * 4).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.8f, 1.2f), false, Main.rand.Next(25, 32 + 1), Main.rand.NextFloat(1.5f, 2f), baseColor);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                if (Projectile.ai[2] == 1)
                {
                    Particle spark2 = new LineParticle(Projectile.Center, (-Projectile.velocity * 5).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.8f, 1.2f), false, Main.rand.Next(35, 48 + 1), Main.rand.NextFloat(2.3f, 3f), baseColor);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (baseColor == Color.White)
                return false;
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLineBloom").Value;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], (baseColor * 0.7f) with { A = 0 }, 1, texture);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, baseColor with { A = 0 }, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 20, targetHitbox);
    }
}
