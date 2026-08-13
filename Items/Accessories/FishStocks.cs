using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class FishStocks : ModItem, ILocalizedModType, IHoldShiftTooltipItem
{
    public new string LocalizationCategory => "Items.Accessories";
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 38;
        Item.value = Item.buyPrice(gold: 7, silver: 77); // Sold by Shady Salesman
        Item.rare = ItemRarityID.Blue;
        Item.accessory = true;
    }
    
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.Calamity().fishStocks = true;
    }
}
