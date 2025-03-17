using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Systems;
using CalamityMod.Tiles.AstralDesert;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges
{
    public sealed class AstralSandstoneMerge : TileBlendTexture
    {
        public override int TileType => ModContent.TileType<AstralSandstone>();
    }
}
