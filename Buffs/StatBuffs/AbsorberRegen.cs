using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class AbsorberRegen : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs(TheAbsorber.AuraRegenBoost.ToRegenPerSecond(), TheAbsorber.AuraDamageBoost.ToPercent(), TheAbsorber.AuraDamageReductionBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().AbsorberRegen = true;
        }
    }
}
