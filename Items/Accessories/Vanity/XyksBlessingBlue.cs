using CalamityMod.CalPlayer;
using CalamityMod.Items.BaseItems;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class XyksBlessingBlue : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "Xyk", null),
            (EquipType.Body, "Xyk", null),
            (EquipType.Legs, "Xyk", null),
            (EquipType.Wings, null, null), //results in setting this equip slot to -1
        ];

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.accessory = true;
            Item.vanity = true;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.Calamity().devItem = true;
        }
        public override bool CanRightClick() => true;
        public override void RightClick(Player player)
        {
            player.PutItemInInventoryFromItemUsage(ModContent.ItemType<XyksBlessingOrange>(), 1);
        }
        public override void UpdateVanity(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.XykVisualsBlue = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
            {
                CalamityPlayer modPlayer = player.Calamity();
                modPlayer.XykVisualsBlue = true;
            }
        }
    }
}
