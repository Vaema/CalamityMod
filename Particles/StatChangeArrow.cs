using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles;

public class StatChangeArrow : Particle
{
    public override string Texture => "CalamityMod/Particles/StatChangeArrow";
    public override bool UseAdditiveBlend => true;
    public override bool UseCustomDraw => true;
    public override bool SetLifetime => true;

    public bool AffectedByGravity = false;

    public Color StartColor;
    public Color EndColor;

    public StatChangeArrow(Vector2 position, Vector2 velocity, float rotation, Color startColor, Color endColor, float scale, int lifetime)
    {
        Position = position;
        Velocity = velocity;
        Color = startColor;
        StartColor = startColor;
        EndColor = endColor;
        Scale = scale;
        Lifetime = lifetime;
        Rotation = rotation;
    }

    public override void Update()
    {
        Color = Color.Lerp(StartColor, EndColor, LifetimeCompletion);
        Lighting.AddLight(Position, Color.ToVector3() * 0.2f);
        if (Velocity.Length() < 12f && AffectedByGravity)
        {
            Velocity.X *= 0.94f;
            Velocity.Y += 0.25f;
        }
    }

    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 DrawSize = new Vector2(Scale);
        Vector2 Origin = new Vector2(texture.Width, texture.Height) /2f;

        spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, Rotation, Origin, DrawSize, SpriteEffects.None, 0);
    }
}
