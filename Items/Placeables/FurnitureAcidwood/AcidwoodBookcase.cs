using CalamityMod.Tiles.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAcidwood
{
    public class AcidwoodBookcase : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AcidwoodBookcaseTile>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(20).
                AddIngredient(ItemID.Book, 10).
                AddTile(TileID.Sawmill).
                Register();
        }
    }
}
