using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class VoidSparkParticle : Particle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public float Ylength = 1f;
        public float Xlength = 0.6f;
        public float ShrinkSpeed = 1;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => false;

        public override string Texture => "CalamityMod/Particles/GlowSpark2";

        public VoidSparkParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, float shrinkSpeed = 1)
        {
            Position = relativePosition;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Scale = scale * 0.357f;
            Lifetime = lifetime;
            Color = InitialColor = color;
            ShrinkSpeed = shrinkSpeed;
        }

        public override void Update()
        {
            Scale *= 0.9f;
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
            Velocity *= 0.95f;

            Ylength *= (1 + (0.25f * ShrinkSpeed));
            Xlength *= (1 - (0.3f * ShrinkSpeed));

            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(Xlength, Ylength) * Scale;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;

            spriteBatch.Draw(texture2, Position - Main.screenPosition, null, Color with { A = 0 }, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.Black, Rotation, texture.Size() * 0.5f, scale * new Vector2(0.75f, 0.9f), 0, 0f);
        }
    }
}
