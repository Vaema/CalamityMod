using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class LifeJelly : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int AuraLifetime = 1800;
        public static int AuraRegenBoost = 4;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AuraLifetime.FramesToSeconds(), AuraRegenBoost.ToRegenPerSecond());

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 40;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.lifejelly = true;
        }
    }
}
