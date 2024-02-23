using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Enemy
{
    public class KelpDonut : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override string Texture => "CalamityMod/Particles/HollowCircleHardEdge";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 15;
            Projectile.timeLeft = 60;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size(), scale: Projectile.scale);

        public override void AI()
        {
            //The ring expanding slowing while increasing opacity until it disappears at max size 
            if (Projectile.ai[0] == 0f)
            {
                Projectile.scale = 0.5f;
                Projectile.ai[0]++;
            }

            if (Projectile.velocity.Length() < 0.08f)
                Projectile.alpha += 15;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 ringScale = Projectile.Size / tex.Size() * Projectile.scale;
            Color ringColor = Color.Blue;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            for (int i = 0; i < 5; i++)
            {
                if (i <= Projectile.oldPos.Length)
                Main.EntitySpriteDraw(tex, Projectile.oldPos[i] + new Vector2(Projectile.width / 2, Projectile.height / 2) - Main.screenPosition, null, ringColor, Projectile.rotation, tex.Size() / 2f, ringScale, SpriteEffects.None);
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            float totalDusts = 18f;
            for (float i = 0f; i < totalDusts; i++)
            {
                Vector2 ringSpeed = new Vector2((float)Math.Cos(i / totalDusts * MathHelper.TwoPi), (float)Math.Sin(i / totalDusts * MathHelper.TwoPi) * 0.5f).RotatedBy(Projectile.rotation) * 4f * Projectile.scale;
                Dust droplets = Dust.NewDustPerfect(Projectile.Center, 211, ringSpeed, 100);
                droplets.noGravity = true;
            }
        }
    }
}
