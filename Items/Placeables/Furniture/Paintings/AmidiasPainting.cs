using CalamityMod.Tiles.Furniture.Paintings;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture.Paintings
{
    public class AmidiasPainting : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AmidiasPaintingTile>());
            Item.width = 52;
            Item.height = 34;
            Item.value = Item.buyPrice(gold: 1);
        }
    }
}
