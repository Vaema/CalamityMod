using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class AbyssalDivingSuitPlates : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            tip = base.Description.Format((AbyssalDivingSuit.PlatesAllDamageReduction - Main.player[Main.myPlayer].Calamity().abyssalDivingSuitPlateHits * AbyssalDivingSuit.PlatesHitDecay).ToPercent());
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().abyssalDivingSuitPlates = true;
        }
    }
}
