using CalamityMod.CalPlayer;
using CalamityMod.Items.BaseItems;
using CalamityMod.Rarities;
using Terraria;
using Terraria.GameContent.NetModules;
using Terraria.ModLoader;
using Terraria.Net;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class XyksBlessingOrange : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "Xyk2", null),
            (EquipType.Body, "Xyk2", null),
            (EquipType.Legs, "Xyk2", null),
            (EquipType.Wings, null, null), //results in setting this equip slot to -1
        ];

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.accessory = true;
            Item.vanity = true;
            Item.rare = ModContent.RarityType<DarkOrange>();
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.Calamity().devItem = true;
        }
        public override bool CanRightClick() => true;
        public override void RightClick(Player player)
        {
            player.PutItemInInventoryFromItemUsage(ModContent.ItemType<XyksBlessingBlue>(), 1);
        }
        public override void UpdateVanity(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.XykVisualsOrange = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
            {
                CalamityPlayer modPlayer = player.Calamity();
                modPlayer.XykVisualsOrange = true;
            }
        }

        // Also research the other variant
        public override void OnResearched(bool fullyResearched)
        {
            if (fullyResearched)
            {
                if (!Main.ServerSideCharacter)
                {
                    Main.LocalPlayerCreativeTracker.ItemSacrifices.RegisterItemSacrifice(ModContent.ItemType<XyksBlessingBlue>(), 1);
                }
                else
                {
                    NetPacket packet = NetCreativeUnlocksPlayerReportModule.SerializeSacrificeRequest(ModContent.ItemType<XyksBlessingBlue>(), 1);
                    NetManager.Instance.SendToServerOrLoopback(packet);
                }
            }
        }
    }
}
