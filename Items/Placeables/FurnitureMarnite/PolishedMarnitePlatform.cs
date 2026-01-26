using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureMarnite
{
    public class PolishedMarnitePlatform : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureMarnite.PolishedMarnitePlatform>());

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<PolishedMarniteBlock>().
                Register();
        }
    }
}
