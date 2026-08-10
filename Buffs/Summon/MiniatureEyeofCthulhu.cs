using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class MiniatureEyeofCthulhu : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<DeathstareEyeball>();

    protected override ref bool MinionBool => ref BuffModdedOwner.deathstareEyeball;
}
