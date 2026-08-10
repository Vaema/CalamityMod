using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureOtherworldly;

[LegacyName("OccultStone")]
public class OtherworldlyStone : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureOtherworldly.OtherworldlyStone>());

    public override void AddRecipes()
    {
        CreateRecipe(200).
            AddRecipeGroup("AnyStoneBlock", 200).
            AddIngredient<DarkPlasma>().
            AddIngredient<ArmoredShell>().
            AddIngredient<TwistingNether>().
            AddIngredient(ItemID.Silk, 10).
            AddTile(TileID.AdamantiteForge).
            Register();
        CreateRecipe().
            AddIngredient<OtherworldlyStoneWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
        CreateRecipe().
            AddIngredient<OtherworldlyPlatform>(2).
            DisableDecraft().
            Register();
    }
}
