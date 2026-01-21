using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class AbyssTreasureChest : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Abyss.AbyssTreasureChest>());
            Item.value = Item.sellPrice(silver: 10); // Special: generated chest price
        }
    }
}
