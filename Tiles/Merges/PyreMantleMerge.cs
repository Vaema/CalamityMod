using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges
{
    public sealed class PyreMantleMerge : TileBlendTexture
    {
        public override int TileType => ModContent.TileType<PyreMantle>();
    }
}
