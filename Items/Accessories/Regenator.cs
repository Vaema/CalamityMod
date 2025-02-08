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
    public class Regenator : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public float regenatorDamageBoost = 0;
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
            modPlayer.regenator = true;
            player.longInvince = true; // Gives better iframes because god help you if you end up getting hit twice


            modPlayer.regenatorDamage = (player.lifeRegenCount * 1.75f) * 0.01f;
            regenatorDamageBoost = MathHelper.Lerp(regenatorDamageBoost, modPlayer.regenatorDamage, 0.1f);

            player.GetDamage<GenericDamageClass>() += regenatorDamageBoost;

            player.nebulaLevelLife = 0;

            if (player.lifeRegenCount > 0)
                player.lifeRegenCount = 0;

            // Boost life regen time quite a bit.
            // This is so that in events and such where small hits are common, your damage boost isn't completley negated
            if (player.lifeRegenTime < 3600)
                player.lifeRegenTime += 10;

            //Hard-lock the player's health to 50%.
            //No lifesteal, no regen, no healing pots
            if (player.statLife >= (int)(player.statLifeMax2 * 0.5f))
            {
                player.statLife = (int)(player.statLifeMax2 * 0.5f);
                player.moonLeech = true;
                modPlayer.healingPotionMultiplier = 0;

                if (player.lifeRegenCount > 0)
                    player.lifeRegenCount = 0;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            list.FindAndReplace("[DAMAGE]", Math.Round(regenatorDamageBoost, 3).ToPercent().ToString());
        }
    }
}
