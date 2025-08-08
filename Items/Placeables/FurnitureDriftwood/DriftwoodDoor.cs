using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.FurnitureDriftwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureDriftwood
{
    public class DriftwoodDoor : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<DriftwoodDoorClosed>());
            Item.value = Item.sellPrice(copper: 40);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(6).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
