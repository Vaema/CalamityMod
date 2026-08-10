using CalamityMod.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture;

public class LanternCenter : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<LanternCenterTile>());
        Item.value = Item.buyPrice(gold: 20); //Same price as Party Center; Sold by Princess
        Item.rare = ItemRarityID.Orange;
    }
}
