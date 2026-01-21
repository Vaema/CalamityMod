using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureCosmilite
{
    public class CosmilitePlatform : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureCosmilite.CosmilitePlatform>());

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<CosmiliteBrick>().
                Register();
        }
    }
}
