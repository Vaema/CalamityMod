using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.AstralDesert
{
    public class AstralCactus : GlowMaskCactus
    {
        public override void SetStaticDefaults()
        {
            // Grows on astral sand
            GrowsOnTileId = new int[1] { ModContent.TileType<AstralSand>() };
        }

        //Idk what to make with the glowmask
        public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralCactus");
        public override Asset<Texture2D> GetGlowTexture() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralCactusGlow");

        // TODO: Fruit Texture for Astral Cactus (it's for Pink Prickly Pear)
        public override Asset<Texture2D> GetFruitTexture() => null;
        public override Asset<Texture2D> GetFruitGlowTexture() => null;

        public override Color GetGlowColor(int i, int j)
        {
            return Color.White;
        }
    }
}
