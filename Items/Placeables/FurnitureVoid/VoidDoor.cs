using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureVoid;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureVoid;

public class VoidDoor : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<VoidDoorClosed>());
        Item.value = Item.sellPrice(copper: 40);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothVoidstone>(6).
            AddTile<VoidCondenser>().
            Register();
    }
}
