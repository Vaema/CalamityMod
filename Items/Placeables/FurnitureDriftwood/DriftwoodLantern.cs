using CalamityMod.Items.Placeables.SunkenSea;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureDriftwood
{
    public class DriftwoodLantern : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureDriftwood.DriftwoodLantern>());
            Item.value = Item.sellPrice(copper: 30);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(6).
                AddIngredient(ItemID.Torch).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
