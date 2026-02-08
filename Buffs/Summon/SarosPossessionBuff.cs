using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SarosPossessionBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<SarosAura>();

        protected override ref bool MinionBool => ref BuffModdedOwner.saros;
    }
}
