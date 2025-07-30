using CalamityMod.Items.Placeables.FurnitureNavystone;
using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.SunkenSea
{
    public class Navystone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<EutrophicSand>();
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.Navystone>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<NavystoneWallSafe>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();

            CreateRecipe().
                AddIngredient<NavystonePlatform>(2).
                DisableDecraft().
                Register();
        }
    }
}
