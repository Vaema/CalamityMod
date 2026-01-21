using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class FancyStars : Particle
    {
        public override string Texture => "CalamityMod/Particles/FancyStars";

        public override bool UseCustomDraw => true;

        public override bool SetLifetime => true;

        public override bool UseAdditiveBlend => true;

        private readonly float _rotationSpeed;
        private readonly int _variant;

        public FancyStars(
            Vector2 position ,
            float rotation ,
            float scale ,
            Vector2 velocity ,
            float rotationSpeed ,
            int lifeTime ,
            Color color)
        {
            Position = position;
            Rotation = rotation;
            Scale = Math.Max(0f , scale);
            Velocity = velocity;
            _rotationSpeed = rotationSpeed;
            Lifetime = lifeTime;
            Color = color;
            _variant = Main.rand.Next(5);
        }

        public override void Update()
        {
            float interpolator = MathF.Pow(Utils.GetLerpValue(0f, 60f, Lifetime), 0.56f);
            Velocity *= interpolator;
            Rotation += _rotationSpeed * interpolator;
            Scale -= 0.01f * interpolator;
            Scale = Math.Max(0f , Scale);
            Color *= interpolator;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle starVariant = texture.Frame(horizontalFrames: 5, verticalFrames: 1, frameX: _variant);
            spriteBatch.Draw(texture , Position - Main.screenPosition , starVariant , Color , Rotation , starVariant.Size() * 0.5f , Scale , SpriteEffects.None , layerDepth: 0);
        }
    }
}
