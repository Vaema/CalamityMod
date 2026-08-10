using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats;

public struct PrefixArmorPenStat(int armorPenBonus) : IVanillaPrefixStat
{
    public int ArmorPenBonus = armorPenBonus;

    public readonly void ApplyEffects(Player player)
    {
        player.GetArmorPenetration<GenericDamageClass>() += ArmorPenBonus;
    }

    public readonly void ModifyTooltip(TooltipLine line)
    {
        line.Text += "\n" + VanillaPrefixChange.GetArmorPenString(ArmorPenBonus);
    }
}
