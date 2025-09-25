using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Systems;
using Terraria.ID;
using CalamityMod.Tiles.SunkenSea;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Merges
{
    public sealed class RunestoneMerge : TileBlendTexture
    {
        public override int TileType => ModContent.TileType<Runestone>();
    }
}
