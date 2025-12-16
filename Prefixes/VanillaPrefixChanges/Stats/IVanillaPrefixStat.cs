using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges.Stats
{
    public interface IVanillaPrefixStat
    {
        public abstract void ApplyEffects(Player player);
        public abstract void ModifyTooltip(TooltipLine line);
    }
}
