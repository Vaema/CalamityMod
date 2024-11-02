using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class VelChangingSpark : Particle
    {
        public Color InitialColor;
        public bool FadeIn = false;
        public float FadeInScale = 0f;
        public bool GlowCenter = false;
        public string NewTexture;
        public float ExtraRotation;
        public Vector2 Stretch = new Vector2(0.5f, 1.6f);
        public float ShrinkSpeed = 0;
        public Vector2 EndVelocity;
        public float LerpRate = 0.1f;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public bool AltVisual = true;
        public override bool UseAdditiveBlend => AltVisual;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public VelChangingSpark(Vector2 relativePosition, Vector2 startVelocity, Vector2 endVelocity, string texture, int lifetime, float scale, Color color, Vector2 stretch, bool useAddativeBlend = true, bool glowCenter = false, float extraRotation = 0, bool fadeIn = false, float shrinkSpeed = 0, float lerpRate = 0.1f)
        {
            Position = relativePosition;
            Velocity = startVelocity;
            EndVelocity = endVelocity;
            NewTexture = texture;
            ExtraRotation = extraRotation;
            AffectedByLight = false;
            Scale = scale;
            Stretch = stretch;
            FadeInScale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            ShrinkSpeed = shrinkSpeed;

            AltVisual = useAddativeBlend;
            GlowCenter = glowCenter;

            FadeIn = fadeIn;

            if (FadeIn)
                Scale = 0f;
            LerpRate = lerpRate;
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

            Velocity = new Vector2(MathHelper.Lerp(Velocity.X, EndVelocity.X, LerpRate), MathHelper.Lerp(Velocity.Y, EndVelocity.Y, LerpRate));

            Rotation = Velocity.ToRotation() + MathHelper.PiOver2 + ExtraRotation;

            Stretch.X *= (1 - 0.2f * ShrinkSpeed);
            Stretch.Y *= (1 + 0.2f * ShrinkSpeed);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = Stretch * Scale;
            Texture2D texture = ModContent.Request<Texture2D>(NewTexture).Value;

            Color col = Color;

            if (AffectedByLight)
            {
                col = Lighting.GetColor((Position / 16).ToPoint()).MultiplyRGB(Color);
            }

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, col, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
            if (GlowCenter)
                spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.Lerp(col, Color.White, 0.8f), Rotation, texture.Size() * 0.5f, scale * 0.8f, 0, 0f);
        }
    }
}
