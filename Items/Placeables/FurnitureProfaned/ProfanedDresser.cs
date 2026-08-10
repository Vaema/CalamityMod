using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureProfaned;

public class ProfanedDresser : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureProfaned.ProfanedDresser>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ProfanedRock>(16).
            AddTile<ProfanedCrucible>().
            Register();
    }
}
