using CalamityMod.Tiles.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAcidwood
{
    public class AcidwoodClock : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AcidwoodClockTile>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(10).
                AddRecipeGroup("IronBar", 3).
                AddIngredient(ItemID.Glass, 6).
                AddTile(TileID.Sawmill).
                Register();
        }
    }
}
