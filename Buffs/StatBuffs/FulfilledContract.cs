using CalamityMod.Items.Accessories;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs;

public class FulfilledContract : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        Main.buffNoTimeDisplay[Type] = false;
    }
}
