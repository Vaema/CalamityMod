using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class GastricAberrationBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<GastricBelcher>();

        protected override ref bool MinionBool => ref BuffModdedOwner.gastricBelcher;
    }
}
