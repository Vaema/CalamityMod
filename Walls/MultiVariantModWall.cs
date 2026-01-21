using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public abstract class MultiVariantModWall : ModWall
    {
        public virtual void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset) { }
    }
}
