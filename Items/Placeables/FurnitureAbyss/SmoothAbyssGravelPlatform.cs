using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAbyss
{
    public class SmoothAbyssGravelPlatform : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAbyss.SmoothAbyssGravelPlatform>());

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<SmoothAbyssGravel>().
                Register();
        }
    }
}
