using CalamityMod.Items.BaseItems;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class SharkyPlush : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "Shark", null),
            (EquipType.Body, "Shark", null),
            (EquipType.Legs, "Shark", null),
        ];

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 24;
            Item.accessory = true;
            Item.vanity = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.Calamity().devItem = true;
        }
    }
}
