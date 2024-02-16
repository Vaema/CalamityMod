using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class NanoParticle : Particle
    {
        public override string Texture => "CalamityMod/Particles/NanoParticleSmall";
        public bool UseAltVisual = true;
        public override bool UseAdditiveBlend => UseAltVisual;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        private float Spin;
        private float opacity;
        private bool Big;
        private bool EmitsLight;
        private Vector2 Gravity;
        private int Time = 0;

        public NanoParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime, bool bigSize = false, bool emitsLight = false, bool AddativeBlend = true, Vector2? gravity = null)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifeTime;
            Rotation = 0;
            Big = bigSize;
            EmitsLight = emitsLight;
            UseAltVisual = AddativeBlend;
            Gravity = (Vector2)(gravity == null ? Vector2.Zero : gravity);
            Variant = Main.rand.Next(3);
        }

        public override void Update()
        {
            Time++;
            Velocity += Gravity;
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);

            if (EmitsLight)
                Lighting.AddLight(Position, opacity * Color.R / 255f, opacity * Color.G / 255f, opacity * Color.B / 255f);

            Velocity *= 0.95f;
            Scale *= 0.98f;
            if (Time % 3 == 0)
            {
                Position += Main.rand.NextVector2Circular(13, 13);
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D nanoTexture = Big ? ModContent.Request<Texture2D>("CalamityMod/Particles/NanoParticleBig").Value : ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = new Rectangle(0, (Big ? 8 : 6), (Big ? 8 : 6), (Big ? 8 : 6));
            spriteBatch.Draw(nanoTexture, Position - Main.screenPosition, frame, Color * opacity, 0, frame.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
}
