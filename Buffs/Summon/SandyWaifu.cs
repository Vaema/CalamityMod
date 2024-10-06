using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SandyWaifu : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<SandElementalMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.sWaifu;
    }
}
