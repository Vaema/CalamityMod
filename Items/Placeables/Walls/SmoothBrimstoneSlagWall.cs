using CalamityMod.Items.Placeables.FurnitureAshen;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls;

public class SmoothBrimstoneSlagWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.SmoothBrimstoneSlagWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<SmoothBrimstoneSlag>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
