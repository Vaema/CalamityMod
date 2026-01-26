using CalamityMod.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls
{
    public class WulfrumComplexPanelWall : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.WulfrumComplexPanelWall>());

        public override void AddRecipes()
        {
            CreateRecipe(8).
                AddIngredient<WulfrumMetalScrap>().
                AddIngredient<AnodizedWulfrumMetal>().
                AddIngredient<EnergyCore>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
