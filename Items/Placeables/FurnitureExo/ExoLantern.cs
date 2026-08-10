using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureExo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureExo;

public class ExoLantern : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<ExoLanternTile>());
        Item.value = Item.sellPrice(copper: 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ExoPlating>(6).
            AddIngredient(ItemID.Torch).
            AddTile<DraedonsForge>().
            Register();
    }
}
