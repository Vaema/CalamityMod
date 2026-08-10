using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureOtherworldly;

[LegacyName("OccultChair")]
public class OtherworldlyChair : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureOtherworldly.OtherworldlyChair>());
        Item.value = Item.sellPrice(copper: 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<OtherworldlyStone>(4).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
