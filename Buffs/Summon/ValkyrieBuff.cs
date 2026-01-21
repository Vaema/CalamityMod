using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class ValkyrieBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<Valkyrie>();

        protected override ref bool MinionBool => ref BuffModdedOwner.aValkyrie;
    }
}
