using CalamityMod.Tiles.Astral;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class AstralChest : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AstralChestLocked>());
            Item.value = Item.sellPrice(silver: 5); // Special: biome chest price
        }
    }
}
