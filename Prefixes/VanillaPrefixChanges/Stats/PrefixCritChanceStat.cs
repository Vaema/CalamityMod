using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats
{
    public struct PrefixCritChanceStat(int critChanceBonus) : IVanillaPrefixStat
    {
        public int CritChanceBonus = critChanceBonus;

        public readonly void ApplyEffects(Player player)
        {
            player.GetCritChance<GenericDamageClass>() += CritChanceBonus;
        }

        public readonly void ModifyTooltip(TooltipLine line)
        {
            line.Text += "\n" + VanillaPrefixChange.GetCritChanceString(CritChanceBonus);
        }
    }
}
