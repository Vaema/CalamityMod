using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class RainbowGlowSparkParticle : Particle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public bool QuickShrink;
        public bool Glowing;
        public float ShrinkSpeed = 1;
        public Vector2 Squash = new Vector2(0.5f, 1.6f);
        public float HueShift;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;


        public override string Texture => "CalamityMod/Particles/GlowSpark";

        public RainbowGlowSparkParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color initialColor, Vector2 squash, bool quickShrink = false, bool glow = true, float shrinkSpeed = 1, float hueShift = 0)
        {
            Position = relativePosition;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Scale = scale;
            Lifetime = lifetime;
            Color = InitialColor = initialColor;
            Squash = squash;
            QuickShrink = quickShrink;
            Glowing = glow;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            ShrinkSpeed = shrinkSpeed;
            HueShift = hueShift;
        }

        public override void Update()
        {
            Scale *= 0.95f;
            Velocity *= 0.95f;
            if (QuickShrink)
            {
                if (ShrinkSpeed == 1)
                {
                    Squash.X *= 0.8f;
                    Squash.Y *= 1.2f;
                }
                else
                {
                    Squash.X *= (1 - 0.2f * ShrinkSpeed);
                    Squash.Y *= (1 + 0.2f * ShrinkSpeed);
                }
            }
            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }

            Color = Main.hslToRgb(Main.rgbToHsl(Color).X + HueShift, Main.rgbToHsl(Color).Y, Main.rgbToHsl(Color).Z);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = Squash * Scale;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float scaleMult = 1;
            if (Main.zenithWorld)
            {
                DateTime day = DateTime.Now;
                if (day.DayOfWeek == DayOfWeek.Tuesday)
                {
                    Texture2D joke = ModContent.Request<Texture2D>("CalamityMod/Particles/MammothParticle").Value;
                    scaleMult = (MathHelper.Lerp(texture.Size().X / joke.Size().X, texture.Size().Y / joke.Size().Y, 0.5f));
                    texture = joke;
                }
            }

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, Rotation, texture.Size() * 0.5f, scale * scaleMult, 0, 0f);
            if (Glowing)
                spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.Lerp(Color.White, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D)), Rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f) * scaleMult, 0, 0f);
        }
    }
}
