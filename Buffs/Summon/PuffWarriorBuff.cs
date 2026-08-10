using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class PuffWarriorBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<PuffWarrior>();

    protected override ref bool MinionBool => ref BuffModdedOwner.puffWarrior;
}
