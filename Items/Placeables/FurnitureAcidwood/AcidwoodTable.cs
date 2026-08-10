using CalamityMod.Tiles.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAcidwood;

public class AcidwoodTable : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AcidwoodTableTile>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Acidwood>(8).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
