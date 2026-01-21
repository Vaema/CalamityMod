using CalamityMod.Projectiles.Typeless;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class DemonshadeSetDevilBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<DemonshadeRedDevil>();

        protected override ref bool MinionBool => ref BuffModdedOwner.rDevil;
    }
}
