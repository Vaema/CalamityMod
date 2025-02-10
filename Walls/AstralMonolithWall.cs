using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class AstralMonolithWall : MultiVariantModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            AddMapEntry(new Color(5, 5, 5));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, ModContent.DustType<AstralBasic>(), 0f, 0f, 1, new Color(255, 255, 255), 1f);
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
