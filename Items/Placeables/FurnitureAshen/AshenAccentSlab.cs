using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAshen
{
    public class AshenAccentSlab : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override string Texture => "CalamityMod/Items/Placeables/FurnitureAshen/AshenSlab";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAshen.AshenAccentSlab>());

        public override void AddRecipes()
        {
            CreateRecipe(50).
                AddIngredient<SmoothBrimstoneSlag>(50).
                AddTile<AshenAltar>().
                AddCondition(Condition.InGraveyard).
                Register();
        }
    }
}
