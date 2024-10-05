using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurniturePlagued
{
    public class PlaguedPlateBookcase : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurniturePlaguedPlate.PlaguedPlateBookcase>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PlaguedContainmentBrick>(20).
                AddIngredient(ItemID.Book, 10).
                AddTile<PlagueInfuser>().
                Register();
        }
    }
}
