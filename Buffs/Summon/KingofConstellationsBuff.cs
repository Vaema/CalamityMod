using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class KingofConstellationsBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<BlackDragonHead>();

    protected override ref bool MinionBool => ref BuffModdedOwner.celestialDragons;
}
