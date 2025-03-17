using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class GlowSquareParticle : Particle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public float fadeOut = 1;
        public bool glowCenter;
        public float RotationUse;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public override string Texture => "CalamityMod/Particles/GlowSquareParticle";

        public GlowSquareParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, bool GlowCenter = true, float rotation = 0)
        {
            Position = relativePosition;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Scale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            glowCenter = GlowCenter;
            RotationUse = rotation;
        }
        public override void Update()
        {
            //if (RotationUse != 0 && fadeOut == 1)
                //Rotation = Main.rand.NextFloat(-100, 100);
            fadeOut -= 0.1f;
            Scale *= 0.93f;
            Color = Color.Lerp(InitialColor, InitialColor * 0.2f, (float)Math.Pow(LifetimeCompletion, 3D));
            Velocity *= 0.95f;
            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }
            if (RotationUse != 0)
            {
                Rotation += RotationUse;
                RotationUse *= 0.992f;
            }
            else
                Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(1f, 1f) * Scale;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
            if (glowCenter)
                spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.White * fadeOut * 0.7f, Rotation, texture.Size() * 0.5f, scale * new Vector2(0.9f, 0.9f), 0, 0f);
        }
    }
}
