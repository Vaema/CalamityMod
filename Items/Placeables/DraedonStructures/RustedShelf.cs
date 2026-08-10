using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures;

public class RustedShelf : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.RustedShelf>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<RustedPlating>().
            Register();
        CreateRecipe().
            AddIngredient<LaboratoryShelf>().
            Register();
    }
}
