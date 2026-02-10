using System;
using System.Collections.Generic;
using CalamityMod.Effects;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NanoblackLightspeedCarveSlashVisual : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int VisualLifetime = 90;
        private const int GrowTime = 16;
        private const int FadeInTime = 10;
        private const int FadeOutTime = 14;
        private const int SlashDelay = 6;
        private const float RangeMultiplier = 2.2f;
        private const float MinWidthStandard = 14f;
        private const float MaxWidthStandard = 46f;
        private const float MinWidthPerfect = 28f;
        private const float MaxWidthPerfect = 62f;

        internal bool IsPerfect => Projectile.ai[0] == 1f;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = VisualLifetime;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            int slashCount = IsPerfect ? 12 : 8;
            if (slashCount <= 0)
                return false;

            float age = VisualLifetime - Projectile.timeLeft;
            float fadeOut = Utils.GetLerpValue(0f, FadeOutTime, Projectile.timeLeft, true);
            if (fadeOut <= 0f)
                return false;

            Effect shader = CalamityShaders.NanoblackSlashShader.Value;
            shader.Parameters["uTime"]?.SetValue((float)Main.gameTimeCache.TotalGameTime.TotalSeconds);

            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);

            float baseLength = NanoblackLightspeedCarve.HitboxRadius * 0.9f * RangeMultiplier;
            float rotationOffset = Main.GlobalTimeWrappedHourly * 0.6f;
            float seed = Projectile.identity * 1.913f;

            Main.spriteBatch.End(out var ss);

            for (int i = 0; i < slashCount; i++)
            {
                float startDelay = i * SlashDelay;
                float growProgress = Utils.GetLerpValue(startDelay, startDelay + GrowTime, age, true);
                float fadeIn = Utils.GetLerpValue(startDelay, startDelay + FadeInTime, age, true);
                if (growProgress <= 0f)
                    continue;

                float angle = rotationOffset + MathHelper.TwoPi * i / slashCount + seed * 0.1f;
                float lengthScale = (0.75f + 0.2f * MathF.Sin(seed + i * 1.7f)) * growProgress;
                float offsetScale = 0.25f * MathF.Cos(seed * 1.3f + i * 2.1f);
                float widthInterpolant = 0.5f + 0.5f * MathF.Sin(seed * 0.7f + i * 2.3f);
                float minWidth = IsPerfect ? MinWidthPerfect : MinWidthStandard;
                float maxWidth = IsPerfect ? MaxWidthPerfect : MaxWidthStandard;
                float slashWidth = MathHelper.Lerp(minWidth, maxWidth, widthInterpolant) * growProgress;

                Vector2 direction = angle.ToRotationVector2();
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                Vector2 offset = perpendicular * baseLength * offsetScale;

                Vector2 start = Projectile.Center + direction * baseLength * lengthScale + offset;
                Vector2 end = Projectile.Center - direction * baseLength * lengthScale + offset;

                var path = new List<Vector3>(2)
                {
                    new Vector3(start - Main.screenPosition, 0f),
                    new Vector3(end - Main.screenPosition, 0f)
                };

                float opacity = fadeOut * fadeIn;
                PrimitiveMesh mesh = TriangleStripBuilder.BuildStrip(
                    path,
                    _ => slashWidth,
                    Color.White * opacity,
                    textured: true,
                    smoothingSegments: 2);

                SanePrimitiveRenderer.DrawMesh(
                    Matrix.Identity,
                    view,
                    projection,
                    mesh,
                    shader,
                    blendState: BlendState.Additive);
            }

            Main.spriteBatch.Begin(ss);

            return false;
        }
    }
}
