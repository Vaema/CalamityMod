using CalamityMod.Items.Placeables.Furniture.Monoliths;
using CalamityMod.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables;

public class Rock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 0;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BossRushMonolith>();
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<PlacedRock>());
        Item.value = Item.sellPrice(copper: 1);
    }
}
