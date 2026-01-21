using CalamityMod.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Body)]
    public class AncientGodSlayerChestplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Vanity";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.vanity = true;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }
    }
}
