using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class GammaHydraBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<GammaHead>();

    protected override ref bool MinionBool => ref BuffModdedOwner.gammaHead;
}
