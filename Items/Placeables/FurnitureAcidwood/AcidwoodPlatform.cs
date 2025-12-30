using CalamityMod.Tiles.FurnitureAcidwood;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureAcidwood
{
    public class AcidwoodPlatform : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<AcidwoodPlatformTile>());

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<Acidwood>().
                Register();
        }
    }
}
