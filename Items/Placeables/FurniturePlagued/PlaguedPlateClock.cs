using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurniturePlagued;

public class PlaguedPlateClock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurniturePlaguedPlate.PlaguedPlateClock>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PlaguedContainmentBrick>(10).
            AddRecipeGroup("IronBar", 3).
            AddIngredient(ItemID.Glass, 6).
            AddTile<PlagueInfuser>().
            Register();
    }
}
