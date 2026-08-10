using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class SilvaCrystalBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<SilvaCrystal>();

    protected override ref bool MinionBool => ref BuffModdedOwner.sCrystal;
}
