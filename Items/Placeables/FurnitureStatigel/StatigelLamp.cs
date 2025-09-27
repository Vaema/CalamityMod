using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureStatigel
{
    public class StatigelLamp : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStatigel.StatigelLamp>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<StatigelBlock>(3).
                AddIngredient(ItemID.Torch).
                AddTile(TileID.Solidifier).
                Register();
        }
    }
}
