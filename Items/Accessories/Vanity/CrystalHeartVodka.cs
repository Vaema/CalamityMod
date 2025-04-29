using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class CrystalHeartVodka : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void Load()
        {
            if (!Main.dedServ)
            {
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Head", EquipType.Head, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Body", EquipType.Body, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Legs", EquipType.Legs, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Back", EquipType.Back, this);
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
            
            // Same as Cirrus' Vodka, due to the obtainment method being shimmering Cirrus' Vodka.
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;

            Item.vanity = true;
            Item.Calamity().devItem = true;
        }

        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<CrystalHeartVodkaPlayer>().vanityEquipped = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
                player.GetModPlayer<CrystalHeartVodkaPlayer>().vanityEquipped = true;
        }
    }

    public class CrystalHeartVodkaPlayer : ModPlayer
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
                Player.back = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Back);
                Player.legs = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Legs);
                Player.body = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Body);
                Player.head = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Head);

                //Player.HideAccessories();
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (vanityEquipped)
            {
                drawInfo.drawPlayer.back = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Back);
                drawInfo.drawPlayer.legs = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Legs);
                drawInfo.legsGlowMask = -1;
                drawInfo.legsOffset = Vector2.Zero;
                drawInfo.drawPlayer.body = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Body);
                drawInfo.bodyGlowMask = -1;
                drawInfo.drawPlayer.head = EquipLoader.GetEquipSlot(Mod, "CrystalHeartVodka", EquipType.Head);
                drawInfo.headGlowMask = -1;
                drawInfo.helmetOffset = Vector2.Zero;
            }
        }
    }
}
