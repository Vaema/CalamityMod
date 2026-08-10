using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class CloudElemental : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<CloudElementalMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.cloudEleBuff;
}
