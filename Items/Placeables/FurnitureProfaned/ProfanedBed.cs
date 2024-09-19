using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureProfaned
{
    public class ProfanedBed : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureProfaned.ProfanedBed>());
            Item.value = Item.sellPrice(silver: 4);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ProfanedRock>(15).
                AddIngredient(ItemID.Silk, 5).
                AddTile<ProfanedCrucible>().
                Register();
        }
    }
}
