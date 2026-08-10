using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Abyss;

public class PyreMantle : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Abyss.PyreMantle>());

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PyreMantleWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
