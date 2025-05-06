using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureOtherworldly
{
    [LegacyName("OccultTable")]
    public class OtherworldlyTable : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureOtherworldly.OtherworldlyTable>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OtherworldlyStone>(8).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
