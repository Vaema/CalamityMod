using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs;

public class PhantomicShield : ModBuff
{
    public override LocalizedText Description => base.Description.WithFormatArgs(PhantomicArtifact.DefenseBoost);

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense += PhantomicArtifact.DefenseBoost;
    }
}
