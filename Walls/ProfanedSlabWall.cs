using CalamityMod.Dusts.Furniture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class ProfanedSlabWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(59, 27, 23));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, ModContent.DustType<ProfanedTileRock>(), 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => spriteBatch.DrawMultiVariantWall(Type, i, j, CreatePattern(i, j));

        private int[] CreatePattern(int i, int j)
        {
            int[] sheetOffset = new int[2] { i % 4, j % 4 };
            sheetOffset[0] = sheetOffset[0] * 468;
            sheetOffset[1] = sheetOffset[1] * 180;
            return sheetOffset;
        }
    }
}
