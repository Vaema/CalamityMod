using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class BlightedSlime : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<BlightedSlimeMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.blightedSlime;
}
