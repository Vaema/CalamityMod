using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class WaterFoamParticle : Particle
    {
        public Color InitialColor;
        public bool HasCreatedFoam = false;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public override string Texture => "CalamityMod/Particles/WaterFoam";

        public WaterFoamParticle(Vector2 relativePosition, Vector2 velocity, int lifetime, float scale, Color color)
        {
            Position = relativePosition;
            Velocity = velocity;
            Scale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 1.7D));

            if (Collision.WetCollision(Position, 1, 1))
            {
                if (!HasCreatedFoam)
                {
                    Vector2 foamVelocity = Main.rand.NextVector2Circular(2f, 0.3f);
                    if (foamVelocity.Y < -0.2f)
                        foamVelocity.Y = -0.2f;

                    // Spawning the particle directly will result in the particle list collection being modified prematurely within the update loop.
                    // Doing it the next frame results in a single-frame buffer, but that shouldn't really be a problem and it ensures that the loop is
                    // not interrupted in unanticipated ways.
                    GeneralParticleHandler.QueueParticleForNextFrame(new MediumMistParticle(Position, foamVelocity, Color.LightCyan, Color.White, 0.2f, 255f, 0.03f));
                    HasCreatedFoam = true;
                }

                Velocity.Y *= 0.7f;
                Time += 4;
            }
            else
                Velocity.Y = MathHelper.Clamp(Velocity.Y + 0.12f, -8f, 12f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float brightness = (float)Math.Pow(Lighting.Brightness((int)(Position.X / 16f), (int)(Position.Y / 16f)), 0.15);
            Texture2D texture = ModContent.GetTexture(Texture);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color * brightness, Rotation, texture.Size() * 0.5f, Scale, 0, 0f);
        }
    }
}
