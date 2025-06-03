using CalamityMod.Items.Potions.Alcohol;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Alcohol
{
    public class VodkaBuff : ModBuff
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
            player.Calamity().vodka = true;
            player.Calamity().TypelessDebuffMultiplier += Vodka.DebuffBoost;
            player.Calamity().HeatDebuffMultiplier -= Vodka.DebuffLoss;
            player.Calamity().ColdDebuffMultiplier -= Vodka.DebuffLoss;
            player.Calamity().SicknessDebuffMultiplier -= Vodka.DebuffLoss;
            player.Calamity().WaterDebuffMultiplier -= Vodka.DebuffLoss;
            player.Calamity().ElectricDebuffMultiplier -= Vodka.DebuffLoss;
        }
    }
}
