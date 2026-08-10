using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class HowlTrio : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<HowlsHeartHowl>();

    protected override ref bool MinionBool => ref BuffModdedOwner.howlTrio;
}
