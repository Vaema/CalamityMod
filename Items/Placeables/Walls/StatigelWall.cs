using CalamityMod.Items.Placeables.FurnitureStatigel;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
namespace CalamityMod.Items.Placeables.Walls;

public class StatigelWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.StatigelWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<StatigelBlock>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
