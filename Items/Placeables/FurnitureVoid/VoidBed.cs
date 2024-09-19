using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureVoid
{
    public class VoidBed : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureVoid.VoidBed>());
            Item.value = Item.sellPrice(silver: 4);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SmoothVoidstone>(15).
                AddIngredient(ItemID.Silk, 5).
                AddTile<VoidCondenser>().
                Register();
        }
    }
}
