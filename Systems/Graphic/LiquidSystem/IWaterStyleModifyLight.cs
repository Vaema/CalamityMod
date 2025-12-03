using Terraria;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public interface IWaterStyleModifyLight
    {
        void ModifyLight(in Tile tile, int x, int y, ref float r, ref float g, ref float b);
    }
}
