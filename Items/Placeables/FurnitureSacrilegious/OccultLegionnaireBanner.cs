using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureSacrilegious;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureSacrilegious;

public class OccultLegionnaireBanner : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<OccultLegionnaireBannerTile>());
        Item.value = Item.sellPrice(silver: 2);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<OccultBrickItem>(3).
            AddIngredient(ItemID.Silk, 5).
            AddTile<SCalAltar>().
            Register();
    }
}
