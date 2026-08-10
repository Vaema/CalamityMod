using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures;

public class LaboratoryElectricPanelItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<LaboratoryElectricPanel>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<LaboratoryPlating>(7).
            AddIngredient<DubiousPlating>().
            AddIngredient<DraedonPowerCell>(4).
            AddTile(TileID.Anvils).
            Register();
    }
}
