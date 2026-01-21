using System.Collections.Generic;
using CalamityMod.Prefixes.VanillaPrefixChanges.Stats;
using Terraria.ID;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public class ArcanePrefixChange : VanillaPrefixChange
    {
        public override int TargetPrefix => PrefixID.Arcane;
        public override string TargetTooltipName => "PrefixAccMaxMana";

        public override IEnumerator<IVanillaPrefixStat> PopulateStats()
        {
            yield return new PrefixArcaneStat(maxManaBonus: 20, magicDamageBonus: 0.02f, manaCostReductionBonus: 0.02f);
        }
    }
}
