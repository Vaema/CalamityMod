using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class PearlParticle : Particle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public float ShrinkSpeed;
        public float RotationSpeed;
        public bool HitTiles;
        public bool hasTileHit;
        public float pVelX;
        public float pVelY;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => false;

        public override string Texture => "CalamityMod/Particles/PearlParticle";

        public PearlParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, float shrinkSpeed = 0.95f, float rotationSpeed = 0, bool hitTiles = false)
        {
            Position = relativePosition;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Scale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            ShrinkSpeed = shrinkSpeed;
            RotationSpeed = rotationSpeed;
            HitTiles = hitTiles;
        }

        public override void Update()
        {
            if (HitTiles)
            {
                if (hasTileHit)
                {
                    if (Velocity.X != pVelX)
                    {
                        Velocity.X = -pVelX;
                    }
                    if (Velocity.Y != pVelY)
                    {
                        Velocity.Y = -pVelY;
                    }
                    HitTiles = false;
                }
                if (Collision.SolidCollision(Position, (int)(7f * Scale), (int)(7f * Scale)))
                {
                    hasTileHit = true;
                    pVelX = Velocity.X;
                    pVelY = Velocity.Y;
                }
            }
            Scale *= ShrinkSpeed;
            RotationSpeed *= ShrinkSpeed;
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
            Velocity *= 0.95f;
            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }
            Rotation += RotationSpeed;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(1f, 1f) * Scale;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityMod/Particles/PearlParticleGlow").Value;

            spriteBatch.Draw(texture2, Position - Main.screenPosition, null, Color, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.Lerp(Color.White, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D)), Rotation, texture.Size() * 0.5f, scale, 0, 0f);
        }
    }
}
