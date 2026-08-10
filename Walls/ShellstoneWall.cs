using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls;

public class ShellstoneWall : ModWall, IVisibleThroughWater
{
    int IVisibleThroughWater.WaterMapEntry { get; set; }

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;
        DustType = DustID.CorruptionThorns;

        this.AddMapEntryWithWaterVisibility(new Color(74, 71, 84));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
