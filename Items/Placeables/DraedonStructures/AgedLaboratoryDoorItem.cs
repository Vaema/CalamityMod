using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures;

public class AgedLaboratoryDoorItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AgedLaboratoryDoorClosed>());
        Item.value = Item.sellPrice(copper: 40); // Non-standard Draedon's furniture: uses Door prices
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<RustedPlating>(6).
            AddTile(TileID.Anvils).
            Register();
    }
}
