using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class MiniPlaguebringerBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<PlaguebringerMK2>();

    protected override ref bool MinionBool => ref BuffModdedOwner.plaguebringerMK2;
}
