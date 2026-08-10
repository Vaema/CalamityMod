using CalamityMod.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture;

public class EffigyOfDecay : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<EffigyOfDecayPlaceable>());
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;
    }
}
