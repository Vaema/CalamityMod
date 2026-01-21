using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class BlackHawkBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<BlackHawkSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.blackhawk;
    }
}
