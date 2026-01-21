using CalamityMod.Projectiles.Typeless;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class FungalClumpBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<FungalClumpMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.fClump;
    }
}
