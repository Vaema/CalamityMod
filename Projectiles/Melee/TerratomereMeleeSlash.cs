using System.Collections.Generic;
using CalamityMod.Effects;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class TerratomereMeleeSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public Vector2[] ControlPoints;

        public bool Flipped => Projectile.ai[0] == 1f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 144;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Opacity = Utils.GetLerpValue(0f, 26f, Projectile.timeLeft, true);
            Projectile.velocity *= 0.91f;
            Projectile.scale *= 1.01f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (ControlPoints == null || ControlPoints.Length < 2)
                return false;

            var path = new List<Vector3>();
            for (int i = 0; i < ControlPoints.Length; i++)
            {
                Vector2 worldPos = Projectile.Center + ControlPoints[i] + ControlPoints[i].SafeNormalize(Vector2.Zero) * (Projectile.scale - 1f) * 40f;
                Vector2 screenPos = worldPos - Main.screenPosition;
                path.Add(new Vector3(screenPos, 0f));
            }

            if (path.Count < 2)
                return false;

            float width = Projectile.scale * 40f;
            Color trailColor = Color.White * Projectile.Opacity;

            PrimitiveMesh mesh = TriangleStripBuilder.BuildStrip(
                path,
                _ => width,
                trailColor,
                textured: true,
                smoothingSegments: 4);

            Effect shader = CalamityShaders.NanoblackSlashShader.Value;
            shader.Parameters["uTime"]?.SetValue((float)Main.gameTimeCache.TotalGameTime.TotalSeconds);

            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);

            Main.spriteBatch.End(out var ss);

            SanePrimitiveRenderer.DrawMesh(
                Matrix.Identity,
                view,
                projection,
                mesh,
                shader,
                blendState: BlendState.Additive);

            Main.spriteBatch.Begin(ss);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);
    }
}
