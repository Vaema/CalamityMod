using CalamityMod.Tiles.Furniture.Fountains;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture.Fountains;

public class SunkenSeaFountain : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<SunkenSeaFountainTile>());
        Item.value = Item.buyPrice(gold: 4); // Sold by Witch Doctor
        Item.rare = ItemRarityID.Blue;
    }
}
