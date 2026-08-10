using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureStatigel;

public class StatigelPlatform : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStatigel.StatigelPlatform>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<StatigelBlock>().
            Register();
    }
}
