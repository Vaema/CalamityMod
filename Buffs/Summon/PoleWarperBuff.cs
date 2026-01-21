using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class PoleWarperBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<PoleWarperSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.poleWarper;
    }
}
