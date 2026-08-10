using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class CinderBlossomBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<CinderBlossom>();

    protected override ref bool MinionBool => ref BuffModdedOwner.cinderBlossom;
}
