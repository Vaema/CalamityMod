using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class SepulcherMinionBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<SepulcherMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.sepulcher;
}
