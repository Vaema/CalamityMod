using CalamityMod.Items.Placeables.SunkenSea;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureDriftwood
{
    public class DriftwoodChandelier : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureDriftwood.DriftwoodChandelier>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(4).
                AddIngredient(ItemID.Torch, 4).
                AddIngredient(ItemID.Chain).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
