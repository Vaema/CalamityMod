using CalamityMod.Items.Placeables.FurnitureExo;
using CalamityMod.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Walls
{
    public class ExoPlatingWallItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<ExoPlatingWall>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<ExoPlating>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
