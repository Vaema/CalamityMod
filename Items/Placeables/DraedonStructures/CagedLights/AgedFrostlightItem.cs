using CalamityMod.Tiles.DraedonStructures.CagedLights;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures.CagedLights;

public class AgedFrostlightItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<CagedFrostlightItem>();

        Item.DefaultToPlaceableTile(ModContent.TileType<AgedFrostlight>());
        Item.value = Item.sellPrice(silver: 1);
    }
}
