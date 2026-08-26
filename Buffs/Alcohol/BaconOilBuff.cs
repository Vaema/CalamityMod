using CalamityMod.DataStructures;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Alcohol;

public class BaconOilBuff : ModBuff
{
    public static DebuffData debuffData = new()
    {
        AlcoholLevel = 3
    };
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = false;
        Main.persistentBuff[Type] = true;
        CalamityBuffSets.DebuffDataset[Type] = debuffData;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Calamity().baconOil = true;
    }
}
