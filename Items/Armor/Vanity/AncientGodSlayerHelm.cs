using CalamityMod.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity;

[AutoloadEquip(EquipType.Head)]
public class AncientGodSlayerHelm : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Vanity";
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.vanity = true;
        Item.value = Item.sellPrice(gold: 15);
        Item.rare = ModContent.RarityType<CosmicPurple>();
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<AncientGodSlayerChestplate>() && legs.type == ModContent.ItemType<AncientGodSlayerLeggings>();
    }

    public override void ArmorSetShadows(Player player)
    {
        player.armorEffectDrawShadow = true;
    }
}
