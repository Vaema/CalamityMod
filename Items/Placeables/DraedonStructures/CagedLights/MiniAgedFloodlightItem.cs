using CalamityMod.Tiles.DraedonStructures.CagedLights;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures.CagedLights
{
    public class MiniAgedFloodlightItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MiniCagedFloodlightItem>();

            Item.DefaultToPlaceableTile(ModContent.TileType<MiniAgedFloodlight>());
            Item.value = Item.sellPrice(silver: 1);
        }
    }
}
