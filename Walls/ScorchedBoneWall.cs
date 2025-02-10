using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.Walls
{
    public class ScorchedBoneWall : MultiVariantModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(49, 33, 35));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = (i % 3) * 468;
            frameYOffset = (j % 3) * 180;
        }
    }
}
