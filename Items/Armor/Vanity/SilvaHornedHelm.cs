using CalamityMod.Items.Armor.Silva;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class SilvaHornedHelm : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Vanity";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;
            Item.vanity = true;
            Item.value = Item.buyPrice(gold: 10); // Sold by Clothier
            Item.rare = ItemRarityID.Blue;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<SilvaArmor>() && legs.type == ModContent.ItemType<SilvaLeggings>();

        public override void ArmorSetShadows(Player player) => player.armorEffectDrawShadow = true;
    }
}
