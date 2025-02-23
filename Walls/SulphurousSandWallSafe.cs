using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class SulphurousSandWallSafe : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }
        public override string Texture => "CalamityMod/Walls/SulphurousSandWall";
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = 32;
            AddMapEntry(new Color(84, 71, 46));
            this.AddMapEntryWithWaterVisibility(new Color(59, 69, 121));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
