using CalamityMod.Items.BaseItems;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    [AutoloadEquip(EquipType.Head)]

    //A lot of legacy names that's for sure. A combo of the pre "WulfrumHeadX" names, and the aforementionned "WulfrumHeadX" names.
    //This is done so that non summoners don't end up with a helmet they don't really care about anyways, and is a cute reference to the old look.
    [LegacyName("WulfrumMask")]
    [LegacyName("WulfrumHeadRogue")]
    [LegacyName("WulfrumHeadgear")]
    [LegacyName("WulfrumHeadRanged")]
    [LegacyName("WulfrumHelm")]
    [LegacyName("WulfrumHeadMelee")]
    [LegacyName("WulfrumHood")]
    [LegacyName("WulfrumHeadMagic")]
    public class AbandonedWulfrumHelmet : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "AbandonedWulfrumHelmetTrans", "WulfrumOldSetHead"),
            (EquipType.Body, "AbandonedWulfrumHelmet", null),
            (EquipType.Legs, "AbandonedWulfrumHelmet", null),
            (EquipType.Face, null, null), //results in setting this equip slot to -1
        ];
        public override bool ShouldHideAccessories => true;

        public override (SoundStyle sound, int delay)? HurtSound(Player p) => (SoundID.NPCHit4, 10);

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 30;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.vanity = true;
        }

        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<WulfrumTransformationPlayer>().vanityEquipped = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
            {
                player.GetModPlayer<WulfrumTransformationPlayer>().vanityEquipped = true;
            }
        }
    }

    public class WulfrumTransformationPlayer : ModPlayer
    {
        public bool vanityEquipped = false;

        public override void ResetEffects()
        {
            vanityEquipped = false;
        }
    }
}
