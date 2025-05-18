using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea.WhisperingPearls
{
    public class WhisperingPearlLightBlue : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.WhisperingPearl>());
            Item.rare = ItemRarityID.Blue;
            Item.placeStyle = 5;
        }
    }
}
