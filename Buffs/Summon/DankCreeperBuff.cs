using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class DankCreeperBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<DankCreeperMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.dCreeper;
    }
}
