using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureSacrilegious;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSacrilegious;

public class OccultBrickItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<OccultBrickTile>());

    public override void AddRecipes()
    {
        CreateRecipe(400).
            AddRecipeGroup("AnyStoneBlock", 400).
            AddIngredient<AshesofAnnihilation>().
            AddTile<SCalAltar>().
            Register();
        CreateRecipe().
            AddIngredient<OccultPlatformItem>(2).
            DisableDecraft().
            Register();
        CreateRecipe().
            AddIngredient<OccultBrickWallItem>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
