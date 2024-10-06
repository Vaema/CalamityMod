using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class ViriliBuff : BaseSummonBuff
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<PlaguePrincess>();

        protected override ref bool MinionBool => ref BuffModdedOwner.virili;
    }
}
