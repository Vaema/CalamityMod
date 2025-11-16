using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace CalamityMod.Systems.Graphic.TempParticleSystem
{
    public class TempParticle
    {
        public Asset<Texture2D> StoredTexture;

        public Vector2 Position;

        public Vector2 StoredPosition;

        public Vector2 Velocity;

        public Vector2 Scale;

        public Color DrawColor;

        public float Rotation;

        public float Opacity;

        public float ParallaxStrength;

        public int Lifetime;

        public int Time;

        public int FrameX;

        public int FrameY;

        public int MaxVerticalFrames;

        public int MaxHorizontalFrames;

        public float[] ExtraData;

        /// <summary>
        /// An 0-1 interpolant representing how close this particle is from its <see cref="Lifetime"/>.
        /// </summary>
        public float LifetimeCompletionRatio => MathHelper.Clamp(Time / (float)Lifetime, 0f, 1f);

        public void SetBasicParticleData(Asset<Texture2D> storedTexture, Vector2 position, Vector2 storedPosition, Vector2 velocity, Vector2 scale, Color drawColor, float rotation, float opacity, float parallaxStrength, int lifetime, int frameX, int frameY, int maxHorizontalFrames, int maxVerticalFrames, float extraData0, float extraData1, float extraData2, float extraData3)
        {
            Time = 0;
            ExtraData = new float[4];

            StoredTexture = storedTexture;
            Position = position;
            StoredPosition = storedPosition;
            Velocity = velocity;
            Scale = scale;
            DrawColor = drawColor;
            Rotation = rotation;
            Opacity = opacity;
            ParallaxStrength = parallaxStrength;
            Lifetime = lifetime;
            FrameX = frameX;
            FrameY = frameY;
            MaxHorizontalFrames = maxHorizontalFrames;
            MaxVerticalFrames = maxVerticalFrames;
            ExtraData[0] = extraData0;
            ExtraData[1] = extraData1;
            ExtraData[2] = extraData2;
            ExtraData[3] = extraData3;      
        }
    }
}
