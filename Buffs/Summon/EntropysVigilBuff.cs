using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class EntropysVigilBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<Calamitamini>();

    protected override ref bool MinionBool => ref BuffModdedOwner.cEyes;
}
