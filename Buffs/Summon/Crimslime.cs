using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class Crimslime : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<CrimslimeMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.cSlime2;
    }
}
