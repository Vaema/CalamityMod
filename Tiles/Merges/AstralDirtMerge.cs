using CalamityMod.Systems;
using CalamityMod.Tiles.Astral;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges;

public sealed class AstralDirtMerge : TileBlendTexture
{
    public override int TileType => ModContent.TileType<AstralDirt>();
}
