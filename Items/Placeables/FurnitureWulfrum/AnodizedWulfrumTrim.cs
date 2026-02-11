using CalamityMod.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureWulfrum
{
    public class AnodizedWulfrumTrim : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.AnodizedWulfrumTrim>());

        public override void AddRecipes()
        {
            CreateRecipe(25).
                AddRecipeGroup("AnyStoneBlock", 25).
                AddIngredient<AnodizedWulfrumMetal>().
                AddTile(TileID.HeavyWorkBench).
                Register();
        }
    }
}
