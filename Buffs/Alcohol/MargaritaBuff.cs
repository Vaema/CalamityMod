using CalamityMod.Items.Potions.Alcohol;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Alcohol
{
    public class MargaritaBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().margarita = true;
            player.Calamity().HeatDebuffMultiplier -= Margarita.DebuffLoss;
            player.Calamity().SicknessDebuffMultiplier -= Margarita.DebuffLoss;
            player.Calamity().ColdDebuffMultiplier -= Margarita.DebuffLoss;
            player.Calamity().WaterDebuffMultiplier -= Margarita.DebuffLoss;
            player.Calamity().ElectricDebuffMultiplier -= Margarita.DebuffLoss;
            player.Calamity().TypelessDebuffMultiplier -= Margarita.DebuffLoss;
        }
    }
}
