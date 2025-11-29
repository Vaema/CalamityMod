using Terraria;
using Terraria.Graphics;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public interface IPaintableWaterStyle
    {
        public void ModifyDrawColor(in Tile tile, int x, int y, ref VertexColors liquidColor, bool isSlope);
    }
}
