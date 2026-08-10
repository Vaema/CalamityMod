using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class Sandnado : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<SandnadoMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.sandnado;
}
