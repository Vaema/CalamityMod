using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using System.Collections.Generic;
using System;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("Regenator")]
    public class Regenerator : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 56;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.regenerator = true;
            player.longInvince = true; // Gives better iframes because god help you if you end up getting hit twice
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (Main.LocalPlayer != null)
                list.FindAndReplace("[DAMAGE]", (Main.LocalPlayer.Calamity().regeneratorDamage).ToPercent());
        }
    }
}
