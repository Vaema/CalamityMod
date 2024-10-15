using CalamityMod.NPCs;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class KamiFlu : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.Calamity().kamiFlu < npc.buffTime[buffIndex])
                npc.Calamity().kamiFlu = npc.buffTime[buffIndex];
            if ((EnemyImmunitiesList.IsNPCImmune(npc) || npc.boss) && npc.Calamity().debuffResistanceTimer <= 0)
                npc.Calamity().debuffResistanceTimer = CalamityGlobalNPC.slowingDebuffResistanceMin + npc.Calamity().kamiFlu;
            npc.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
