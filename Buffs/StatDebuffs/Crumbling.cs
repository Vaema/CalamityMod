using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class Crumbling : ModBuff
    {
        public static int DefenseReduction = 10;
        public static float MultiplicativeDamageReductionPlayer = 0.7f;
        // 8% DR reduction
        public static float MultiplicativeDamageReductionEnemy = 0.92f;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.Calamity().crumble < npc.buffTime[buffIndex])
                npc.Calamity().crumble = npc.buffTime[buffIndex];
            npc.DelBuff(buffIndex);
            buffIndex--;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().crumble = true;
        }
    }
}
