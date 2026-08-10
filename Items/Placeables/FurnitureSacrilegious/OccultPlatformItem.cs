using CalamityMod.Tiles.FurnitureSacrilegious;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureSacrilegious;

public class OccultPlatformItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<OccultPlatformTile>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<OccultBrickItem>().
            Register();
    }
}
