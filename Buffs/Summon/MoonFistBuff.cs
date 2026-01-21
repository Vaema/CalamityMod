using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class MoonFistBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<MoonFist>();

        protected override ref bool MinionBool => ref BuffModdedOwner.MoonFist;
    }
}
