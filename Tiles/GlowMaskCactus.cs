using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Tiles;

public abstract class GlowMaskCactus : ModCactus
{
    public abstract Asset<Texture2D> GetFruitGlowTexture();
    public abstract Asset<Texture2D> GetGlowTexture();

    public abstract Color GetGlowColor(int i, int j);
}
