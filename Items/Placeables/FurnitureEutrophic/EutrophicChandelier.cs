using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureEutrophic
{
    public class EutrophicChandelier : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureEutrophic.EutrophicChandelier>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(4).
                AddIngredient(ItemID.Torch, 4).
                AddIngredient(ItemID.Chain).
                AddTile<EutrophicShelf>().
                Register();
        }
    }
}
