using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class BabyStormlionBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<StormjawBaby>();

        protected override ref bool MinionBool => ref BuffModdedOwner.stormjaw;
    }
}
