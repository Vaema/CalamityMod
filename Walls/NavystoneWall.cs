using Microsoft.Xna.Framework;
using Terraria.ModLoader;
namespace CalamityMod.Walls
{
    public class NavystoneWall : WallVisibleThroughWater
    {
        public override void SetStaticDefaults()
        {
            DustType = 96;
            AddEntries(new Color(16, 45, 48));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
