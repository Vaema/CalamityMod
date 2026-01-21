using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class IceShieldBuff : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs(AquaticHeart.IceShieldDamageReductionBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().aquaticHeartIce = true;
        }
    }
}
