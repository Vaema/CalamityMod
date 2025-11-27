using CalamityMod.Tiles.Furniture.Paintings;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture.Paintings
{
    public class CalamityCanvas2024 : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override string Texture => "CalamityMod/Items/Placeables/Furniture/Paintings/CalamityCanvas";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CalamityCanvas2024Tile>());
            Item.width = 96;
            Item.height = 64;
            Item.value = Item.sellPrice(silver: 40);
        }
    }
}
