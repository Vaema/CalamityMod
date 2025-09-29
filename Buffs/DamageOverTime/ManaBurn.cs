using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class ManaBurn : ModBuff
    {
        //as of now, this doesn't use the custom debuff system due to the debuff being a front for the actual effects in the Chaos Stone rework
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().ManaBurn = true;
        }
    }
    public class ManaBurnIconItem : ModItem
    {
        private string BuffName = "ManaBurn";
        public override string Texture => $"CalamityMod/Buffs/DamageOverTime/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
