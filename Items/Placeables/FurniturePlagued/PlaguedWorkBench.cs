using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurniturePlaguedPlate;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurniturePlagued;

[LegacyName("PlaguedPlateWorkbench")]
public class PlaguedWorkBench : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<PlaguedPlateWorkBenchTile>());
        Item.value = Item.sellPrice(copper: 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PlaguedContainmentBrick>(10).
            AddTile<PlagueInfuser>().
            Register();
    }
}
