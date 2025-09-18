using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Walls
{
    public class LargeBasaltWall : MultiVariantModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(65, 64, 68));
        }
        public override void RandomUpdate(int i, int j)
        {
            Tile t = Main.tile[i, j];
            Tile tUP = Main.tile[i, j - 1];
            Tile tDOWN = Main.tile[i, j + 1];
            Tile tLEFT = Main.tile[i - 1, j];
            Tile tRIGHT = Main.tile[i + 1, j];
            Tile tUPLEFT = Main.tile[i -1, j - 1];
            Tile tDOWNLEFT = Main.tile[i - 1, j + 1];
            Tile tUPRIGHT = Main.tile[i + 1, j -1];
            Tile tDOWNRIGHT = Main.tile[i + 1, j + 1];
            if (t.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                t.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tUP.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tUP.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tDOWN.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tDOWN.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tLEFT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tLEFT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tRIGHT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tRIGHT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }

            if (tUPLEFT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tUPLEFT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tDOWNLEFT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tDOWNLEFT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tUPRIGHT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tUPRIGHT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tDOWNRIGHT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tDOWNRIGHT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            Dust dust;
            dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 16, DustID.Smoke, 0f, -1.9069767f, 195, new Color(255, 255, 255), 1f)];
            dust.noGravity = false;
            dust.fadeIn = 1.4209302f;
        }
        public override void PlaceInWorld(int i, int j, Item item)
        {
            Tile t = Main.tile[i, j];
            if (t.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                t.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Ash, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = (i % 4) * 468;
            frameYOffset = (j % 4) * 180;
        }
    }
}
