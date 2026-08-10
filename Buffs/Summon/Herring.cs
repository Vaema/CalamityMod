using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class Herring : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<HerringAI>();

    protected override ref bool MinionBool => ref BuffModdedOwner.herring;
}
