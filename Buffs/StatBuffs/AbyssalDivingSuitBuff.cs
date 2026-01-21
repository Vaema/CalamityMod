using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class AbyssalDivingSuitBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.abyssalDivingSuitPrevious)
            {
                if (player.IsUnderwater())
                {
                    if (player.maxFallSpeed < 9f)
                        player.maxFallSpeed = 9f;
                    player.gills = true;
                }
                modPlayer.depthCharm = true;
                modPlayer.jellyfishNecklace = true;
                modPlayer.anechoicPlating = true;
                player.arcticDivingGear = true;
                player.accFlipper = true;
                player.accDivingHelm = true;
                player.iceSkate = true;
                if (player.wet)
                {
                    Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0.2f, 0.8f, 0.9f);
                }
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
