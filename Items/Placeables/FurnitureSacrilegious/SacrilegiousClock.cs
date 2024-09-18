using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureSacrilegious;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSacrilegious
{
    public class SacrilegiousClock : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SacrilegiousClockTile>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OccultBrickItem>(10).
                AddRecipeGroup("IronBar", 3).
                AddIngredient(ItemID.Glass, 6).
                AddTile<SCalAltar>().
                Register();
        }
    }
}
