using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using CalamityMod.Particles;
using Terraria.ModLoader;
using System;

namespace CalamityMod.Particles
{
    public class RoundedStarParticle : Particle
    {
        public override string Texture => "CalamityMod/Particles/RoundedStar";

        public float RotationSpeed;
        public float Deceleration;

        public bool UseSpiralAI;
        private int OwnerIndex;
        private float InitialRadius;
        private float OrbitalAngle;

        public override bool UseCustomDraw => true;
        public RoundedStarParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime, float rotationSpeed, float deceleration, bool useSpiralAI, Vector2 spiralTarget, int ownerIndex)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            RotationSpeed = rotationSpeed;
            Deceleration = deceleration;
            UseSpiralAI = useSpiralAI;
            OwnerIndex = ownerIndex;

            if (UseSpiralAI)
            {
                // Store the initial radius and angle relative to the player
                Vector2 directionToPlayer = Position - Main.player[OwnerIndex].Center;
                InitialRadius = directionToPlayer.Length();
                OrbitalAngle = directionToPlayer.ToRotation();
            }
        }

        public override void Update()
        {
            if (UseSpiralAI)
            {
                OrbitalAngle += RotationSpeed;

                float fadeScale = 1f - (Time / (float)Lifetime);
                float currentRadius = InitialRadius * fadeScale;

                Vector2 playerCenter = Main.player[OwnerIndex].Center;

                Vector2 newRelativePosition = OrbitalAngle.ToRotationVector2() * currentRadius;

                // Update the particle's position.
                Position = playerCenter + newRelativePosition;
            }
            else
            {
                // If not using the spiral AI, apply deceleration
                Velocity *= Deceleration;
                Position += Velocity;
            }

            Rotation += RotationSpeed;
            Time++;
            if (Time >= Lifetime)
                Kill();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float fade = 1f - (Time / (float)Lifetime);
            float scaleFade = Scale * fade;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * fade, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
}
