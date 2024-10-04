using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class FrostbatBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<FrostbatMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.FrostbatBool;
    }
}
