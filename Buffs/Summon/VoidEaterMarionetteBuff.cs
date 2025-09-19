using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class VoidEaterMarionetteBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<VoidEaterMarionetteHead>();

        protected override ref bool MinionBool => ref BuffModdedOwner.hasVoidEaterMarionette;
    }
}
