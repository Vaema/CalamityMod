using CalamityMod.Tiles.FurnitureStratus;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureStratus;

public class StratusStarPlatformItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<StratusStarPlatform>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<StratusBricks>().
            Register();
    }
}
