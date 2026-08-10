using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSilva;

public class SilvaTable : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureSilva.SilvaTable>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SilvaCrystal>(8).
            AddTile(TileID.GlassKiln).
            Register();
    }
}
