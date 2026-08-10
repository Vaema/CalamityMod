using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class AncientMineralSharkBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<ApexShark>();

    protected override ref bool MinionBool => ref BuffModdedOwner.apexShark;
}
