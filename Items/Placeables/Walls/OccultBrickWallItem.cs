using CalamityMod.Items.Placeables.FurnitureSacrilegious;
using CalamityMod.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Walls
{
    public class OccultBrickWallItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<OccultBrickWall>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
            AddIngredient<OccultBrickItem>().
            AddTile(TileID.WorkBenches).
            Register();
        }
    }
}
