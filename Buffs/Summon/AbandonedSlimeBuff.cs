using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class AbandonedSlimeBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<AstrageldonSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.aSlime;
    }
}
