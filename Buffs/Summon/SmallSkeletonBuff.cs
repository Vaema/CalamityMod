using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SmallSkeletonBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<SmallSkeletonMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.necrosteocytesDudes;
    }
}
