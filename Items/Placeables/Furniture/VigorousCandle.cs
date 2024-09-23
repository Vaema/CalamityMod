using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    [LegacyName("PinkCandle")]
    public class VigorousCandle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.PinkCandle>());
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Pink;
        }
    }
}
