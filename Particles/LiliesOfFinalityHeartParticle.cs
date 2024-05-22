using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class LiliesOfFinalityHeartParticle : Particle
    {
        public override string Texture => "CalamityMod/Particles/LiliesOfFinalityHeartParticle";
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        public LiliesOfFinalityHeartParticle(Vector2 position, Vector2 velocity, int lifeTime, float scale = 1f)
        {
            Position = position;
            Velocity = velocity;
            Lifetime = lifeTime;
            Scale = scale;
            Rotation = velocity.ToRotation() + MathHelper.PiOver2;
            Color = Color.White;
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Scale *= 0.97f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D emoteTexture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = emoteTexture.Size() * 0.5f;
            float opacity = 1f - MathF.Pow(LifetimeCompletion, 4f);
            spriteBatch.Draw(emoteTexture, Position - Main.screenPosition, null, Color * opacity * 0.7f, Rotation, origin, Scale, SpriteEffects.None, 0);
        }
    }
}
