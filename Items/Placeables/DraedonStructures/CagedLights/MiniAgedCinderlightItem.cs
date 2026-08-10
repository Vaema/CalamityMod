using CalamityMod.Tiles.DraedonStructures.CagedLights;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures.CagedLights;

public class MiniAgedCinderlightItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MiniCagedCinderlightItem>();

        Item.DefaultToPlaceableTile(ModContent.TileType<MiniAgedCinderlight>());
        Item.value = Item.sellPrice(silver: 1);
    }
}
