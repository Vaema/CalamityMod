using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class EndoCooperBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<EndoCooperBody>();

        protected override ref bool MinionBool => ref BuffModdedOwner.endoCooper;
    }
}
