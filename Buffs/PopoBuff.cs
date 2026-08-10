using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories.Vanity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs;

public class PopoBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        CalamityPlayer modPlayer = player.Calamity();
        if (player.Transformation().Type != ModContent.ItemType<Popo>())
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
