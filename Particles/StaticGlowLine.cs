using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class StaticGlowLine : Particle
    {
        internal Asset<Texture2D> texAsset;
        // 1660 / 2048 is the size of the actual spark on the asset, roughly.
        private const float approximateSparkHeight = 1660f;

        internal Vector2 destination;
        internal Color InitialColor;
        internal bool usesGlow;
        internal float xScale = 1f;
        internal float xShrink = 0.99f;
        private float currentYScale = 1f;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public override string Texture => "CalamityMod/Particles/GlowSpark";

        public StaticGlowLine(Vector2 start, Vector2 dest, Vector2 velocity, int lifetime, float xScale, float xShrink, Color color, bool glow = true)
        {
            Position = start;
            destination = dest;
            Velocity = velocity;
            Lifetime = lifetime;

            Scale = 1f; // unused for this particle
            this.xScale = xScale;
            this.xShrink = xShrink;

            Color = InitialColor = color;
            usesGlow = glow;

            Rotation = (dest - start).ToRotation() + MathHelper.PiOver2;
            texAsset = ModContent.Request<Texture2D>(Texture);
        }

        public override void Update()
        {
            // Color lerping to transparent matches the behavior of Glow Sparks.
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));

            // Line shrinks horizontally continuously while existing.
            xScale *= xShrink;

            // Recalculate current Y scale every frame.
            Vector2 delta = destination - Position;
            currentYScale = delta.Length() / approximateSparkHeight;

            // The Glow Spark asset is vertically oriented.
            // However, this particle's velocity and orientation are completely independent of one another.
            Rotation = (destination - Position).ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = texAsset.Value;
            Vector2 texSize = texture.Size() * 0.5f;
            Vector2 scaleThisFrame = new(xScale, currentYScale);

            // Adjust the origin towards the target to compensate for the empty edges of the Glow Spark texture.
            // Add a little extra so the edges of the beam go through the source and destination slightly.
            float adjustmentDistance = 2f * currentYScale * (texture.Height - approximateSparkHeight);
            Vector2 toDest = (destination - Position).SafeNormalize(-Vector2.UnitY);
            Vector2 drawPos = Position + toDest * adjustmentDistance - Main.screenPosition;

            spriteBatch.Draw(texture, drawPos, null, Color, Rotation, texSize, scaleThisFrame, 0, 0f);

            if (usesGlow)
            {
                float glowInteriorWidth = 0.45f;
                Vector2 glowScale = new(glowInteriorWidth, 1f);
                spriteBatch.Draw(texture, drawPos, null, Color.White, Rotation, texSize, scaleThisFrame * glowScale, 0, 0f);
            }
        }
    }
}
