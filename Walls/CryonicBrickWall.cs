using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Walls
{
    public class CryonicBrickWall : MultiVariantModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(72, 75, 122));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.BubbleBurst_Blue, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int xPos = i % 2;
            int yPos = j % 4;
            switch (xPos)
            {
                case 0:
                    switch (yPos)
                    {
                        case 0:
                            frameXOffset = 0;
                            frameYOffset = 0;
                            break;
                        case 1:
                            frameXOffset = 0;
                            frameYOffset = 1;
                            break;
                        case 2:
                            frameXOffset = 1;
                            frameYOffset = 0;
                            break;
                        case 3:
                            frameXOffset = 1;
                            frameYOffset = 1;
                            break;
                    }
                    break;
                case 1:
                    switch (yPos)
                    {
                        case 0:
                            frameXOffset = 1;
                            frameYOffset = 0;
                            break;
                        case 1:
                            frameXOffset = 1;
                            frameYOffset = 1;
                            break;
                        case 2:
                            frameXOffset = 0;
                            frameYOffset = 0;
                            break;
                        case 3:
                            frameXOffset = 0;
                            frameYOffset = 1;
                            break;
                    }
                    break;
            }

            frameXOffset *= 468;
            frameYOffset *= 180;
        }
    }
}
