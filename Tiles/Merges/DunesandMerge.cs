using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges;

public sealed class DunesandMerge : TileBlendTexture
{
    public override int TileType => ModContent.TileType<Dunesand>();
}
