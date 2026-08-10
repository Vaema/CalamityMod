using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class Phantom : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<PhantomGuy>();

    protected override ref bool MinionBool => ref BuffModdedOwner.pGuy;
}
