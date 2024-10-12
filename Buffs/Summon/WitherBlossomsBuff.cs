using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class WitherBlossomsBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<WitherBlossom>();

        protected override ref bool MinionBool => ref BuffModdedOwner.witherBlossom;
    }
}
