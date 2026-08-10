using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureAshen;

public class AshenPlatform : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAshen.AshenPlatform>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<SmoothBrimstoneSlag>().
            Register();
    }
}
