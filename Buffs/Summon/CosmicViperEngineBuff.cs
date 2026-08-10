using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class CosmicViperEngineBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<CosmicViperSummon>();

    protected override ref bool MinionBool => ref BuffModdedOwner.cosmicViper;
}
