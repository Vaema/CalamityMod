using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureWulfrum.FurnitureAnodizedWulfrum
{
    public class AnodizedWulfrumSink : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.FurnitureAnodizedWulfrum.AnodizedWulfrumSink>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<RoundedAnodizedWulfrumPanels>(6).
                AddIngredient(ItemID.WaterBucket).
                AddTile(TileID.HeavyWorkBench).
                Register();
        }
    }
}
