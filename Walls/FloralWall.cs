using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class FloralWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(23, 39, 48));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Chlorophyte, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.RedTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => spriteBatch.DrawMultiVariantWall(Type, i, j, CreatePattern(i, j));

        private int[] CreatePattern(int i, int j)
        {
            int[] sheetOffset = new int[2] { i % 2, j % 3 };
            int xPos = i % 2;
            int yPos = j % 3;
            switch (xPos)
            {
                case 0:
                    switch (yPos)
                    {
                        case 0:
                            sheetOffset = new int[2] { 0, 0 };
                            break;
                        case 1:
                            sheetOffset = new int[2] { 0, 1 };
                            break;
                        case 2:
                            sheetOffset = new int[2] { 0, 2 };
                            break;
                    }
                    break;
                case 1:
                    switch (yPos)
                    {
                        case 0:
                            sheetOffset = new int[2] { 1, 0 };
                            break;
                        case 1:
                            sheetOffset = new int[2] { 1, 1 };
                            break;
                        case 2:
                            sheetOffset = new int[2] { 1, 2 };
                            break;
                    }
                    break;
            }
            sheetOffset[0] = sheetOffset[0] * 468;
            sheetOffset[1] = sheetOffset[1] * 180;
            return sheetOffset;
        }
    }
}
