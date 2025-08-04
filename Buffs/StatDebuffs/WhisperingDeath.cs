using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class WhisperingDeath : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().whisperingDeath = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().whisperingDeath = true;
        }
    }
    public class WhisperingDeathIconItem : ModItem
    {
        private string BuffName = "WhisperingDeath";
        public override string Texture => $"CalamityMod/Buffs/StatDebuffs/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
