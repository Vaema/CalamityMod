using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureEutrophic
{
    public class EutrophicSink : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureEutrophic.EutrophicSink>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(6).
                AddIngredient(ItemID.WaterBucket).
                AddTile<EutrophicShelf>().
                Register();
        }
    }
}
