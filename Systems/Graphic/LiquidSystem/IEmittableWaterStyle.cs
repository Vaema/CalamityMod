using Terraria;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public interface IEmittableWaterStyle
    {
        void ModifyLight(ref readonly Tile tile, int i, int j, int type, ref float r, ref float g, ref float b);
    }
}
