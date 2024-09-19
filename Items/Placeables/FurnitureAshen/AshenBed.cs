using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAshen
{
    public class AshenBed : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAshen.AshenBed>());
            Item.value = Item.sellPrice(silver: 4);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SmoothBrimstoneSlag>(15).
                AddIngredient(ItemID.Silk, 5).
                AddTile<AshenAltar>().
                Register();
        }
    }
}
