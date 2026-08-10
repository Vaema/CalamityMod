using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls;

public class RoundedAnodizedWulfrumPanelWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.RoundedAnodizedWulfrumPanelWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<Items.Placeables.FurnitureWulfrum.RoundedAnodizedWulfrumPanels>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
