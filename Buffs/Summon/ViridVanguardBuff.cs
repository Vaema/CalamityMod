using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class ViridVanguardBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<ViridVanguardBlade>();

    protected override ref bool MinionBool => ref BuffModdedOwner.viridVanguard;
}
