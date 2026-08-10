using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class LiliesOfFinalityBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<LiliesOfFinalityElster>();

    protected override ref bool MinionBool => ref BuffModdedOwner.LiliesOfFinalityBool;
}
