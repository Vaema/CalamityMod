using CalamityMod.Items.Armor.Reaver;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class ReaverRage : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs(ReaverHeadTank.ReaverRageDefenseBoost, ReaverHeadTank.ReaverRageDamageBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().rRage = true;
        }
    }
    public class ReaverRageIconItem : ModItem
    {
        private string BuffName = "ReaverRage";
        public override string Texture => $"CalamityMod/Buffs/StatBuffs/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
