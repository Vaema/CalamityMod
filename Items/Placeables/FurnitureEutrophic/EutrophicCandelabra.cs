using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureEutrophic
{
    public class EutrophicCandelabra : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureEutrophic.EutrophicCandelabra>());
            Item.value = Item.sellPrice(silver: 3);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(5).
                AddIngredient(ItemID.Torch, 3).
                AddTile<EutrophicShelf>().
                Register();
        }
    }
}
