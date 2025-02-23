using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class AbyssGravelWallSafe : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }
        public override string Texture => "CalamityMod/Walls/AbyssGravelWall";
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            AddMapEntry(new Color(6, 10, 54));
            this.AddMapEntryWithWaterVisibility(new Color(21, 47, 111));
            DustType = 33;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
