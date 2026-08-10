using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureVoid;

[LegacyName("VoidBath")]
public class VoidBathtub : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureVoid.VoidBath>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothVoidstone>(14).
            AddTile<VoidCondenser>().
            Register();
    }
}
