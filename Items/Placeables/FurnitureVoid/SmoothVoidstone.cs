using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureVoid;

public class SmoothVoidstone : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureVoid.SmoothVoidstone>());

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Voidstone>().
            AddTile(TileID.WorkBenches).
            Register();
        CreateRecipe().
            AddIngredient<SmoothVoidstoneWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
        CreateRecipe().
            AddIngredient<SmoothVoidstonePlatform>(2).
            DisableDecraft().
            Register();
    }
}
