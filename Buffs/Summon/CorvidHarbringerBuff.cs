using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class CorvidHarbringerBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<PowerfulRaven>();

        protected override ref bool MinionBool => ref BuffModdedOwner.powerfulRaven;
    }
}
