using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class RottenDogtooth : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";
    internal const int ArmorCrunchDebuffTime = 150;
    public override void SetDefaults()
    {
        Item.width = 14;
        Item.height = 22;
        Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
        Item.rare = ItemRarityID.Blue;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.Calamity().rottenDogTooth = true;
    }
}
