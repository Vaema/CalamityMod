using CalamityMod.Items.BaseItems;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class OracleHeadphones : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "Mishiro", null),
            (EquipType.Body, "Mishiro", null),
            (EquipType.Legs, "Mishiro", null),
            (EquipType.Back, "Mishiro", null)
        ];

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 30;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.vanity = true;
            Item.Calamity().devItem = true;
        }
    }
}
