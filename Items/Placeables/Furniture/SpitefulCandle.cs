using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture;

[LegacyName("YellowCandle")]
public class SpitefulCandle : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public static float ExtraChipDamageRatio = 0.07f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((1f + ExtraChipDamageRatio).ToString(), ExtraChipDamageRatio.ToString());

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.YellowCandle>());
        Item.value = Item.buyPrice(gold: 25); // Sold by Wizard
        Item.rare = ItemRarityID.Purple;
    }
}
