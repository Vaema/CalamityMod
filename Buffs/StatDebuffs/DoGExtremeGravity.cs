using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class DoGExtremeGravity : ModBuff
    {
        public static int MaxFlightTimeCap = 400;
        public static float FlightTimeLossPercent = 0.25f;
        public override LocalizedText Description => base.Description.WithFormatArgs(FlightTimeLossPercent.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().DoGExtremeGravity = true;
        }
    }
}
