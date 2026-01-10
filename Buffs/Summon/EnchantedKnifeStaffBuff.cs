using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class EnchantedKnifeStaffBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<EnchantedKnifeSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.EnchantedKnifeStaffBool;
    }
}
