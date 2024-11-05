using CalamityMod.Items.Weapons.Typeless;
using Terraria;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class KamiBuff : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs(YanmeisKnife.RunSpeedBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().kamiBoost = true;
            if (player.buffTime[buffIndex] == 1)
                SoundEngine.PlaySound(YanmeisKnife.ExpireSound, player.Center);
        }
    }
}
