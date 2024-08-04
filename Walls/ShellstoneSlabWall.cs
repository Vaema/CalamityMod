using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class ShellstoneSlabWall : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }

        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = 24;

            this.AddMapEntryWithWaterVisibility(new Color(100, 127, 137));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
