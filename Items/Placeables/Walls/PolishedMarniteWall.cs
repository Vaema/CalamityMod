using CalamityMod.Items.Placeables.FurnitureMarnite;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls
{
    public class PolishedMarniteWall : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.PolishedMarniteWall>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<PolishedMarniteBlock>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
