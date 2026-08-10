using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls;

[LegacyName("NavystoneWallSafe")]
public class NavystoneWall : ModWall, IVisibleThroughWater
{
    public override string Texture => "CalamityMod/Walls/NavystoneWall";
    int IVisibleThroughWater.WaterMapEntry { get; set; }
    
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;
        DustType = DustID.BlueMoss;
        this.AddMapEntryWithWaterVisibility(new Color(0, 50, 50));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
