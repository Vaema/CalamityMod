using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class MountedScannerBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<MountedScannerSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.mountedScanner;
    }
}
