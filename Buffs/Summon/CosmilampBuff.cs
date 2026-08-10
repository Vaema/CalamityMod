using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class CosmilampBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<CosmilampMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.cLamp;
}
