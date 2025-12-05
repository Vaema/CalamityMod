using CalamityMod.Items.DraedonMisc;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureExo;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureExo
{
    public class ExoScreen : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ExoScreenTile>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ExoPlating>(10).
                AddIngredient<DraedonPowerCell>(3).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
