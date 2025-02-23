using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class SulphurousShaleWall : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }
        public override void SetStaticDefaults()
        {
            DustType = 32;
            AddMapEntry(new Color(59, 40, 63));
            this.AddMapEntryWithWaterVisibility(new (44, 62, 125));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
