using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class Mechworm : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<MechwormHead>();

        protected override ref bool MinionBool => ref BuffModdedOwner.mWorm;
    }
}
