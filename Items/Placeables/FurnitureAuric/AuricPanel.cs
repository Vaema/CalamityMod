using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureAuric;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAuric;

public class AuricPanel : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }
    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<AuricPanelTile>());

    public override void AddRecipes()
    {
        CreateRecipe(400).
            AddRecipeGroup("AnyStoneBlock", 400).
            AddIngredient<AuricOre>().
            AddTile<CosmicAnvil>().
            Register();
        CreateRecipe().
            AddIngredient<AuricPlatform>(2).
            DisableDecraft().
            Register();
        /*CreateRecipe().
            AddIngredient<AuricPanelWallItem>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();*/
    }
}
