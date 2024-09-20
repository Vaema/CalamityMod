using CalamityMod.Tiles.Furniture;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class ForgottenScholarStatue : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ForgottenScholarStatueTile>());
            Item.width = 56;
            Item.height = 96;
        }
    }
}
