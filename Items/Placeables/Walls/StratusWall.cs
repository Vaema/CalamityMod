using CalamityMod.Items.Placeables.FurnitureStratus;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls;

public class StratusWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.StratusWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<StratusBricks>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
