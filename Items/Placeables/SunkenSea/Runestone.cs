using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea
{
    public class Runestone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Shellstone>();
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.Runestone>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<RunestoneWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
        }

    }
}
