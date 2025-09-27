using CalamityMod.NPCs;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class GalvanicCorrosion : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().galvanicCorrosion = true;
            if ((CalamityNPCSets.ResistSlowingDebuffsAndOtherSpecialEffects[npc.type] || npc.boss) && npc.Calamity().debuffResistanceTimer <= 0)
                npc.Calamity().debuffResistanceTimer = CalamityGlobalNPC.slowingDebuffResistanceMin + npc.buffTime[buffIndex];
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().galvanicCorrosion = true;
        }
    }
    public class GalvanicCorrosionIconItem : ModItem
    {
        private string BuffName = "GalvanicCorrosion";
        public override string Texture => $"CalamityMod/Buffs/StatDebuffs/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
