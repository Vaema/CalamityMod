using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureOtherworldly;

[LegacyName("OccultCandelabra")]
public class OtherworldlyCandelabra : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureOtherworldly.OtherworldlyCandelabra>());
        Item.value = Item.sellPrice(silver: 3);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<OtherworldlyStone>(5).
            AddIngredient(ItemID.Torch, 3).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
