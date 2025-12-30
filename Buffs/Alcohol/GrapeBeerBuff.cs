using CalamityMod.Items.Potions.Alcohol;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Alcohol
{
    public class GrapeBeerBuff : ModBuff
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
            var cplayer = player.Calamity();
            cplayer.grapeBeer = true;
            cplayer.critDamage -= GrapeBeer.CritLoss * 0.01f;
        }
    }
}
