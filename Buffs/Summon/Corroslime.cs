using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class Corroslime : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<CorroslimeMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.cSlime;
    }
}
