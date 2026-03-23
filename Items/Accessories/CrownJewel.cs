using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class CrownJewel : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int RegenBoost = 1;
        public static int DebuffedRegenBoost = 3; // Added on top of the baseline regen boost
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond(), DebuffedRegenBoost.ToRegenPerSecond());
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.defense = 1;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.crownJewel = true;
        }
    }
}
