using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class EndoHydraBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<EndoHydraBody>();

        protected override ref bool MinionBool => ref BuffModdedOwner.endoHydra;
    }
}
