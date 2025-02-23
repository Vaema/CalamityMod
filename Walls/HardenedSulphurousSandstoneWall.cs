using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class HardenedSulphurousSandstoneWall : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = 32;
            AddMapEntry(new Color(62, 50, 39));
            this.AddMapEntryWithWaterVisibility(new Color(49, 66, 133));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
