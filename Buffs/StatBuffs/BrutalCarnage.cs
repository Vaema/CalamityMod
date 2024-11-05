using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class BrutalCarnage : ModBuff
    {
        public static float MeleeDamageBoost = 0.2f;
        public override LocalizedText Description => base.Description.WithFormatArgs(MeleeDamageBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().brutalCarnage = true;
        }
    }
}
