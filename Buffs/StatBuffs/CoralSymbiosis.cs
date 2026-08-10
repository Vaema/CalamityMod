using CalamityMod.Items.Weapons.Magic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs;

public class CoralSymbiosis : ModBuff
{
    public override LocalizedText Description => base.Description.WithFormatArgs(CoralSpout.SymbiosisDamageBuff);

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<CoralSpoutPlayer>().Symbiosis = true;
    }
}
