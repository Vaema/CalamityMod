using CalamityMod.Rarities;
using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures;

public class PowerCellFactoryItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    // Animation time parameters are more fitting to be in the tile
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((PowerCellFactory.TotalFrames * PowerCellFactory.AnimationFramerate + PowerCellFactory.BetweenCellDowntime).FramesToSeconds());

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<PowerCellFactory>());
        Item.value = Item.sellPrice(silver: 50);
        Item.rare = ModContent.RarityType<DarkOrange>();
    }
}
