using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class MutatedTruffleBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<MutatedTruffleMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.MutatedTruffleBool;
    }
}
