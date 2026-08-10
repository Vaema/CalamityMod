using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Summon;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class ProfanedSoulGuardians : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        CalamityPlayer modPlayer = player.Calamity();
        if (player.ownedProjectileCounts[ModContent.ProjectileType<MiniGuardianAttack>()] <= 0 || modPlayer.profanedCrystalBuffs)
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
        else
            player.buffTime[buffIndex] = 18000;
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        Player player = Main.LocalPlayer;
        if (player.Calamity().profanedCrystal && !player.Calamity().profanedCrystalBuffs)
            tip = this.GetLocalizedValue("VanityDescription");
    }
}
