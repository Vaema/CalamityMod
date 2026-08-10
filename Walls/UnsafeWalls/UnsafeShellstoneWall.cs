using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls.UnsafeWalls;

public class UnsafeShellstoneWall : ModWall, IVisibleThroughWater
{
    public override string Texture => "CalamityMod/Walls/ShellstoneWall";
    int IVisibleThroughWater.WaterMapEntry { get; set; }

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = false;
        DustType = DustID.CorruptionThorns;

        this.AddMapEntryWithWaterVisibility(new Color(74, 71, 84));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override bool Drop(int i, int j, ref int type)
    {
        return false;
    }
}
