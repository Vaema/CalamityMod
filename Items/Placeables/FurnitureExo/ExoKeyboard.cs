using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureExo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureExo
{
    public class ExoKeyboard : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ExoKeyboardTile>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ExoPlating>(15).
                AddIngredient(ItemID.Bone, 4).
                AddIngredient(ItemID.Book).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
