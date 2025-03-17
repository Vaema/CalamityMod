using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class FlarebatBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<FlarebatMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.FlarebatBool;
    }
}
