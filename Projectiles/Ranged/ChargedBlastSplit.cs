using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ChargedBlastSplit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/LaserProj";
        public Color baseColor = Color.White;
        public int tileHits = 4;
        public Vector2 startVel;
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
            Projectile.penetrate = 3;
            Projectile.extraUpdates = 5;
            Projectile.timeLeft = 240;
            Projectile.scale = 0.7f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            bool InfinityShot = Projectile.ai[2] == 1;
            if (baseColor == Color.White)
            {
                baseColor = (InfinityShot ? new Color(229, 49, 39) : Color.DodgerBlue);
                startVel = Projectile.velocity;
                if (InfinityShot)
                    Projectile.timeLeft = 340;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft < 290 && InfinityShot)
            {
                Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, -startVel.X, 0.02f);
                Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, -startVel.Y, 0.02f);
            }
            if (InfinityShot)
            {
                Projectile.tileCollide = false;
                Projectile.penetrate = -1;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 3; k++)
            {
                Particle spark2 = new LineParticle(Projectile.Center, (Projectile.velocity * 5).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.8f, 1.2f), false, Main.rand.Next(8, 11 + 1), Main.rand.NextFloat(0.5f, 1f), baseColor);
                GeneralParticleHandler.SpawnParticle(spark2);
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
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 10, targetHitbox);

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            tileHits--;
            if (tileHits <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;
            }

            return false;
        }
    }
}
