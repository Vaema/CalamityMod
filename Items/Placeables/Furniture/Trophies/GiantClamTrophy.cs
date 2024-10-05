using CalamityMod.Tiles.Furniture.BossTrophies;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Trophies
{
    [LegacyName("GiantClamBanner")]
    public class GiantClamTrophy : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GiantClamTrophyTile>());
            Item.width = Item.height = 30;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }
    }
}
