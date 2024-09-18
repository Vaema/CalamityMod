using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureEutrophic
{
    public class SmoothNavystone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureEutrophic.SmoothNavystone>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>().
                AddTile(TileID.WorkBenches).
                Register();
            CreateRecipe().
                AddIngredient<SmoothNavystoneWall>(4).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
