using System.Collections.Generic;
using CalamityMod.Items.BaseItems;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("SirensHeart")]
    public class AquaticHeart : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static float WaterSpeedBoost = 0.15f;
        public static float IceShieldDamageReductionBoost = 0.2f;
        public static int IceShieldCooldown = CalamityUtils.SecondsToFrames(30);
        public static LocalizedText FullTooltip => CalamityUtils.GetText("Items.Accessories.AquaticHeart.FullTooltip").WithFormatArgs(WaterSpeedBoost.ToPercent(), IceShieldDamageReductionBoost.ToPercent(), IceShieldCooldown.FramesToSeconds());

        public override string AssetPath => "CalamityMod/Items/Accessories/";
        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "AquaticTrans", null),
            (EquipType.Body, "AquaticTrans", null),
            (EquipType.Legs, "AquaticTrans", null),
            (EquipType.Face, null, null), //results in setting this equip slot to -1
        ];

        public override (SoundStyle sound, int delay)? HurtSound(Player p) => (SoundID.FemaleHit, 10);

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) => player.Calamity().aquaticHeart = true;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string statusTooltip = NPC.downedBoss3 ? FullTooltip.ToString() : this.GetLocalizedValue("LockedTooltip");
            tooltips.FindAndReplace("[STATUS]", statusTooltip);
        }
    }
}
