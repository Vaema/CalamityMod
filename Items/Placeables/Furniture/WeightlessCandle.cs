using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture;

[LegacyName("BlueCandle")]
public class WeightlessCandle : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public static float MoveSpeedBoost = 0.1f;
    public static double WingTimeBoost = 0.1D;
    public static float AccelerationBoost = 0.1f; // All 10% so we only need just one in the tooltip
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.BlueCandle>());
        Item.value = Item.buyPrice(gold: 25); // Sold by Wizard
        Item.rare = ItemRarityID.Purple;
    }
}
