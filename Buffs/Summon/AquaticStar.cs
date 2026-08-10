using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class AquaticStar : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<AquaticStarMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.aquaticStar;
}
