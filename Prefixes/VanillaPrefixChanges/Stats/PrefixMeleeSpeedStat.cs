using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats;

public struct PrefixMeleeSpeedStat(float meleeSpeedBonus) : IVanillaPrefixStat
{
    public float MeleeSpeedBonus = meleeSpeedBonus;

    public readonly void ApplyEffects(Player player)
    {
        player.GetAttackSpeed<MeleeDamageClass>() += MeleeSpeedBonus;
    }

    public readonly void ModifyTooltip(TooltipLine line)
    {
        line.Text += "\n" + VanillaPrefixChange.GetMeleeSpeedString(MeleeSpeedBonus);
    }
}
