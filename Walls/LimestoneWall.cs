using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class LimestoneWall : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }

        public override void SetStaticDefaults()
        {
            DustType = DustID.Pot;
            AddMapEntry(new Color(125, 85, 61));
            this.AddMapEntryWithWaterVisibility(new Color(78, 76, 127));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
