using CalamityMod.DataStructures;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class SagePoison : ModBuff
    {
        public static DebuffData debuffData = new DebuffData()
        {
            EnemyLostRegen = 50, //This is the base DoT before the scaling formula. Not used in the method, here for referece.
            SicknessDebuffScaling = 1, //Unused in the method, but kept so other things can know this is a sickness debuff
            NPCLifeRegenMethod = SagePoisonPower
        };
        public static void SagePoisonPower(NPC npc, int buffType, ref int buffIndex, ref int damage)
        {
            // npc.Calamity().sagePoisonDamage = 50 * (float)(Math.Pow(totalSageSpirits, 0.73D) + Math.Pow(totalSageSpirits, 1.1D)) * 0.5f
            // See SageNeedle.cs for details
            int baseSagePoisonDoTValue = (int)(npc.Calamity().sagePoisonDamage * npc.Calamity().ActiveSicknessDebuffMultiplier);
            npc.Calamity().ApplyDPSDebuff(baseSagePoisonDoTValue, baseSagePoisonDoTValue / 5, ref npc.lifeRegen, ref damage);
        }
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().sagePoison = true;
        }
    }
}
