using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Tiles;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
    public class GreedPot : ModItem
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<GreedPotTile>());

    }
}
