using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class BrimstoneElemental : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<BrimstoneElementalMinion>();

    protected override ref bool MinionBool => ref BuffModdedOwner.brimEleBuff;
}
