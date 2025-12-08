using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureStatigel
{
    public class StatigelCandelabra : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStatigel.StatigelCandelabra>());
            Item.value = Item.sellPrice(silver: 3);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<StatigelBlock>(5).
                AddIngredient(ItemID.Torch, 3).
                AddTile(TileID.Solidifier).
                Register();
        }
    }
}
