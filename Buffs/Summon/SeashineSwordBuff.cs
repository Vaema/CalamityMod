using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SeashineSwordBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<SeashineSwordProj>();

        protected override ref bool MinionBool => ref BuffModdedOwner.seashineSwordBuff;
    }
}
