using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Crags;
using CalamityMod.Items.Placeables.Ores;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAncient
{
    public class AncientAltar : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAncient.AncientAltar>());
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BrimstoneSlag>(10).
                AddIngredient<ScorchedBone>(5).
                AddTile(ModContent.TileType<Tiles.Furniture.CraftingStations.AshenAltar>()).
                Register();
        }
    }
}
