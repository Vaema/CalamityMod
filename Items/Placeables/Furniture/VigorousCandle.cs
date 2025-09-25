using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    [LegacyName("PinkCandle")]
    public class VigorousCandle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public static double PercentHealthPerSecond = 0.004D;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(PercentHealthPerSecond.ToPercent());

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.PinkCandle>());
            Item.value = Item.buyPrice(gold: 25); // Sold by Wizard
            Item.rare = ItemRarityID.Pink;
        }
    }
}
