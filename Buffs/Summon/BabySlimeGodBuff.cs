using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class BabySlimeGodBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<CrimsonSlimeGodMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.sGod;
    }
}
