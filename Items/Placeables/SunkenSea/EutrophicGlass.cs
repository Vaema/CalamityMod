using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea;

public class EutrophicGlass : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.EutrophicGlass>());

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<EutrophicSand>(2).
            AddTile(TileID.Furnaces).
            Register();

        CreateRecipe().
            AddIngredient<EutrophicGlassWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
