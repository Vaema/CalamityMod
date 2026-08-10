using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class CalamarisLamentBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<CalamarisLamentMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.CalamarisLament;
}
