using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls;

public class AcidwoodWallItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.AcidwoodWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
        AddIngredient<Acidwood>().
        AddTile(TileID.WorkBenches).
        Register();
    }
}
