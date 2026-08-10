using CalamityMod.Items.Potions;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Potions;

public class PhotosynthesisBuff : ModBuff
{
    public override LocalizedText Description => base.Description.WithFormatArgs(PhotosynthesisPotion.IncreasedHeartHeal);

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Calamity().photosynthesis = true;
    }
}
