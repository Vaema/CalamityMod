using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class PerditionBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<PerditionBeacon>();

    protected override ref bool MinionBool => ref BuffModdedOwner.perditionBeacon;
}
