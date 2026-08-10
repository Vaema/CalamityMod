using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity;

[AutoloadEquip(EquipType.Legs)]
public class LucisBoots : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Vanity";

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.value = Item.buyPrice(gold: 10); // Sold by Steampunker
        Item.rare = ItemRarityID.Pink;
        Item.vanity = true;
        Item.Calamity().donorItem = true;
    }
}
