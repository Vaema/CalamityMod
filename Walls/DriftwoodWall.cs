using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Walls;

public class DriftwoodWall : ModWall, IVisibleThroughWater
{
    int IVisibleThroughWater.WaterMapEntry { get; set; }

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        this.AddMapEntryWithWaterVisibility(new Color(69, 56, 58));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
