using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class WaterElemental : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<WaterElementalMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.waterEleBuff;
}
