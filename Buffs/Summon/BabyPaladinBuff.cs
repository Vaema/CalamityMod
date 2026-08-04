using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class BabyPaladinBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<StatigelBlightedSlime>();

        protected override ref bool MinionBool => ref BuffModdedOwner.sGod;
    }
}
