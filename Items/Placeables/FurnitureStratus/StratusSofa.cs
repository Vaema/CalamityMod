using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureStratus;

public class StratusSofa : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStratus.StratusSofa>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<StratusBricks>(5).
            AddIngredient(ItemID.Silk, 2).
            AddTile<Tiles.Furniture.CraftingStations.VoidCondenser>().
            Register();
    }
}
