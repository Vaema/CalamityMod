using CalamityMod.Items.Placeables.Astral;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls;

public class AstralSnowWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.AstralSnowWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<AstralSnow>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
