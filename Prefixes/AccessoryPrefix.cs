using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes
{
    public class Invigorating : ModPrefix, ILocalizedModType
    {
        public new string LocalizationCategory => "Prefixes.Accessory";
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override void ApplyAccessoryEffects(Player player)
        {
            player.lifeRegen += GetLifeRegenAmount();
        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult = VanillaPrefixChange.RarityPlusOneButClosestToTierTwo;
        }

        public LocalizedText LifeRegenTooltip => CalamityUtils.GetText($"{LocalizationCategory}.LifeRegenTooltip");
        public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
        {
            var perSecHP = (GetLifeRegenAmount() / 2.0f).ToString("0.#");
            yield return new TooltipLine(Mod, "CalamityMod:PrefixLifeRegenBoost", LifeRegenTooltip.Format(perSecHP))
            {
                IsModifier = true
            };
        }

        public static int GetLifeRegenAmount()
        {
            return 2;
        }
    }

    public class Dauntless : ModPrefix, ILocalizedModType
    {
        public new string LocalizationCategory => "Prefixes.Accessory";
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override void ApplyAccessoryEffects(Player player)
        {
            player.statLifeMax2 += GetHealthBoostAmount();
        }

        public override void ModifyValue(ref float valueMult)
        {
            valueMult = VanillaPrefixChange.RarityPlusOneButClosestToTierTwo;
        }

        public LocalizedText LifeBoostTooltip => CalamityUtils.GetText($"{LocalizationCategory}.MaxLifeBoostTooltip");
        public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
        {
            yield return new TooltipLine(Mod, "CalamityMod:PrefixMaxLifeBoost", LifeBoostTooltip.Format(GetHealthBoostAmount()))
            {
                IsModifier = true
            };
        }

        public static int GetHealthBoostAmount()
        {
            if (DownedBossSystem.downedDoG) return 30;
            if (NPC.downedMoonlord) return 25;
            if (NPC.downedPlantBoss) return 20;
            if (Main.hardMode) return 15;
            return 10;
        }
    }
}
