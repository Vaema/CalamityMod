using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class FrostBlossomBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<FrostBlossom>();

        protected override ref bool MinionBool => ref BuffModdedOwner.frostBlossom;
    }
}
