using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges.Stats;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public class PrecisePrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Precise;
        public override string TargetTooltipName => "PrefixAccCritChance";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixCritChanceStat(2);

            if (NPC.downedMoonlord)
            {
                yield return new PrefixArmorPenStat(3);
            }
            else if (Main.hardMode)
            {
                yield return new PrefixArmorPenStat(2);
            }
            else
            {
                yield return new PrefixArmorPenStat(1);
            }
        }
    }

    public class LuckyPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Lucky;
        public override string TargetTooltipName => "PrefixAccCritChance";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixCritChanceStat(4);
            yield return new PrefixLuckStat(0.05f);
        }
    }
}
