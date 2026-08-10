using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea;

public class SeaPrismBrick : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.SeaPrismBrick>());

    public override void AddRecipes()
    {
        CreateRecipe(25).
                AddRecipeGroup("AnyStoneBlock", 25).
                AddIngredient<SeaPrism>().
                AddTile(TileID.Furnaces).
                Register();
    }
}
