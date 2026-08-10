using CalamityMod.Systems;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges;

public sealed class AstralSnowMerge : TileBlendTexture
{
    public override int TileType => ModContent.TileType<AstralSnow.AstralSnow>();
}
