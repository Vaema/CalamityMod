using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class HydrothermicVentBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<HydrothermicVent>();

        protected override ref bool MinionBool => ref BuffModdedOwner.cSpirit;
    }
}
