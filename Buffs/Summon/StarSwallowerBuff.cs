using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class StarSwallowerBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<StarSwallowerSummon>();

        protected override ref bool MinionBool => ref BuffModdedOwner.starSwallowerPetFroge;
    }
}
