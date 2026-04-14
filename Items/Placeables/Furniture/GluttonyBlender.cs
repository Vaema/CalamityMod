using CalamityMod.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class GluttonyBlender : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override string Texture => "Terraria/Images/Item_2193"; //Placeholder

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GluttonyBlenderTile>());
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 5);
        }
    }
}
