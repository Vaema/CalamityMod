using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureOtherworldly;

[LegacyName("OccultBed")]
public class OtherworldlyBed : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureOtherworldly.OtherworldlyBed>());
        Item.value = Item.sellPrice(silver: 4);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<OtherworldlyStone>(15).
            AddIngredient(ItemID.Silk, 5).
            AddTile(TileID.Sawmill).
            Register();
    }
}
