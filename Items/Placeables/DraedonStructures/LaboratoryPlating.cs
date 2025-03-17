using CalamityMod.Items.Placeables.Walls.DraedonStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures
{
    public class LaboratoryPlating : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.LaboratoryPlating>());
        }

        public override void AddRecipes()
        {
            CreateRecipe(25).
                AddRecipeGroup("AnyStoneBlock", 25).
                AddRecipeGroup("IronBar").
                AddTile(TileID.HeavyWorkBench).
                Register();

            CreateRecipe().
                AddIngredient<RustedPlating>().
                AddTile(TileID.Anvils).
                Register();

            CreateRecipe().
                AddIngredient<LaboratoryShelf>(2).
                Register();

            CreateRecipe().
                AddIngredient<LaboratoryPlatingWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();

            CreateRecipe().
                AddIngredient<LaboratoryPlateBeam>(4).
                AddTile(TileID.WorkBenches).
                Register();

            CreateRecipe().
                AddIngredient<LaboratoryPlatePillar>(4).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
