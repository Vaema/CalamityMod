using CalamityMod.Tiles.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAcidwood
{
    public class AcidwoodLantern : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AcidwoodLanternTile>());
            Item.value = Item.sellPrice(copper: 30);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(6).
                AddIngredient(ItemID.Torch).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
