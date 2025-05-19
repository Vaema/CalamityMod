using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class CloudyWaifu : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<CloudElementalMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.cWaifu;
    }
}
