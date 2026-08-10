using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class TundraFlameBlossomsBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<TundraFlameBlossom>();

    protected override ref bool MinionBool => ref BuffModdedOwner.tundraFlameBlossom;
}
