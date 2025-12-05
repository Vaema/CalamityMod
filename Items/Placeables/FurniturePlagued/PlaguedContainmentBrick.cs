using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurniturePlagued
{
    [LegacyName("PlaguedPlate")]
    public class PlaguedContainmentBrick : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurniturePlaguedPlate.PlaguedPlate>());

        public override void AddRecipes()
        {
            // Plagued Containment Brick does not have a direct decraft condition
            // Instead, it turns into Nanodroids when shimmered before defeating Golem
            CreateRecipe(50).
                AddRecipeGroup("AnyStoneBlock", 50).
                AddIngredient<PlagueCellCanister>().
                AddTile<PlagueInfuser>().
                Register();
            CreateRecipe().
                AddIngredient<PlaguedPlateWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
            CreateRecipe().
                AddIngredient<PlaguedPlatePlatform>(2).
                DisableDecraft().
                Register();
        }
    }
}
