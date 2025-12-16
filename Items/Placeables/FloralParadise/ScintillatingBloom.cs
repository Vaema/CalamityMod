using CalamityMod.Tiles.FloralParadise;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FloralParadise
{
    public class ScintillatingBloom : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<PinkFlower>());
    }
}
