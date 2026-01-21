using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class LilPlaguebringerBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<PlaguebringerSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.plaguebringerPatronSummon;
    }
}
