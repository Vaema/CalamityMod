using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class SulphurousSandstoneWall : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }
        public override void SetStaticDefaults()
        {
            DustType = 32;
            AddMapEntry(new Color(57, 45, 38));
            this.AddMapEntryWithWaterVisibility(new Color(45, 60, 120));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
