using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureAbyss;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureAbyss
{
    public class AbyssDoor : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AbyssDoorClosed>());
            Item.value = Item.sellPrice(copper: 40);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SmoothAbyssGravel>(6).
                AddTile<VoidCondenser>().
                Register();
        }
    }
}
