using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class HapuFruit : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void Load()
        {
            if (!Main.dedServ)
            {
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Heart_Head", EquipType.Head, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Heart_Body", EquipType.Body, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Heart_Legs", EquipType.Legs, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Heart_Back", EquipType.Back, this);
            }
        }

        public override void SetStaticDefaults()
        {

            if (Main.dedServ)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;

            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;

            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
        }

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

        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<HapuFruitPlayer>().vanityEquipped = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
            {
                player.GetModPlayer<HapuFruitPlayer>().vanityEquipped = true;
            }
        }
    }

    public class HapuFruitPlayer : ModPlayer
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
                Player.back = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Back);
                Player.legs = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Legs);
                Player.body = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Body);
                Player.head = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Head);

                //Player.HideAccessories();
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (vanityEquipped)
            {
                drawInfo.drawPlayer.back = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Back);
                drawInfo.drawPlayer.legs = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Legs);
                drawInfo.legsGlowMask = -1;
                drawInfo.legsOffset = Vector2.Zero;
                drawInfo.drawPlayer.body = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Body);
                drawInfo.drawPlayer.head = EquipLoader.GetEquipSlot(Mod, "HapuFruit", EquipType.Head);
                drawInfo.headGlowMask = -1;
                drawInfo.helmetOffset = Vector2.Zero;
            }
        }
    }
}
