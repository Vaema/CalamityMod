using CalamityMod.DataStructures;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    [LegacyName("CragsLava")]
    public class SearingLava : ModBuff
    {
        public static DebuffData debuffData = new DebuffData()
        {
            EnemyLostRegen = 1000, //Never infliced on enemies, token value. Really high bc this hurts a lot on players
            HeatDebuffScaling = 1
        };
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffDatasets.DebuffDataset[Type] = debuffData;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().searingLava = true;
        }
    }
}
