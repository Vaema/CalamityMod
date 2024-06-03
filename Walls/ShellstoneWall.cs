using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class ShellstoneWall : WallVisibleThroughWater
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = false;
            DustType = 24;

            AddEntries(new Color(74, 71, 84));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
