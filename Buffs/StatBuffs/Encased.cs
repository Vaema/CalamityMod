using CalamityMod.Balancing;
using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class Encased : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().encased = true;
            if (player.buffTime[buffIndex] == 2)
            {
                SoundEngine.PlaySound(SoundID.Item27, player.Center);

                // 17APR2024: Ozzatron: Permafrost's Concoction gives true invulnerability to everything and is boosted by Cross Necklace.
                int encasedIFrames = PermafrostsConcoction.EncasedIFrames + (player.longInvince ? BalancingConstants.CrossNecklaceIFrameBoost : 0);
                player.GiveUniversalIFrames(encasedIFrames, true);
            }
        }
    }
}
