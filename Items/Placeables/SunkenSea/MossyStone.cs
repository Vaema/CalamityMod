using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea
{
    public class MossyStone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.MossyStone>());
    }
}
