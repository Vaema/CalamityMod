using CalamityMod.Items.BaseItems;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class GoldenBomb : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "Mihaii", null),
            (EquipType.Body, "Mihaii", null),
            (EquipType.Legs, "Mihaii", null),
        ];

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.vanity = true;
            Item.Calamity().devItem = true;
        }
    }
}
