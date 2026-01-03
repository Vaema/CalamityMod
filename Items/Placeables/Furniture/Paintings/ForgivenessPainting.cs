using CalamityMod.Tiles.Furniture.Paintings;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture.Paintings
{
    public class ForgivenessPainting : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ForgivenessPaintingTile>());
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.buyPrice(gold: 15);
        }
    }
}
