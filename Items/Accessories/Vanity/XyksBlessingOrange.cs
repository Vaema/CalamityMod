using System;
using CalamityMod.CalPlayer;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Rarities;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class XyksBlessingOrange : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void Load()
        {
            if (!Main.dedServ)
            {
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/XYK_Head", EquipType.Head, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/XYK_Body", EquipType.Body, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/XYK_Legs", EquipType.Legs, this);
            }
        }

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = true;

            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = false;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;

            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.accessory = true;
            Item.consumable = true;
            Item.vanity = true;
            Item.rare = ModContent.RarityType<DarkOrange>();
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.Calamity().devItem = true;
        }
        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ModContent.ItemType<XyksBlessingBlue>());
        }
        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<XyksBlessingOrangePlayer>().vanityEquipped = true;
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.XykVisualsOrange = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
            {
                player.GetModPlayer<XyksBlessingOrangePlayer>().vanityEquipped = true;
                CalamityPlayer modPlayer = player.Calamity();
                modPlayer.XykVisualsOrange = true;
            }
        }
    }

    public class XyksBlessingOrangePlayer : ModPlayer
    {
        public bool vanityEquipped = false;

        public override void ResetEffects()
        {
            vanityEquipped = false;
        }

        public override void FrameEffects()
        {
            if (vanityEquipped)
            {
                Player.legs = EquipLoader.GetEquipSlot(Mod, "XyksBlessingOrange", EquipType.Legs);
                Player.body = EquipLoader.GetEquipSlot(Mod, "XyksBlessingOrange", EquipType.Body);
                //Player.head = EquipLoader.GetEquipSlot(Mod, "XyksBlessingOrange", EquipType.Head);
            }
        }
    }
}
