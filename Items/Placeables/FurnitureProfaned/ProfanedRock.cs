using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureProfaned;

public class ProfanedRock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureProfaned.ProfanedRock>());

    public override void AddRecipes()
    {
        CreateRecipe(50).
            AddRecipeGroup("AnyStoneBlock", 50).
            AddIngredient<UnholyEssence>().
            AddTile(TileID.AdamantiteForge).
            Register();
        CreateRecipe().
            AddIngredient<ProfanedRockWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
        CreateRecipe().
            AddIngredient<ProfanedPlatform>(2).
            DisableDecraft().
            Register();
    }
}
