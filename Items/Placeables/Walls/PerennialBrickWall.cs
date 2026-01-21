using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls
{
    public class PerennialBrickWall : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.PerennialBrickWall>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<PerennialBrick>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
