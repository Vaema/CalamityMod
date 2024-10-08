using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class FleshBallBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<FleshBallMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.fleshBall;
    }
}
