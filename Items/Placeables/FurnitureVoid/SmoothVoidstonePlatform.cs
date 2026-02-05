using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureVoid
{
    public class SmoothVoidstonePlatform : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureVoid.SmoothVoidstonePlatform>());

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<SmoothVoidstone>().
                Register();
        }
    }
}
