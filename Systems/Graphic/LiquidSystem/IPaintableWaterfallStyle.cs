using Terraria;
using Terraria.Graphics;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public interface IPaintableWaterfallStyle
    {
        void ModifyDrawColor(in Tile tile, int x, int y, ref VertexColors liquidColor);
    }
}
