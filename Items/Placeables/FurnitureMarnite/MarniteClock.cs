using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureMarnite;

public class MarniteClock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureMarnite.MarniteClock>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PolishedMarniteBlock>(4).
            AddRecipeGroup("IronBar", 3).
            AddIngredient(ItemID.Glass, 6).
            AddTile(TileID.Sawmill).
            Register();
    }
}
