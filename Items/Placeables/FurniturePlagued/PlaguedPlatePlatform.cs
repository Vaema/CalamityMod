using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurniturePlagued;

public class PlaguedPlatePlatform : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurniturePlaguedPlate.PlaguedPlatePlatform>());

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<PlaguedContainmentBrick>().
            Register();
    }
}
