using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class SnakeEyesBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<SnakeEyesSummon>();

    protected override ref bool MinionBool => ref BuffModdedOwner.snakeEyes;
}
