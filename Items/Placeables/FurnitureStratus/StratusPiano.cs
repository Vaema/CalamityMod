using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureStratus;

public class StratusPiano : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStratus.StratusPiano>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<StratusBricks>(15).
            AddIngredient(ItemID.Bone, 4).
            AddIngredient(ItemID.Book).
            AddTile<Tiles.Furniture.CraftingStations.VoidCondenser>().
            Register();
    }
}
