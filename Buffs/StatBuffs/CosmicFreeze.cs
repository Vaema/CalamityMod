using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class CosmicFreeze : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().cFreeze = true;
        }
    }
    public class CosmicFreezeIconItem : ModItem
    {
        private string BuffName = "CosmicFreeze";
        public override string Texture => $"CalamityMod/Buffs/StatBuffs/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
