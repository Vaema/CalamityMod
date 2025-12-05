using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurniturePlaguedPlate;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurniturePlagued
{
    public class PlaguedPlateDoor : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PlaguedPlateDoorClosed>());
            Item.value = Item.sellPrice(copper: 40);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PlaguedContainmentBrick>(6).
                AddTile<PlagueInfuser>().
                Register();
        }
    }
}
