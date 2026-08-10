using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class StellarTorusBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<StellarTorusSummon>();

    protected override ref bool MinionBool => ref BuffModdedOwner.StellarTorus;
}
