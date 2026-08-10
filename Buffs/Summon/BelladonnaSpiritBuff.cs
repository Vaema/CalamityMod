using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class BelladonnaSpiritBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<BelladonnaSpirit>();

    protected override ref bool MinionBool => ref BuffModdedOwner.belladonaSpirit;
}
