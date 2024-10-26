using CalamityMod.Dusts.Furniture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class ScorchedBoneWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(49, 33, 35));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => spriteBatch.DrawMultiVariantWall(Type, i, j, CreatePattern(i, j));

        private int[] CreatePattern(int i, int j)
        {
            int[] sheetOffset = new int[2] { i % 3, j % 3 };
            sheetOffset[0] = sheetOffset[0] * 468;
            sheetOffset[1] = sheetOffset[1] * 180;
            return sheetOffset;
        }
    }
}
