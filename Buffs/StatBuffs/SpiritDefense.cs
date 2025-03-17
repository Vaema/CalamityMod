using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class SpiritDefense : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs(SpiritGlyph.DefenseBoost, SpiritGlyph.DamageReductionBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += SpiritGlyph.DefenseBoost;
            player.endurance += SpiritGlyph.DamageReductionBoost; // TODO -- is this applied too late to be affected by the DR softcap?
        }
    }
}
