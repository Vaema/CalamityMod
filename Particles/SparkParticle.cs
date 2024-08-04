using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class SparkParticle : Particle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public bool FadeIn = false;
        public float FadeInScale = 0f;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public SparkParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, bool fadeIn = false, bool affectedByLight = false)
        {
            Position = relativePosition;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            AffectedByLight = affectedByLight;
            Scale = scale;
            FadeInScale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            FadeIn = fadeIn;

            if (FadeIn)
                Scale = 0f;
        }

        public override void Update()
        {
            if (!FadeIn)
            {
                Scale *= 0.95f;
                Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
            }
            else
            {
                if ((float)Time / (float)Lifetime < 0.5f)
                {
                    Scale = MathHelper.Lerp(Scale, FadeInScale, 0.2f);
                }
                else
                {
                    Scale = MathHelper.Lerp(Scale, FadeInScale, -0.21f);
                }
            }
            Velocity *= 0.95f;
            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(0.5f, 1.6f) * Scale;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Color col = Color;

            if (AffectedByLight)
            {
                col = Lighting.GetColor((Position / 16).ToPoint()).MultiplyRGB(Color);
            }

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, col, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, col, Rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f), 0, 0f);
        }
    }
}
