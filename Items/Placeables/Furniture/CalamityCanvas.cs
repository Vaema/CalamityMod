using CalamityMod.Tiles.Furniture;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class CalamityCanvas : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CalamityCanvasTile>());
            Item.width = 96;
            Item.height = 64;
            Item.value = Item.sellPrice(silver: 40);
        }
    }
}
