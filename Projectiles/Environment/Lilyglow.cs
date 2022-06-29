using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Environment
{
    public class Lilyglow : ModProjectile
    {
        public ref float Direction => ref Projectile.ai[0];

        public override string Texture => "CalamityMod/ExtraTextures/FusableParticleBase";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Lilyglow");

        public override void SetDefaults()
        {
            Projectile.width = 360;
            Projectile.height = 360;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.scale = Main.rand?.NextFloat(0.02f, 0.035f) ?? 0.03f;
            Projectile.Size /= Projectile.scale;
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(0f, 120f, Projectile.timeLeft, true) * Utils.GetLerpValue(600f, 540f, Projectile.timeLeft, true);
            if (Projectile.timeLeft < 60f)
                Projectile.scale *= 0.98f;

            // Oh? You're inside of blocks? DIEDIEDIEDIEDIEDIE
            if (Collision.SolidCollision(Projectile.Center, 1, 1))
                Projectile.timeLeft -= 8;

            Projectile.Size = Vector2.One * 360f;
            Projectile.hide = Projectile.Opacity < 0.15f;

            // Rotate around.
            Projectile.velocity = Projectile.velocity.RotatedBy((float)Math.Cos(Projectile.timeLeft / 33f + Projectile.identity) * 0.0283f);

            Point left = (Projectile.Center - Vector2.UnitX * 24f).ToTileCoordinates();
            Point right = (Projectile.Center + Vector2.UnitX * 24f).ToTileCoordinates();
            Point top = (Projectile.Center - Vector2.UnitY * 24f).ToTileCoordinates();
            Point bottom = (Projectile.Center + Vector2.UnitY * 24f).ToTileCoordinates();

            DelegateMethods.v3_1 = Projectile.GetAlpha(Color.White).ToVector3();
            Utils.PlotLine(left, right, DelegateMethods.CastLight);
            Utils.PlotLine(top, bottom, DelegateMethods.CastLight);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float fadeToPink = (float)Math.Pow(Projectile.identity % 8f / 8f, 0.93);
            Color c = Color.Lerp(Color.Yellow, new Color(255, 110, 105), fadeToPink) * Projectile.Opacity;
            c.A = (byte)Projectile.alpha;
            return c;
        }
    }
}
