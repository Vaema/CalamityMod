using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureNavystone
{
    public class NavystoneSink : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.NavystoneSink>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SmoothNavystone>(6).
                AddIngredient(ItemID.WaterBucket).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
