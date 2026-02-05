using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureWulfrum
{
    public class WulfrumLabstationItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.WulfrumLabstation>());
            Item.value = Item.sellPrice(silver: 1); // This is REALLY too easy to craft to sell for 2 gold
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<WulfrumPlating>(20).
                AddTile(TileID.HeavyWorkBench).
                Register();
        }
    }
}
