using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.FurnitureDriftwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureDriftwood
{
    public class DriftwoodBaroqueCello : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureDriftwood.DriftwoodBaroqueCello>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(15).
                AddIngredient(ItemID.Silk, 4).
                AddTile(TileID.Sawmill).
                Register();
        }
    }
}
