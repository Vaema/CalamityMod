using CalamityMod.Dusts.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class RunestoneWall : WallVisibleThroughWater
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddEntries(new Color(122, 59, 48));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
