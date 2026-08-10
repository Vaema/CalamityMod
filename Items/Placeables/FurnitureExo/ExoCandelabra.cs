using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureExo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureExo;

public class ExoCandelabra : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<ExoCandelabraTile>());
        Item.value = Item.sellPrice(silver: 3);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ExoPlating>(5).
            AddIngredient(ItemID.Torch, 3).
            AddTile<DraedonsForge>().
            Register();
    }
}
