using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("VictideVisage", "VictideHeadRanged")]
    public class VictideCoralTurban : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Vanity";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.vanity = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
        }
    }
}
