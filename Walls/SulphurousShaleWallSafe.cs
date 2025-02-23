using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class SulphurousShaleWallSafe : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }
        public override string Texture => "CalamityMod/Walls/SulphurousShaleWall";
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = 32;
            AddMapEntry(new Color(59, 40, 63));
            this.AddMapEntryWithWaterVisibility(new Color(44, 62, 125));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
