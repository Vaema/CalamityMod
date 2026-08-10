using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureOtherworldly;

[LegacyName("OccultLamp")]
public class OtherworldlyLamp : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureOtherworldly.OtherworldlyLamp>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<OtherworldlyStone>(3).
            AddIngredient(ItemID.Torch).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
