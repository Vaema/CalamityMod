using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class LegionofCelestiaBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<CelestialAxeMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.eAxe;
}
