using CalamityMod.Items.BaseItems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class LucisSight : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Face, "LucisSight", null),
        ];

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.buyPrice(gold: 10); // Sold by Steampunker
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
            Item.vanity = true;
            Item.Calamity().donorItem = true;
        }
    }
}
