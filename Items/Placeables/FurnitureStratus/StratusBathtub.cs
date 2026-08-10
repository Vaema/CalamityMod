using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureStratus;

public class StratusBathtub : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStratus.StratusBathtub>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<StratusBricks>(14).
            AddTile<Tiles.Furniture.CraftingStations.VoidCondenser>().
            Register();
    }
}
