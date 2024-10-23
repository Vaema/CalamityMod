using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class VoidstoneSlabWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(19, 17, 22));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => spriteBatch.DrawMultiVariantWall(Type, i, j, CreatePattern(i, j));

        private int[] CreatePattern(int i, int j)
        {
            int xPos = i % 3;
            int yPos = j % 3;
            int[] sheetOffset = new int[2] { 0, (xPos + yPos * 3) % 5 };
            sheetOffset[1] = sheetOffset[1] * 180;
            return sheetOffset;
        }
    }
}
