using Terraria;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public interface IEmittableWaterStyle
    {
        void ModifyLight(in Tile tile, int x, int y, ref float r, ref float g, ref float b);
    }
}
