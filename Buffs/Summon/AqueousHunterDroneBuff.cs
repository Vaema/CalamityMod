using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class AqueousHunterDroneBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<AqueousHunterDroneSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.aqueousHunterDrone;
    }
}
