using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges;

public sealed class ShellstoneMerge : TileBlendTexture
{
    public override int TileType => ModContent.TileType<Shellstone>();
}
