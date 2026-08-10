using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSilva;

public class SilvaPlatform : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureSilva.SilvaPlatform>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<SilvaCrystal>().
            Register();
    }
}
