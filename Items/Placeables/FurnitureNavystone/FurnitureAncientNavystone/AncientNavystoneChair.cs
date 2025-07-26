using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureNavystone.FurnitureAncientNavystone
{
    [LegacyName("EutrophicChair")]
    public class AncientNavystoneChair : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.FurnitureAncientNavystone.AncientNavystoneChair>());
            Item.value = Item.sellPrice(copper: 30);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(4).
                AddTile<EutrophicShelf>().
                Register();
        }
    }
}
