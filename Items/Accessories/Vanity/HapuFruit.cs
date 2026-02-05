using CalamityMod.Items.BaseItems;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class HapuFruit : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "Heart", null),
            (EquipType.Body, "Heart", null),
            (EquipType.Legs, "Heart", null),
            (EquipType.Back, "Heart", null)
        ];

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 30;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.vanity = true;
            Item.Calamity().devItem = true;
        }
    }
}
