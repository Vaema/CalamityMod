using CalamityMod.Systems;
using CalamityMod.Tiles.AstralDesert;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges
{
    public sealed class AstralSandMerge : TileBlendTexture
    {
        public override int TileType => ModContent.TileType<AstralSand>();
    }
}
