using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges.Stats;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public class BriskPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Brisk;
        public override string TargetTooltipName => "PrefixAccMoveSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMovementSpeedStat(0.02f);

            if (NPC.downedMoonlord) yield return new PrefixArmorPenStat(3);
            else if (Main.hardMode) yield return new PrefixArmorPenStat(2);
            else yield return new PrefixArmorPenStat(1);
        }
    }

    public class FleetingPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Fleeting;
        public override string TargetTooltipName => "PrefixAccMoveSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMovementSpeedStat(0.02f);
            yield return new PrefixDamageStat(0.02f);
        }
    }

    public class HastyPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Hasty2;
        public override string TargetTooltipName => "PrefixAccMoveSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMovementSpeedStat(0.02f);
            yield return new PrefixMeleeSpeedStat(0.02f);
        }
    }

    public class QuickPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Quick2;
        public override string TargetTooltipName => "PrefixAccMoveSpeed";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixMovementSpeedStat(0.04f);
        }
    }
}
