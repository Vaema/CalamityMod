using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class LimestoneWall : WallVisibleThroughWater
    {
        public override void SetStaticDefaults()
        {
            DustType = 22;
            AddEntries(new Color(147, 92, 63));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
