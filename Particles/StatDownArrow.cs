using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class StatDownArrow : Particle
    {
        public override string Texture => "CalamityMod/Particles/StatDownArrow";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        public Color StartColor;
        public Color EndColor;

        public StatDownArrow(Vector2 position, Vector2 velocity, Color startColor, Color endColor, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = startColor;
            StartColor = startColor;
            EndColor = endColor;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = 0;
        }

        public override void Update()
        {
            Color = Color.Lerp(StartColor, EndColor, LifetimeCompletion);
            Lighting.AddLight(Position, Color.ToVector3() * 0.2f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 DrawSize = new Vector2(Scale);
            Vector2 Origin = new Vector2(texture.Width, texture.Height);

            Vector2 PositionAdjust = new Vector2(-5, -11);

            spriteBatch.Draw(texture, Position - Main.screenPosition - PositionAdjust, null, Color, Rotation, Origin, DrawSize, SpriteEffects.None, 0);
        }
    }
}
