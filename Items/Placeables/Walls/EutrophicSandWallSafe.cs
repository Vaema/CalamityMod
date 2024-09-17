using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls
{
    public class EutrophicSandWallSafe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<EutrophicSandWall>();
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.EutrophicSandWallSafe>());

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<EutrophicSand>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
