using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class PlantationStaffBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<PlantationStaffSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.PlantationSummon;
    }
}
