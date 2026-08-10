using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureAuric;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAuric;

public class ExoAuricPanel : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }
    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<ExoAuricPanelTile>());

    public override void AddRecipes()
    {
        CreateRecipe(400).
            AddIngredient<AuricPanel>(400).
            AddIngredient<ExoPrism>().
            AddTile<DraedonsForge>().
            Register();
        /*CreateRecipe().
            AddIngredient<ExoAuricPanelWallItem>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();*/
    }
}
