using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs;

public class GreenJellyRegen : ModBuff
{
    public override LocalizedText Description => base.Description.WithFormatArgs(GrandGelatin.AuraRegenBoost.ToRegenPerSecond());

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Calamity().GreenJellyRegen = true;
    }
}
