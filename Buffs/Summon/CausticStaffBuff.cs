using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class CausticStaffBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<CausticStaffSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.causticDragon;
    }
}
