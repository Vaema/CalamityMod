using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs;

public class PhantomicEmpowerment : ModBuff
{
    public override LocalizedText Description => base.Description.WithFormatArgs(PhantomicArtifact.SummonDamageBoost.ToPercent());

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetDamage<SummonDamageClass>() += PhantomicArtifact.SummonDamageBoost;
    }
}
