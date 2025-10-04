using CalamityMod.NPCs;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class TemporalSadness : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().temporalSadness = true;
            if ((CalamityNPCSets.ResistSlowingDebuffsAndOtherSpecialEffects[npc.type] || npc.boss) && npc.Calamity().debuffResistanceTimer <= 0)
                npc.Calamity().debuffResistanceTimer = CalamityGlobalNPC.slowingDebuffResistanceMin + npc.buffTime[buffIndex];
        }
    }
}
