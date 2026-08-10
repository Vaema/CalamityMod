using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
using Terraria.ID;
using CalamityMod.Items.Placeables.FurnitureDriftwood;

namespace CalamityMod.Items.Placeables.Walls;

public class DriftwoodWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.DriftwoodWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
        AddIngredient<Driftwood>().
        AddTile(TileID.WorkBenches).
        Register();
    }
}
