using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats
{
    public struct PrefixDamageStat(float damageBonus) : IVanillaPrefixStat
    {
        public float DamageBonus = damageBonus;

        public readonly void ApplyEffects(Player player)
        {
            player.GetDamage<GenericDamageClass>() += DamageBonus;
        }

        public readonly void ModifyTooltip(TooltipLine line)
        {
            line.Text += "\n" + VanillaPrefixChange.GetDamageString(DamageBonus);
        }
    }
}
