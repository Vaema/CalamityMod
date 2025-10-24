using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges.Stats;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public class JaggedPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Jagged;
        public override string TargetTooltipName => "PrefixAccDamage";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixDamageStat(0.02f);

            if (NPC.downedMoonlord) yield return new PrefixArmorPenStat(3);
            else if (Main.hardMode) yield return new PrefixArmorPenStat(2);
            else yield return new PrefixArmorPenStat(1);
        }
    }

    public class SpikedPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Spiked;
        public override string TargetTooltipName => "PrefixAccDamage";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixDamageStat(0.02f);
            yield return new PrefixDefenseStat(2);
        }
    }

    public class AngryPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Angry;
        public override string TargetTooltipName => "PrefixAccDamage";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixDamageStat(0.02f);
            yield return new PrefixCritChanceStat(0.02f);
        }
    }

    public class MenacingPrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Menacing;
        public override string TargetTooltipName => "PrefixAccDamage";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixDamageStat(0.04f);
        }
    }
}
