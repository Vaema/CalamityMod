using CalamityMod.Items.Placeables.Crags;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAncient
{
    public class AncientBookcase : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAncient.AncientBookcase>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BrimstoneSlag>(20).
                AddIngredient(ItemID.Book, 10).
                AddTile<AshenAltar>().
                Register();
        }
    }
}
