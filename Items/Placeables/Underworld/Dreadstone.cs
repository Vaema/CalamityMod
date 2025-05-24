using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Underworld
{
    public class Dreadstone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 100;
        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Underworld.Dreadstone>());

    }
}
