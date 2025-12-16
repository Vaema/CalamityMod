using Terraria;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public interface IWaterStylePostDrawEffect
    {
        public void PostDrawEffect(in Tile tile, int x, int y);
    }
}
