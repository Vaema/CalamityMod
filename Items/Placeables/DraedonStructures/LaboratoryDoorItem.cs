using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures;

public class LaboratoryDoorItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<LaboratoryDoorClosed>());
        Item.value = Item.sellPrice(copper: 40); // Non-standard Draedon's furniture: uses Door prices
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<LaboratoryPlating>(6).
            AddTile(TileID.Anvils).
            Register();
    }
}
