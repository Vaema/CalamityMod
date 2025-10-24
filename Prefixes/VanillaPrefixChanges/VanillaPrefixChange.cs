
using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges.Stats;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public abstract class VanillaPrefixChange
    {
        public abstract int TargetPrefix { get; }
        public abstract string TargetTooltipName { get; }

        public const float RarityPlusTwo = 1.2f + 0.001f; // Precision Errors...
        public const float RarityPlusOne = 1.05f + 0.001f;
        public const float RarityKeep = 1.0f;
        public const float RarityMinusOne = 0.95f - 0.001f;
        public const float RarityMinusTwo = 0.8f - 0.001f;
        public const float RarityPlusOneButClosestToTierTwo = 1.199f;

        public abstract IEnumerator<IVanillaPrefixStat> PopulateStats();

        public virtual void ModifyValue(ref float valueMult)
        {
            valueMult = RarityPlusOneButClosestToTierTwo;
        }

        public virtual void PostApplyEffects(Player player) { }
        public virtual void PostModifyTooltip(TooltipLine line) { }

        public static LocalizedText GetStatTooltip(string key) => CalamityUtils.GetText($"Vanilla.PrefixTooltip.{key}");
        public static LocalizedText GetLuckyPrefixAddedTooltip() => CalamityUtils.GetText($"Vanilla.AddedTooltip.LuckyPrefix");

        public static string GetArcaneBuffString(int maxManaBonus, float magicDamageBonus, float manaCostReductionBonus)
        {
            return GetStatTooltip("ArcaneBuffStat").Format(maxManaBonus, magicDamageBonus.ToPercent(), manaCostReductionBonus.ToPercent());
        }

        public static string GetArmorPenString(int armorPen)
        {
            return GetStatTooltip("ArmorPenStat").Format(armorPen);
        }

        public static string GetCritChanceString(float critP)
        {
            return GetStatTooltip("CritStat").Format(critP.ToPercent());
        }

        public static string GetDamageString(float damageP)
        {
            return GetStatTooltip("DamageStat").Format(damageP.ToPercent());
        }

        public static string GetDefenseString(int defense)
        {
            return GetStatTooltip("DefenseStat").Format(defense);
        }

        public static string GetDamageReductionString(float DR)
        {
            return GetStatTooltip("DRStat").Format(DR.ToPercent());
        }

        public static string GetLuckString(float luckP)
        {
            return GetStatTooltip("LuckStat").Format(luckP.ToPercent());
        }

        public static string GetMeleeSpeedString(float meleeSpeedP)
        {
            return GetStatTooltip("MeleeSpeedStat").Format(meleeSpeedP.ToPercent());
        }

        public static string GetMoveSpeedString(float speedP)
        {
            return GetStatTooltip("SpeedStat").Format(speedP.ToPercent());
        }
    }
}
