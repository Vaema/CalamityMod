using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class HauntedDishesBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<HauntedDishes>();

        protected override ref bool MinionBool => ref BuffModdedOwner.hauntedDishes;
    }
}
