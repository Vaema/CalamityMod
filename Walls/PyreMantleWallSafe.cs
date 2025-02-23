using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class PyreMantleWallSafe : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }
        public override string Texture => "CalamityMod/Walls/PyreMantleWall";

        public override void SetStaticDefaults()
        {
            DustType = 33;
            Main.wallHouse[Type] = true;
            AddMapEntry(new Color(33, 30, 30));
            this.AddMapEntryWithWaterVisibility(new Color(16, 35, 82));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
