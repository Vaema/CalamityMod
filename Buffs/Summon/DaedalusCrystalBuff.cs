using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class DaedalusCrystalBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<DaedalusCrystal>();

    protected override ref bool MinionBool => ref BuffModdedOwner.dCrystal;
}
