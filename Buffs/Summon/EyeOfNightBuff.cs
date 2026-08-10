using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class EyeOfNightBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<EyeOfNightSummon>();

    protected override ref bool MinionBool => ref BuffModdedOwner.eyeOfNight;
}
