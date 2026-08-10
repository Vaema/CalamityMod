using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls;

[LegacyName("ChaosplateWall")]
public class HavocplateWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.HavocplateWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<Plates.Havocplate>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
