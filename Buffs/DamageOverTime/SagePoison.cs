using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.DataStructures;
using CalamityMod.NPCs;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class SagePoison : ModBuff
    {
        public static DebuffData debuffData = new DebuffData()
        {
            EnemyLostRegen = 50,
            SicknessDebuffScaling = 1,
            NPCLifeRegenMethod = SagePoisonPower
        };
        public static void SagePoisonPower(NPC npc, int buffType, ref int buffIndex, ref int damage)
        {
            // The base DoT has this formula used when it's applied
            // (float)(Math.Pow(totalSageSpirits, 0.9D) + Math.Pow(totalSageSpirits, 1.13D)) * 0.5f;
            // See SageNeedle.cs for details

            //Reduce power of weakness/resistances but leave sickness debuff boosters at full power
            //We first need to apply Irradiated as that isn't included in SicknessDebuffMultiplier due to needing to dynamically update if Irradiated is applied
            StatModifier multiplier = npc.Calamity().SicknessDebuffMultiplier;
            if (npc.Calamity().irradiated)
                multiplier += npc.Calamity().scionsCurioEffected ? 1.75f : 1f;

            bool wormBoss = CalamityNPCTypeSets.DesertScourge.Contains(npc.type) || CalamityNPCTypeSets.EaterOfWorlds.Contains(npc.type) || CalamityNPCTypeSets.Perforators.Contains(npc.type) ||
                CalamityNPCTypeSets.AquaticScourge.Contains(npc.type) || CalamityNPCTypeSets.AstrumDeus.Contains(npc.type) || CalamityNPCTypeSets.StormWeaver.Contains(npc.type);
            float NewWeaknessEffectiveness = 1.25f; //1.25x DPS instead of 2x - 1/4th the effectiveness
            float NewWeaknessEffectivenessWorm = 1.125f; //1.125x DPS instead of 1.5x - 1/4th the effectiveness
            float NewResistanceEffectiveness = 0.875f; // 0.875x instead of 0.5x - 1/4th the effectiveness

            if (npc.Calamity().VulnerableToSickness.HasValue)
                if (npc.Calamity().VulnerableToSickness.Value)
                    multiplier *= (wormBoss ? NewWeaknessEffectivenessWorm : NewWeaknessEffectiveness);
                else
                    multiplier *= NewResistanceEffectiveness;

            //Apply the DOT
            int baseSagePoisonDoTValue = (int)multiplier.ApplyTo(npc.Calamity().sagePoisonDamage);
            Main.NewText(baseSagePoisonDoTValue);
            npc.Calamity().ApplyDPSDebuff(baseSagePoisonDoTValue, baseSagePoisonDoTValue / 5, ref npc.lifeRegen, ref damage);
        }
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffDatasets.DebuffDataset[Type] = debuffData;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().sagePoison = true;
        }
    }
}
