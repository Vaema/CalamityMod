using Terraria;
using Terraria.Graphics;

namespace CalamityMod.Systems.Graphic.LiquidSystem;

public interface IWaterStyleModifyColor
{
    public void ModifyColor(in Tile tile, int x, int y, ref VertexColors liquidColor, bool isSlope);
}
