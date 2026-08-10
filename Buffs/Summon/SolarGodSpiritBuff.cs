using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class SolarGodSpiritBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<VengefulSunSpiritMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.vengefulSunMinion;
}
