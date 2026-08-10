using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class DazzlingStabberBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<DazzlingStabber>();

    protected override ref bool MinionBool => ref BuffModdedOwner.providenceStabber;
}
