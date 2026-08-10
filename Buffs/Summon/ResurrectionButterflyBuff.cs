using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class ResurrectionButterflyBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<PinkButterfly>();

    protected override ref bool MinionBool => ref BuffModdedOwner.resButterfly;
}
