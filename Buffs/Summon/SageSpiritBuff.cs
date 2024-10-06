using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SageSpiritBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<SageSpirit>();

        protected override ref bool MinionBool => ref BuffModdedOwner.sageSpirit;
    }
}
