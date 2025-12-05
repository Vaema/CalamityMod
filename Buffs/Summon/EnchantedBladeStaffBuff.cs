using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class EnchantedBladeStaffBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<EnchantedBladeSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.EnchantedBladeStaffBool;
    }
}
