using CalamityMod.Projectiles.Typeless;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SeaSnailBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<VictideSeaSnail>();

        protected override ref bool MinionBool => ref BuffModdedOwner.victideSnail;
    }
}
