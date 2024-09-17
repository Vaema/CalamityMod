using CalamityMod.Tiles.Furniture.Fountains;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture.Fountains
{
    public class AstralFountainItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AstralFountainTile>());
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.Blue;
        }
    }
}
