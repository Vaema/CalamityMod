using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class CrystalHeartVodka : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Head", EquipType.Head, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Body", EquipType.Body, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Legs", EquipType.Legs, this);
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Accessories/Vanity/Fabsol_Back", EquipType.Back, this);
            }
        }

        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
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
            
            // Same as Fabsol's Vodka, due to the obtainment method being shimmering Fabsol's Vodka.
            Item.value = Item.buyPrice(0, 2, 60, 0);
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
    }
}
