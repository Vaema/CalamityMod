using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs;

public class MarkedforDeath : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.Calamity().markedForDeath = true;
    }
}
