using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SolarSpirit : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<SunSpiritMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.sunSpirit;
    }
}
