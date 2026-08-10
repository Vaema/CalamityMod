using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureCosmilite;

public class CosmiliteClock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureCosmilite.CosmiliteClock>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<CosmiliteBrick>(10).
            AddRecipeGroup("IronBar", 3).
            AddIngredient(ItemID.Glass, 6).
            AddTile<CosmicAnvil>().
            Register();
    }
}
