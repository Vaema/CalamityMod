using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class MidnightSunBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<MidnightSunUFO>();

        protected override ref bool MinionBool => ref BuffModdedOwner.midnightUFO;
    }
}
