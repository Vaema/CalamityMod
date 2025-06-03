using CalamityMod.Items.Potions.Alcohol;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Alcohol
{
    public class EvergreenGinBuff : ModBuff
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
            player.Calamity().evergreenGin = true;
            player.Calamity().SicknessDebuffMultiplier += EvergreenGin.DebuffBoost;
            player.Calamity().WaterDebuffMultiplier += EvergreenGin.DebuffBoost;
            player.Calamity().ElectricDebuffMultiplier -= EvergreenGin.DebuffLoss;
            player.Calamity().HeatDebuffMultiplier -= EvergreenGin.DebuffLoss;
        }
    }
}
