using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats
{
    public struct PrefixDefenseStat(int defenseBonus) : IVanillaPrefixStat
    {
        public int DefenseBonus = defenseBonus;

        public readonly void ApplyEffects(Player player)
        {
            player.statDefense += DefenseBonus;
        }

        public readonly void ModifyTooltip(TooltipLine line)
        {
            line.Text += "\n" + VanillaPrefixChange.GetDefenseString(DefenseBonus);
        }
    }
}
