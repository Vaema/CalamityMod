using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class AstralProbeBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<AstralProbeSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.astralProbe;
    }
}
