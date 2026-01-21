using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SiriusBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<SiriusMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.sirius;
    }
}
