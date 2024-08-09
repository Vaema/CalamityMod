using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class EmoteExpressionParticle : Particle
    {
        public override string Texture => "CalamityMod/Particles/EmoteExpressions";

        public override bool UseCustomDraw => true;

        public override bool SetLifetime => true;

        public enum EmoteType
        {
            Exclamation,
            DoubleExclamation,
            QuestionExclamation,
            Question,
            Note,
            DoubleNote,
            Smile,
            BigSmile
        }

        private Rectangle Frame;

        public EmoteExpressionParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifeTime, EmoteType emote)
        {
            Position = position;
            Scale = scale;
            Velocity = velocity;
            Color = color;
            Lifetime = lifeTime;
            Frame = ModContent.Request<Texture2D>(Texture).Value.Frame(horizontalFrames: 8, verticalFrames: 1, frameX: (int)emote);
            Rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Scale *= 0.97f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D emoteTexture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(Frame.Width / 2f, Frame.Height);
            float opacity = 1 - (float)Math.Pow(LifetimeCompletion, 4f);

            spriteBatch.Draw(emoteTexture, Position - Main.screenPosition, Frame, Color * opacity, Rotation, origin, Scale, SpriteEffects.None, 0);
        }
    }
}
