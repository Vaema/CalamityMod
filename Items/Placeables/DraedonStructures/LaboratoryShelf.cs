using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures
{
    public class LaboratoryShelf : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.LaboratoryShelf>());
        }

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<LaboratoryPlating>().
                Register();
            CreateRecipe().
                AddIngredient<RustedShelf>().
                Register();
        }
    }
}
