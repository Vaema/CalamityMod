using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;
namespace CalamityMod.Walls;

public class NavyslateWall : ModWall, IVisibleThroughWater
{
    int IVisibleThroughWater.WaterMapEntry { get; set; }

    public override void SetStaticDefaults()
    {
        DustType = DustID.BlueMoss;
        this.AddMapEntryWithWaterVisibility(new Color(11, 40, 43));
    }
    
    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
