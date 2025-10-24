using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges.Stats;
using Terraria.ID;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public class WildPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Wild;
        public override string TargetTooltipName => "PrefixAccMeleeSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMeleeSpeedStat(0.02f);
            yield return new PrefixDamageStat(0.02f);
        }
    }

    public class RashPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Rash;
        public override string TargetTooltipName => "PrefixAccMeleeSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMeleeSpeedStat(0.02f);
            yield return new PrefixCritChanceStat(0.02f);
        }
    }

    public class IntrepidPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Intrepid;
        public override string TargetTooltipName => "PrefixAccMeleeSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMovementSpeedStat(0.02f);
            yield return new PrefixCritChanceStat(0.02f);
        }
    }

    public class ViolentPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Violent;
        public override string TargetTooltipName => "PrefixAccMeleeSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMeleeSpeedStat(0.04f);
        }
    }
}
