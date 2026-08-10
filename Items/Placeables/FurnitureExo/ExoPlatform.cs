using CalamityMod.Tiles.FurnitureExo;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureExo;

public class ExoPlatform : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<ExoPlatformTile>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<ExoPlating>().
            Register();
    }
}
