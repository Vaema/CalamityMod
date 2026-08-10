using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureWulfrum;

public class WulfrumPlating : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.WulfrumPlating>());

    public override void AddRecipes()
    {
        CreateRecipe(25).
            AddRecipeGroup("AnyStoneBlock", 25).
            AddIngredient<WulfrumMetalScrap>().
            AddTile(TileID.Furnaces).
            Register();
        CreateRecipe().
            AddIngredient<WulfrumPlatingWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
        CreateRecipe().
            AddIngredient<WulfrumComplexPanelWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
