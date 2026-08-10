using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class BrittleStar : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<BrittleStarMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.brittleStar;
}
