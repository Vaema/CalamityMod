using CalamityMod.DataStructures;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class MarkedforDeath : ModBuff
    {
        public static int DefenseReduction = 5;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().markedForDeath = true;
        }
    }
    public class MarkedforDeathIconItem : ModItem
    {
        private string BuffName = "MarkedforDeath";
        public override string Texture => $"CalamityMod/Buffs/StatDebuffs/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
