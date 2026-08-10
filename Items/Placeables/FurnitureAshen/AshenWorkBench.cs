using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureAshen;

[LegacyName("AshenWorkbench")]
public class AshenWorkBench : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAshen.AshenWorkBench>());
        Item.value = Item.sellPrice(copper: 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothBrimstoneSlag>(10).
            AddTile<AshenAltar>().
            Register();
    }
}
