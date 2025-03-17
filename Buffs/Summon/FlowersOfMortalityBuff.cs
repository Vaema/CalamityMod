using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class FlowersOfMortalityBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<FlowersOfMortalityPetal>();

        protected override ref bool MinionBool => ref BuffModdedOwner.flowersOfMortality;
    }
}
