using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    [LegacyName("PurpleCandle")]
    public class ResilientCandle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public static float DefenseRatioBonus = 0.15f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DefenseRatioBonus.ToPercent(), (0.5f + DefenseRatioBonus).ToPercent(), (0.75f + DefenseRatioBonus).ToPercent(), (1f + DefenseRatioBonus).ToPercent());

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.PurpleCandle>());
            Item.value = Item.buyPrice(gold: 25); // Sold by Wizard
            Item.rare = ItemRarityID.Purple;
        }
    }
}
