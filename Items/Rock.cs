using CalamityMod.Items.Placeables.Furniture.Monoliths;
using CalamityMod.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
    public class Rock : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
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
}
