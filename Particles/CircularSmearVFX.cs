using CalamityMod.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using ReLogic.Content;

namespace CalamityMod.Particles;

public class CircularSmearVFX : Particle //Also check out Split mod!
{
    public override string Texture => "CalamityMod/Particles/CircularSmear";
    public override bool UseAdditiveBlend => true;

    public override bool SetLifetime => true;
    public override bool UseCustomDraw => true;

    public Asset<Texture2D> LoadedAsset;

    public CircularSmearVFX(Vector2 position, Color color, float rotation, float scale, Asset<Texture2D>? texture = null)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Color = color;
        Scale = scale;
        Rotation = rotation;
        Lifetime = 2;

        LoadedAsset = texture;
        if (LoadedAsset is null)
            LoadedAsset = ExtraTextureRefs.CircularSmear;
    }

    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(LoadedAsset.Value, Position - Main.screenPosition, null, Color, Rotation, LoadedAsset.Value.Size() * 0.5f, Scale, SpriteEffects.None, 0);
    }
}
