using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class IceClasperBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<IceClasperMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.IceClasperBool;
}
