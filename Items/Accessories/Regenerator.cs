using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("Regenator")]
    public class Regenerator : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static float HealthRatioCap = 0.5f;
        public static int FramesPerHeal = 8;
        public static int RegenTimeBoost = 4; // on top of +1/s regular increment
        public static float RegenToDamageRatio = 0.015f; // per regen point
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(HealthRatioCap.ToPercent(), (60 / (float)FramesPerHeal).Round());

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
