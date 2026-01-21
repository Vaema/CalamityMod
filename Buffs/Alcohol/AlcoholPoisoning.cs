using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Alcohol
{
    public class AlcoholPoisoning : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            // The nurse technically cannot get rid of this debuff by herself, as it reapplies immediately after if you have alcohol left
            // But she also cleanses excess alcohol if you have Alcohol Poisoning (see CalamityPlayer.PostNurseHeal)
            // Removing this set means she costs 1 debuff worth of cost to remove excess alcohol
            // BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().alcoholPoisoning = true;
        }
    }
}
