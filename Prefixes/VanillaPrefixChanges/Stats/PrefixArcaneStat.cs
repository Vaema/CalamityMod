using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats;

public struct PrefixArcaneStat(int maxManaBonus, float magicDamageBonus, float manaCostReductionBonus) : IVanillaPrefixStat
{
    public int MaxManaBonus = maxManaBonus;
    public float MagicDamageBonus = magicDamageBonus;
    public float ManaCostReductionBonus = manaCostReductionBonus;

    public readonly void ApplyEffects(Player player)
    {
        player.statManaMax2 += MaxManaBonus;
        player.GetDamage<MagicDamageClass>() += MagicDamageBonus;
        player.manaCost -= ManaCostReductionBonus;
    }

    public readonly void ModifyTooltip(TooltipLine line)
    {
        line.Text += "\n" + VanillaPrefixChange.GetArcaneBuffString(MaxManaBonus, MagicDamageBonus, ManaCostReductionBonus);
    }
}
