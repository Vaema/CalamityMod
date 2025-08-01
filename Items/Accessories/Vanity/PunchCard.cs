using CalamityMod.Items.BaseItems;
using CalamityMod.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class PunchCard : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "Shayy", null),
            (EquipType.Body, "Shayy", null),
            (EquipType.Legs, "Shayy", null),
        ];

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 38;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.vanity = true;
            Item.Calamity().devItem = true;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                PopupGUIManager.FlipActivityOfGUIWithType(typeof(PunchCardGUI));
            return true;
        }
    }
}
