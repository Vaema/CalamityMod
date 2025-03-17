using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class HermitCrab : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<HermitCrabMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.hCrab;
    }
}
