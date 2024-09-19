using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureEutrophic
{
    public class EutrophicLantern : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureEutrophic.EutrophicLantern>());
            Item.value = Item.sellPrice(copper: 30);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(6).
                AddIngredient(ItemID.Torch).
                AddTile<EutrophicShelf>().
                Register();
        }
    }
}
