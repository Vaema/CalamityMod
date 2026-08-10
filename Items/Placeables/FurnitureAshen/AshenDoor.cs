using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureAshen;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureAshen;

public class AshenDoor : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AshenDoorClosed>());
        Item.value = Item.sellPrice(copper: 40);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothBrimstoneSlag>(6).
            AddTile<AshenAltar>().
            Register();
    }
}
