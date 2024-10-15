using CalamityMod.NPCs;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class TimeDistortion : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.Calamity().timeSlow < npc.buffTime[buffIndex])
                npc.Calamity().timeSlow = npc.buffTime[buffIndex];
            if ((EnemyImmunitiesList.IsNPCImmune(npc) || npc.boss) && npc.Calamity().debuffResistanceTimer <= 0)
                npc.Calamity().debuffResistanceTimer = CalamityGlobalNPC.slowingDebuffResistanceMin + npc.Calamity().timeSlow;
            npc.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
