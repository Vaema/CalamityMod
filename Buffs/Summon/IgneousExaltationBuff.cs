using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class IgneousExaltationBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<IgneousBlade>();

    protected override ref bool MinionBool => ref BuffModdedOwner.igneousExaltation;
}
