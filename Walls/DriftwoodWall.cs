using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class DriftwoodWall : WallVisibleThroughWater
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddEntries(new Color(69, 56, 58));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
