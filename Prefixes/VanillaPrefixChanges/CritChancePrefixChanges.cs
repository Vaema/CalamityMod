using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges.Stats;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public class PrecisePrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Precise;
        public override string TargetTooltipName => "PrefixAccCritChance";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            if (NPC.downedMoonlord)
            {
                yield return new PrefixCritChanceStat(0.02f);
                yield return new PrefixArmorPenStat(3);
            }
            else if (Main.hardMode)
            {
                yield return new PrefixCritChanceStat(0.02f);
                yield return new PrefixArmorPenStat(2);
            }
            else
            {
                yield return new PrefixCritChanceStat(0.01f);
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
            yield return new PrefixCritChanceStat(0.04f);
        }

        public override void PostModifyTooltip(TooltipLine line)
        {
            line.Text += "\n" + GetLuckyPrefixAddedTooltip().Value;
        }
    }
}
