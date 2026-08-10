using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureDriftwood;

public class DriftwoodPlatform : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureDriftwood.DriftwoodPlatform>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<Driftwood>().
            Register();
    }
}
