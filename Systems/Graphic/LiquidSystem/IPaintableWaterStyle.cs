using Terraria.Graphics;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public interface IPaintableWaterStyle
    {
        void DrawColor(int x, int y, int type, ref VertexColors liquidColor, bool isSlope = false);
    }
}
