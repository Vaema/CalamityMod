using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls
{
    [LegacyName("ChaoticBrickWall")]
    public class ScoriaBrickWall : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.ScoriaBrickWall>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<ScoriaBrick>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
