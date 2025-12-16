using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges
{
    public sealed class AbyssGravelMerge : TileBlendTexture
    {
        public override int TileType => ModContent.TileType<AbyssGravel>();
    }
}
