using CalamityMod.Tiles.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAcidwood
{
    public class AcidwoodCandelabra : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AcidwoodCandelabraTile>());
            Item.value = Item.sellPrice(silver: 3);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(5).
                AddIngredient(ItemID.Torch, 3).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
