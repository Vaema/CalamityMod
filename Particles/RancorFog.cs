using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod.Particles;

public class RancorFog : Particle
{
    private float Opacity = 0f;
    public override bool UseAdditiveBlend => true;
    public override bool SetLifetime => true;
    public override bool UseCustomDraw => true;
    public RancorFog(Vector2 position, Vector2 velocity, int lifetime, float scale, float rotation)
    {
        Position = position;
        Velocity = velocity;
        Lifetime = lifetime;
        Scale = scale;
        Rotation = rotation;
    }

    public override void Update()
    {
        Rotation += Velocity.X * 0.004f;
        Velocity *= 0.985f;
        Opacity = Utils.GetLerpValue(0f, 15f, Time, true) * Utils.GetLerpValue(Lifetime, Lifetime - 90f, Time, true);
    }

    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D texture = GeneralParticleHandler.GetTexture(Type);
        float opacity = Opacity * 0.5f;
        Color drawColor = new Color(236, 0, 68) * opacity;
        Main.EntitySpriteDraw(texture, Position - Main.screenPosition, null, drawColor, Rotation, texture.Size() * 0.5f, Scale, SpriteEffects.None);
    }
}
