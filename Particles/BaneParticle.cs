using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class BaneParticle : Particle
    {
        public override string Texture => "CalamityMod/Particles/BaneParticle";
        public Color InitialColor;
        public Color EndColor;
        public bool AffectedByGravity;
        public bool FadeIn = false;
        public float FadeInScale = 0f;

        public string NewTexture;
        public float ActiveRotation;
        public float AddedRotation = 0;
        public float SpeedReduction = 0.95f;
        public float ExtraRotation = 0;

        public Vector2 Stretch = Vector2.One;
        public float ShrinkSpeed = 0;
        public bool AltVisual = false;
        public override bool UseAdditiveBlend => AltVisual;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public override int FrameVariants => 5;

        public BaneParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, Color colorEnd, Vector2 stretch, bool useAddativeBlend = false, float activeRotation = 0, float speedReduction = 0.95f, float extraRotation = 0, bool fadeIn = false, bool affectedByLight = true, float shrinkSpeed = 0)
        {
            Position = relativePosition;
            Velocity = velocity;
            ActiveRotation = activeRotation;
            AffectedByGravity = affectedByGravity;
            AffectedByLight = affectedByLight;
            Scale = scale;
            Stretch = stretch;
            FadeInScale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            EndColor = colorEnd;
            ShrinkSpeed = shrinkSpeed;
            SpeedReduction = speedReduction;
            ExtraRotation = extraRotation;

            AltVisual = useAddativeBlend;

            FadeIn = fadeIn;

            if (FadeIn)
                Scale = 0f;

            Variant = Main.rand.Next(FrameVariants);
        }

        public override void Update()
        {
            if (!FadeIn)
            {
                Scale *= 0.95f;
                InitialColor = Color.Lerp(InitialColor, EndColor, (float)Math.Pow(LifetimeCompletion, 3D));
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
            Velocity *= SpeedReduction;
            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }

            float velRot = ActiveRotation == 0 ? (Velocity.ToRotation() + MathHelper.PiOver2) : ActiveRotation;
            Rotation = velRot + AddedRotation;
            AddedRotation += ActiveRotation;
            ActiveRotation *= 0.975f;

            Stretch.X *= (1 - 0.2f * ShrinkSpeed);
            Stretch.Y *= (1 + 0.2f * ShrinkSpeed);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/BaneParticleGlow").Value;
            int frameWidth = 18;
            int frameHeight = 18;
            int frameSpacing = frameHeight + 2;
            Rectangle frame = new Rectangle(0, frameSpacing * Variant, frameWidth, frameHeight);
            Vector2 scale = Stretch * Scale;

            Color col = Color;

            if (AffectedByLight)
            {
                col = Lighting.GetColor((Position / 16).ToPoint()).MultiplyRGB(Color);
            }

            spriteBatch.Draw(texture, Position - Main.screenPosition, frame, col, Rotation + ExtraRotation, frame.Size() * 0.5f, scale, 0, 0f);
        }
    }
}
