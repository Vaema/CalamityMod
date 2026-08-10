using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod.Particles;

public class SeaPrismParticle : Particle
{
    public override string Texture => "CalamityMod/Particles/SeaPrisms";
    public Color InitialColor;
    public bool AffectedByGravity;
    public bool FadeIn = false;
    public float FadeInScale = 0f;

    public string NewTexture;
    public float ActiveRotation;
    public float AddedRotation = 0;
    public float SpeedReduction = 0.95f;

    public Vector2 Stretch = Vector2.One;
    public float ShrinkSpeed = 0;
    public bool AltVisual = false;
    public override bool UseAdditiveBlend => AltVisual;
    public override bool SetLifetime => true;
    public override bool UseCustomDraw => true;

    public override int FrameVariants => 3;

    public SeaPrismParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, Vector2 stretch, bool useAddativeBlend = false, float activeRotation = 0, float speedReduction = 0.95f, bool fadeIn = false, bool affectedByLight = true, float shrinkSpeed = 0)
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
        ShrinkSpeed = shrinkSpeed;
        SpeedReduction = speedReduction;

        AltVisual = useAddativeBlend;

        FadeIn = fadeIn;

        if (FadeIn)
            Scale = 0f;

        Variant = Main.rand.Next(3);
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
        Texture2D texture = GeneralParticleHandler.GetTexture(Type);
        int frameWidth = 16;
        int frameHeight = 32;
        int frameSpacing = frameWidth + 2; // If you're using this elsewhere and the sheet is vertical, be sure to swap frameWidth with frameHeight
        Rectangle frame = new Rectangle(frameSpacing * Variant, 0, frameWidth, frameHeight);
        Vector2 scale = Stretch * Scale;

        Color col = Color;

        if (AffectedByLight)
        {
            col = Lighting.GetColor((Position / 16).ToPoint()).MultiplyRGB(Color);
        }

        spriteBatch.Draw(texture, Position - Main.screenPosition, frame, col, Rotation, frame.Size() * 0.5f, scale, 0, 0f);
    }
}
