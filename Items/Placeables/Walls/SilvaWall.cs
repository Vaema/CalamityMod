using CalamityMod.Items.Placeables.FurnitureSilva;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls;

public class SilvaWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.SilvaWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<SilvaCrystal>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
