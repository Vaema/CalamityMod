using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class ShellfishBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<Shellfish>();

    protected override ref bool MinionBool => ref BuffModdedOwner.shellfish;
}
