using CalamityMod.Items.Weapons.Magic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs;

public class SandsWindBuff : ModBuff
{
    public override LocalizedText Description => base.Description.WithFormatArgs(PrimordialEarth.BuffDefenseBoost, PrimordialEarth.BuffDamageBoost.ToPercent());

    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = false;
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Calamity().sandsWindBuff = true;
    }
}
