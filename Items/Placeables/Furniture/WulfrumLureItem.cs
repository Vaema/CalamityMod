using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class WulfrumLureItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public static int SignalTime = 30 * 60;
        public static int SpawnIntervals = 4 * 60;
        public static int MaxEnemiesPerWave = 3;

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<WulfrumLure>());
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
            AddIngredient<WulfrumMetalScrap>(5).
            AddIngredient<WulfrumBattery>().
            AddTile(TileID.Anvils).
            Register();
        }
    }
}
