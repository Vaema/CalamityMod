using CalamityMod.Items.Placeables.SunkenSea;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls
{
    public class NavystoneWallSafe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<NavystoneWall>();
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.NavystoneWallSafe>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<Navystone>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
