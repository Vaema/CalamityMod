using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class WulfrumDroidBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<WulfrumDroid>();

    protected override ref bool MinionBool => ref BuffModdedOwner.wDroid;
}
