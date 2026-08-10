using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs;

public class AquaticHeartWaterSpeed : ModBuff
{
    public override LocalizedText Description => base.Description.WithFormatArgs(AquaticHeart.WaterSpeedBoost.ToPercent());

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Calamity().aquaticHeartWaterBuff = true;
    }
}
