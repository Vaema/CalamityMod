using CalamityMod.Tiles.FloralParadise;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FloralParadise
{
    public class PeatItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Peat>());
    }
}
