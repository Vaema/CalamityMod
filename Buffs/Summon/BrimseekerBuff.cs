using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class BrimseekerBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<DormantBrimseekerBab>();

        protected override ref bool MinionBool => ref BuffModdedOwner.brimseeker;
    }
}
