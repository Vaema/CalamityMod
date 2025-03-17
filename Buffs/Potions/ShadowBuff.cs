using CalamityMod.Items.Potions;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Potions
{
    public class ShadowBuff : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs(ShadowPotion.StealthRegenBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().shadow = true;
            if (player.yoraiz0rEye < 2 && CalamityClientConfig.Instance.StealthInvisibility)
                player.yoraiz0rEye = 2;
        }
    }
}
