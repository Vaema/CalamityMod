using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Face)]
    public class Abaddon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int CritBoost = 8;
        public static float BrimstoneFlamesReduction = 0.5f;
        public static int AbaddonExploDamage = 25;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(CritBoost, BrimstoneFlamesReduction.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().abaddon = true;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
        }
    }
}
