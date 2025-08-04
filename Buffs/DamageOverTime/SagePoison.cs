using CalamityMod.DataStructures;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class SagePoison : ModBuff
    {
        public static DebuffData debuffData = new DebuffData()
        {
            EnemyLostRegen = 50,
            NPCLifeRegenMethod = SagePoisonPower
        };
        public static void SagePoisonPower(NPC npc, int buffType, ref int buffIndex, ref int damage)
        {
            // The dot ia multipliplied by this before being applied:
            // (float)(Math.Pow(totalSageSpirits, 0.9D) + Math.Pow(totalSageSpirits, 1.13D)) * 0.5f;
            // See SageNeedle.cs for details
            int baseSagePoisonDoTValue = (int)npc.Calamity().ActiveTypelessDebuffMultiplier.ApplyTo(npc.Calamity().sagePoisonDamage);
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
    public class SagePoisonIconItem : ModItem
    {
        private string BuffName = "SagePoison";
        public override string Texture => $"CalamityMod/Buffs/DamageOverTime/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
