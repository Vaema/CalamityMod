using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class DaedalusGolemBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<DaedalusGolem>();

    protected override ref bool MinionBool => ref BuffModdedOwner.daedalusGolem;
}
