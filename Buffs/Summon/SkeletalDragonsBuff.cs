using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class SkeletalDragonsBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<SkeletalDragonMother>();

    protected override ref bool MinionBool => ref BuffModdedOwner.dragonFamily;
}
