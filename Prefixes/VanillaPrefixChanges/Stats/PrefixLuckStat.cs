using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats;

public struct PrefixLuckStat(float luckBonus) : IVanillaPrefixStat
{
    public float LuckBonus = luckBonus;

    public readonly void ApplyEffects(Player player)
    {
        player.Calamity().calamityBonusLuck += LuckBonus;
    }

    public readonly void ModifyTooltip(TooltipLine line)
    {
        line.Text += "\n" + VanillaPrefixChange.GetLuckString(LuckBonus);
    }
}
