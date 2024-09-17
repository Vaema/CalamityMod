using CalamityMod.Items.Placeables.FurnitureProfaned;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls
{
    public class RunicProfanedBrickWall : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.RunicProfanedBrickWall>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<RunicProfanedBrick>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
