using System.Collections.Generic;
using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    public class StarlightFuelCell : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override LocalizedText Tooltip => CalamityUtils.GetText($"{LocalizationCategory}.AdrenalineBoosterTooltip").WithFormatArgs(BalancingConstants.AdrenalineDamagePerBooster.ToPercent(), BalancingConstants.AdrenalineDRPerBooster.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = SoundID.Item122;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Lime;
            Item.SetRevExclusive();
        }

        public static bool HasConsumedBefore(Player player) => player.Calamity().adrenalineBoostTwo;

        public override bool CanUseItem(Player player)
        {
            if (HasConsumedBefore(player))
            {
                // Refuse Text can be added on here
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.itemTime = Item.useTime;
                CalamityPlayer modPlayer = player.Calamity();
                modPlayer.adrenalineBoostTwo = true;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (HasConsumedBefore(Main.LocalPlayer))
                list.AddConsumedTooltip("Tooltip0");
        }
    }
}
